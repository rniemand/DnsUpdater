using System;
using System.Net.Http;
using Rn.DnsUpdater.Core.Config;
using Rn.NetCore.Metrics.Builders;
using Rn.NetCore.Metrics.Models;

namespace Rn.DnsUpdater.Metrics;

public interface IUpdateDnsEntryMetricBuilder : IMetricBuilder
{
  IUpdateDnsEntryMetricBuilder WithCategory(string category, string subCategory, bool skipToLower = true);
  IUpdateDnsEntryMetricBuilder ForDnsEntry(DnsUpdaterEntry dnsEntry);
  IUpdateDnsEntryMetricBuilder WithResponse(HttpResponseMessage response);
  IMetricTimingToken WithTiming();
  void WithException(Exception ex);
}