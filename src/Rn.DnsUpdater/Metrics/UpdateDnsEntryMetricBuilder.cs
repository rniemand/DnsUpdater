using System;
using System.Net.Http;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Common.Metrics.Builders;
using Rn.NetCore.Common.Metrics.Enums;
using Rn.NetCore.Common.Metrics.Models;

namespace Rn.DnsUpdater.Metrics
{
  public class UpdateDnsEntryMetricBuilder
  {
    private readonly MetricBuilder _builder;

    public class Tags
    {
      public const string EntryName = "entry";
      public const string EntryType = "entry_type";
      public const string Category = "category";
      public const string SubCategory = "sub_category";
      public const string ResponseCode = "response_code";
    }

    public class Fields
    {
      public const string UpdateInterval = "update_interval";
      public const string UpdateTimeout = "update_timeout";
    }

    // Constructor
    public UpdateDnsEntryMetricBuilder()
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder) Add tests
      _builder = new MetricBuilder(MetricSource.Client)
        // Tags
        .WithTag(Tags.EntryName, MetricPlaceholder.Unknown)
        .WithTag(Tags.EntryType, MetricPlaceholder.Unknown)
        .WithTag(Tags.Category, MetricPlaceholder.None)
        .WithTag(Tags.SubCategory, MetricPlaceholder.None)
        .WithTag(Tags.ResponseCode, MetricPlaceholder.Null)
        // Fields
        .WithField(Fields.UpdateInterval, 0)
        .WithField(Fields.UpdateTimeout, 0)
        .WithField(CronMetricBuilder.Fields.QueryCount, 1)
        .WithField(CronMetricBuilder.Fields.ResultsCount, 1);
    }


    // Builder methods
    public UpdateDnsEntryMetricBuilder WithCategory(string category, string subCategory, bool skipToLower = true)
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithCategory) Add tests
      _builder
        .WithTag(Tags.Category, category, skipToLower)
        .WithTag(Tags.SubCategory, subCategory, skipToLower);

      return this;
    }

    public UpdateDnsEntryMetricBuilder ForDnsEntry(DnsUpdaterEntry dnsEntry)
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.ForDnsEntry) Add tests
      _builder
        .WithTag(Tags.EntryName, dnsEntry.Name.LowerTrim())
        .WithTag(Tags.EntryType, dnsEntry.Type.ToString("G"), true)
        .WithField(Fields.UpdateInterval, dnsEntry.UpdateIntervalSec)
        .WithField(Fields.UpdateTimeout, dnsEntry.GetIntConfig(ConfigKeys.TimeoutMs, 0));

      return this;
    }

    public UpdateDnsEntryMetricBuilder WithResponse(HttpResponseMessage response)
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithResponse) Add tests
      _builder
        .WithTag(Tags.ResponseCode, response.StatusCode.ToString("G"));

      if (!response.IsSuccessStatusCode)
        _builder.MarkFailed();

      return this;
    }

    public IMetricTimingToken WithTiming()
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithTiming) Add tests
      return _builder.WithTiming();
    }

    public void WithException(Exception ex)
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.WithException) Add tests
      _builder.WithException(ex);
    }


    // Build()
    public MetricBuilder Build()
    {
      // TODO: [TESTS] (UpdateDnsEntryMetricBuilder.Build) Add tests
      return _builder;
    }
  }
}
