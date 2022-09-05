# Lykke.Snow.PriceAlerts

The Price alerts service allows users to receive custom notifications when an asset's price crosses a certain value.

## General configuration

```json
{
  "PriceAlerts": 
  {
    "Db": 
    {
      "ConnectionString": "db_connection_string"
    },
    "Cqrs": 
    {
      "ConnectionString": "rabbitmq_connection_string",
      "RetryDelay": "00:00:02",
      "EnvironmentName": "dev"
    },
    "AssetService": 
    {
      "Url": "http://mt-asset-service.mt.svc.cluster.local",
      "ApiKey": ""
    },
    "TradingCore": 
    {
      "Url": "http://mt-trading-core.mt.svc.cluster.local",
      "ApiKey": ""
    },
    "PriceAlertsClient": 
    {
      "Url": "http://price-alerts.mt.svc.cluster.local",
      "ApiKey": ""
    },
    "RabbitMq": 
    {
      "Consumers": 
      {
        "QuotesRabbitMqSettings": 
        {
          "ConnectionString": "rabbitmq_connection_string",
          "ExchangeName": "lykke.mt.pricefeed"
        }
      },
      "Publishers": 
      {
      }
    },
    "MeteorService": 
    {
      "Url": "http://meteor.mt.svc.cluster.local:5026"
    },
    "OidcSettings": {
        "ApiAuthority": "http://bouncer.mt.svc.cluster.local:5005",
        "ClientId": "price_alerts_api",
        "ClientSecret": "",
        "ClientScope": "meteor_api meteor_api:server",
        "ValidateIssuerName": false,
        "RequireHttps": false,
        "RenewTokenTimeoutSec": 1800
    },
    "MeteorMessageExpiration": "1.00:00:00"
  }
}
```

`QuotesRabbitMqSettings` describes a queue that is connected to the best price exchange of MT Core service.

*MeteorMessageExpiration* is responsible for expiration of triggered price alert notifications on FE.

Bouncer must be configured to allow access for price alerts *clientId* with client scopes for meteor: `"meteor_api meteor_api:server"`