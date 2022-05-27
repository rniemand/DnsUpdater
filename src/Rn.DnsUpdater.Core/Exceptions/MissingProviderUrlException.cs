using System.Runtime.Serialization;

namespace Rn.DnsUpdater.Core.Exceptions;

[Serializable]
public class MissingProviderUrlException : Exception
{
  public string Provider { get; set; } = string.Empty;

  public MissingProviderUrlException(string provider)
    : base($"Missing provider URL for {provider}")
  {
    Provider = provider;
  }

  protected MissingProviderUrlException(SerializationInfo info, StreamingContext context)
    : base(info, context)
  { }
}
