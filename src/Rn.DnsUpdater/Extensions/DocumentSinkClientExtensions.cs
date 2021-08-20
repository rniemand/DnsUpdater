using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentSink.ClientLib;
using DocumentSink.ClientLib.Enums;
using DocumentSink.ClientLib.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Extensions
{
  internal class LogValuesFormatter
  {
    private const string NullValue = "(null)";
    private static readonly char[] FormatDelimiters = { ',', ':' };
    private readonly string _format;
    private readonly List<string> _valueNames = new List<string>();

    public LogValuesFormatter(string format)
    {
      OriginalFormat = format;

      var sb = new StringBuilder();
      int scanIndex = 0;
      int endIndex = format.Length;

      while (scanIndex < endIndex)
      {
        int openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
        int closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

        if (closeBraceIndex == endIndex)
        {
          sb.Append(format, scanIndex, endIndex - scanIndex);
          scanIndex = endIndex;
        }
        else
        {
          // Format item syntax : { index[,alignment][ :formatString] }.
          int formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

          sb.Append(format, scanIndex, openBraceIndex - scanIndex + 1);
          sb.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));
          _valueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
          sb.Append(format, formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1);

          scanIndex = closeBraceIndex + 1;
        }
      }

      _format = sb.ToString();
    }

    public string OriginalFormat { get; private set; }
    public List<string> ValueNames => _valueNames;

    private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
    {
      // Example: {{prefix{{{Argument}}}suffix}}.
      int braceIndex = endIndex;
      int scanIndex = startIndex;
      int braceOccurrenceCount = 0;

      while (scanIndex < endIndex)
      {
        if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
        {
          if (braceOccurrenceCount % 2 == 0)
          {
            // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
            braceOccurrenceCount = 0;
            braceIndex = endIndex;
          }
          else
          {
            // An unescaped '{' or '}' found.
            break;
          }
        }
        else if (format[scanIndex] == brace)
        {
          if (brace == '}')
          {
            if (braceOccurrenceCount == 0)
            {
              // For '}' pick the first occurrence.
              braceIndex = scanIndex;
            }
          }
          else
          {
            // For '{' pick the last occurrence.
            braceIndex = scanIndex;
          }

          braceOccurrenceCount++;
        }

        scanIndex++;
      }

      return braceIndex;
    }

    private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
    {
      int findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
      return findIndex == -1 ? endIndex : findIndex;
    }

    public string Format(object[] values)
    {
      if (values != null)
      {
        for (int i = 0; i < values.Length; i++)
        {
          values[i] = FormatArgument(values[i]);
        }
      }

      return string.Format(CultureInfo.InvariantCulture, _format, values ?? Array.Empty<object>());
    }

    internal string Format()
    {
      return _format;
    }

    internal string Format(object arg0)
    {
      return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0));
    }

    internal string Format(object arg0, object arg1)
    {
      return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1));
    }

    internal string Format(object arg0, object arg1, object arg2)
    {
      return string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1), FormatArgument(arg2));
    }

    public KeyValuePair<string, object> GetValue(object[] values, int index)
    {
      if (index < 0 || index > _valueNames.Count)
      {
        throw new IndexOutOfRangeException(nameof(index));
      }

      if (_valueNames.Count > index)
      {
        return new KeyValuePair<string, object>(_valueNames[index], values[index]);
      }

      return new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
    }

    public IEnumerable<KeyValuePair<string, object>> GetValues(object[] values)
    {
      var valueArray = new KeyValuePair<string, object>[values.Length + 1];
      for (int index = 0; index != _valueNames.Count; ++index)
      {
        valueArray[index] = new KeyValuePair<string, object>(_valueNames[index], values[index]);
      }

      valueArray[valueArray.Length - 1] = new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
      return valueArray;
    }

    private object FormatArgument(object value)
    {
      if (value == null)
      {
        return NullValue;
      }

      // since 'string' implements IEnumerable, special case it
      if (value is string)
      {
        return value;
      }

      // if the value implements IEnumerable, build a comma separated string.
      var enumerable = value as IEnumerable;
      if (enumerable != null)
      {
        return string.Join(", ", enumerable.Cast<object>().Select(o => o ?? NullValue));
      }

      return value;
    }

  }

  internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object>>
  {
    internal const int MaxCachedFormatters = 1024;
    private const string NullFormat = "[null]";
    private static int _count;
    private static ConcurrentDictionary<string, LogValuesFormatter> _formatters = new ConcurrentDictionary<string, LogValuesFormatter>();
    private readonly LogValuesFormatter _formatter;
    private readonly object[] _values;
    private readonly string _originalMessage;

    // for testing purposes
    internal LogValuesFormatter Formatter => _formatter;

    public FormattedLogValues(string format, params object[] values)
    {
      if (values != null && values.Length != 0 && format != null)
      {
        if (_count >= MaxCachedFormatters)
        {
          if (!_formatters.TryGetValue(format, out _formatter))
          {
            _formatter = new LogValuesFormatter(format);
          }
        }
        else
        {
          _formatter = _formatters.GetOrAdd(format, f =>
          {
            Interlocked.Increment(ref _count);
            return new LogValuesFormatter(f);
          });
        }
      }
      else
      {
        _formatter = null;
      }

      _originalMessage = format ?? NullFormat;
      _values = values;
    }

    public KeyValuePair<string, object> this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException(nameof(index));
        }

        if (index == Count - 1)
        {
          return new KeyValuePair<string, object>("{OriginalFormat}", _originalMessage);
        }

        return _formatter.GetValue(_values, index);
      }
    }

    public int Count
    {
      get
      {
        if (_formatter == null)
        {
          return 1;
        }

        return _formatter.ValueNames.Count + 1;
      }
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      for (int i = 0; i < Count; ++i)
      {
        yield return this[i];
      }
    }

    public override string ToString()
    {
      if (_formatter == null)
      {
        return _originalMessage;
      }

      return _formatter.Format(_values);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
  
  public class DocumentSinkLoggerAdapter<T> : ILoggerAdapter<T>
  {
    private readonly IDocumentSinkClient _documentSink;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly ILogger<T> _logger;
    private readonly string _hostName;
    private bool _useBaseLogger;
    private int _errorCount;

    public DocumentSinkLoggerAdapter(IServiceProvider serviceProvider)
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.DocumentSinkLoggerAdapter) Add tests
      _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();
      _documentSink = serviceProvider.GetService<IDocumentSinkClient>();
      _useBaseLogger = false;
      _errorCount = 0;

      // Try to set the Host Name
      var environment = serviceProvider.GetService<IEnvironmentAbstraction>();
      if (environment != null)
        _hostName = environment.MachineName.UpperTrim();

      if (_documentSink == null)
      {
        _useBaseLogger = true;
        _logger = serviceProvider.GetRequiredService<ILogger<T>>();
      }
    }

    public void Trace(string message, params object[] args)
      => Log(LogSeverity.Trace, message, args);

    public void Debug(string message, params object[] args)
      => Log(LogSeverity.Debug, message, args);

    public void Info(string message, params object[] args)
      => Log(LogSeverity.Information, message, args);

    public void Warning(string message, params object[] args)
      => Log(LogSeverity.Warning, message, args);

    public void Error(string message, params object[] args)
      => Log(LogSeverity.Error, message, args);

    // TODO: [REVISE] (DocumentSinkLoggerAdapter.Error) Handle "Exception"
    public void Error(Exception ex, string message, params object[] args)
      => Log(LogSeverity.Error, message, args);

    // Internal methods
    private void Log(LogSeverity severity, string message, params object[] args)
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.Log) Add tests
      if (_useBaseLogger)
      {
        BaseLoggerLog(severity, message, args);
        return;
      }

      // Generate the message to send
      try
      {
        var formatter = new FormattedLogValues(message, args);
        var entry = new LogFileLine(severity)
        {
          EntryDate = _dateTime.UtcNow,
          Message = formatter.ToString()
        };

        // Handle special placeholders
        foreach (var formattedLogValue in formatter)
          ExtendMessage(entry, formattedLogValue);

        // Set instance specific properties
        entry.Logger = GetCallingLogger(); 
        entry.Method = GetCallingMethod();
        entry.Host = _hostName;

        Task.Run(() => _documentSink.Ingest(entry));
      }
      catch
      {
        _errorCount++;
        if (_errorCount >= 5)
          _useBaseLogger = true;
      }
    }

    private static string GetCallingMethod()
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.GetCallingMethod) Add tests
      var stackTrace = new StackTrace();
      var stackFrame = stackTrace.GetFrame(3);
      if (stackFrame == null)
        return string.Empty;

      var methodBase = stackFrame.GetMethod();
      if (methodBase == null)
        return string.Empty;

      var methodName = methodBase.Name;
      var typeName = methodBase.ReflectedType?.FullName;

      if (!methodName.IgnoreCaseEquals("MoveNext"))
        return methodName;

      const string rxp1 = "(.*?)\\+<([^>]+)>.*";
      if (!typeName.MatchesRegex(rxp1))
        return methodName;

      var match = typeName.GetRegexMatch(rxp1);
      return match.Groups[2].Value;
    }

    private static string GetCallingLogger()
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.GetCallingMethod) Add tests
      var stackTrace = new StackTrace();
      var stackFrame = stackTrace.GetFrame(3);
      if (stackFrame == null)
        return string.Empty;

      var methodBase = stackFrame.GetMethod();
      if (methodBase == null)
        return string.Empty;

      var methodName = methodBase.Name;
      var typeName = methodBase.ReflectedType?.FullName;

      if (!methodName.IgnoreCaseEquals("MoveNext"))
        return typeName;

      const string rxp1 = "(.*?)\\+<([^>]+)>.*";
      if (!typeName.MatchesRegex(rxp1))
        return methodName;

      var match = typeName.GetRegexMatch(rxp1);
      return match.Groups[1].Value;
    }

    private static void ExtendMessage(LogFileLine entry, KeyValuePair<string, object> kvPair)
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.ExtendMessage) Add tests
      var (key, value) = kvPair;

      if (key.IgnoreCaseEquals(LogLineField.Category.ToString("G")))
      {
        entry.Category = CastAsString(value);
      }
      else
      {
        // swallow
      }
    }

    private void BaseLoggerLog(LogSeverity severity, string message, params object[] args)
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.BaseLoggerLog) Add tests
      switch (severity)
      {
        case LogSeverity.Unknown:
        case LogSeverity.Verbose:
        case LogSeverity.Trace:
          _logger.LogTrace(message, args);
          return;

        case LogSeverity.Debug:
          _logger.LogDebug(message, args);
          return;

        case LogSeverity.Information:
          _logger.LogInformation(message, args);
          return;

        case LogSeverity.Warning:
          _logger.LogWarning(message, args);
          return;

        case LogSeverity.Error:
          _logger.LogError(message, args);
          return;

        case LogSeverity.Fatal:
          _logger.LogError($"FATAL: {message}", args);
          return;

        default:
          throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
      }
    }

    private static string CastAsString(object obj)
    {
      // TODO: [TESTS] (DocumentSinkLoggerAdapter.CastAsString) Add tests
      return obj switch
      {
        int i => i.ToString("D"),
        string s => s,
        DateTime d => d.ToString("u"),
        long l => l.ToString("D"),
        float f => f.ToString("N"),
        _ => obj.ToString()
      };
    }
  }
}
