[Home](/README.md) / [Docs](/docs/README.md) / [Configuration](/docs/configuration/README.md) / DnsUpdaterEntry

# DnsUpdaterEntry
More to come..

```json
{
  "enabled": true,
  "name": "DNS record 1",
  "nextUpdate": null,
  "type": "FreeDns",
  "updateIntervalSec": 43200,
  "config": {}
}
```

Below is a breakdown of each available property and how to configure them.

| Property | Type | Required | Default | Notes |
| --- | --- | ---- | ---- | --- |
| `enabled` | bool | optional | `false` | Allows enabling / disabling this entry. |
| `name` | string | required | - | A name for the current entry. |
| `nextUpdate` | DateTime | optional | `NULL` | The next time this entry will be updated - `null` values will be updated on application start. |
| `type` | [DnsType](/docs/enums/DnsType.md) | required | `Unspecified` | The type of DNS Entry being configured. |
| `updateIntervalSec` | Int | optional | `43200` | Interval in which to update this DNS Entry. |
| `config` | Dic<string, string> | optional | `{}` | Additional configuration used for the current DNS Entry. |
