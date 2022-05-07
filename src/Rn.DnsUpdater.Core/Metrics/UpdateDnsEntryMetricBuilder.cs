using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Enums;
using Rn.DnsUpdater.Core.Metrics.Interfaces;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Metrics.Builders;
using Rn.NetCore.Metrics.Enums;
using Rn.NetCore.Metrics.Models;

namespace Rn.DnsUpdater.Core.Metrics;

public class NullUpdateDnsEntryMetricBuilder : IUpdateDnsEntryMetricBuilder
{
  public bool IsNullMetricBuilder { get; }

  public NullUpdateDnsEntryMetricBuilder()
  {
    // TODO: [TESTS] (NullUpdateDnsEntryMetricBuilder) Add tests
    IsNullMetricBuilder = true;
  }

  public IUpdateDnsEntryMetricBuilder WithCategory(string category, string subCategory, bool skipToLower = true) => this;
  public IUpdateDnsEntryMetricBuilder ForDnsEntry(DnsUpdaterEntry dnsEntry) => this;
  public IUpdateDnsEntryMetricBuilder WithResponse(HttpResponseMessage response) => this;
  public IMetricTimingToken WithTiming() => new NullMetricTimingToken();

  public void WithException(Exception ex) { }

  public CoreMetric Build() => null;
}

public class UpdateDnsEntryMetricBuilder : MetricBuilderBase, IUpdateDnsEntryMetricBuilder
{
  public bool IsNullMetricBuilder { get; }


  // Constructor
  public UpdateDnsEntryMetricBuilder() 
    : base("client")
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder) Add tests
    IsNullMetricBuilder = false;

    // Set default Tags
    SetTag(Tags.EntryName, MetricPlaceholder.Unknown);
    SetTag(Tags.EntryType, MetricPlaceholder.Unknown);
    SetTag(Tags.Category, MetricPlaceholder.None);
    SetTag(Tags.SubCategory, MetricPlaceholder.None);
    SetTag(Tags.ResponseCode, MetricPlaceholder.Null);

    // Set default Fields
    SetField(Fields.UpdateInterval, 0);
    SetField(Fields.UpdateTimeout, 0);
    SetField(Fields.QueryCount, 1);
    SetField(Fields.ResultsCount, 1);
  }


  // Builder methods
  public IUpdateDnsEntryMetricBuilder WithCategory(string category, string subCategory, bool skipToLower = true)
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithCategory) Add tests
    SetTag(Tags.Category, category, skipToLower);
    SetTag(Tags.SubCategory, subCategory, skipToLower);
    return this;
  }

  public IUpdateDnsEntryMetricBuilder ForDnsEntry(DnsUpdaterEntry dnsEntry)
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.ForDnsEntry) Add tests
    SetTag(Tags.EntryName, dnsEntry.Name.LowerTrim());
    SetTag(Tags.EntryType, dnsEntry.Type.ToString("G"), true);

    SetField(Fields.UpdateInterval, dnsEntry.UpdateIntervalSec);
    SetField(Fields.UpdateTimeout,
      dnsEntry.GetIntConfig(ConfigKeys.TimeoutMs, 0)
    );

    return this;
  }

  public IUpdateDnsEntryMetricBuilder WithResponse(HttpResponseMessage response)
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithResponse) Add tests
    SetTag(Tags.ResponseCode, response.StatusCode.ToString("G"));

    if (!response.IsSuccessStatusCode)
      SetSuccess(false);

    return this;
  }

  public IMetricTimingToken WithTiming()
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithTiming) Add tests
    return new MetricTimingToken(CoreMetric, MetricField.Value);
  }

  public void WithException(Exception ex)
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithException) Add tests
    SetException(ex);
  }


  // Build()
  public CoreMetric Build()
  {
    // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.GetMetric) Add tests
    return CoreMetric;
  }


  // Misc.
  public static class Tags
  {
    public const string EntryName = "entry";
    public const string EntryType = "entry_type";
    public const string Category = "category";
    public const string SubCategory = "sub_category";
    public const string ResponseCode = "response_code";
  }

  public static class Fields
  {
    public const string UpdateInterval = "update_interval";
    public const string UpdateTimeout = "update_timeout";
    public const string QueryCount = "query_count";
    public const string ResultsCount = "results_count";
  }
}
