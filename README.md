# Lykke.Snow.PriceAlerts

The Price alerts service allows users to receive custom notifications when an asset's price crosses a certain value.

## General configuration

<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./template.json) -->
<!-- The below code snippet is automatically added from ./template.json -->
```json
{
  "API_AUTHORITY": "String",
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "CLIENT_ID": "String",
  "CLIENT_SCOPE": "String",
  "CLIENT_SECRET": "String",
  "ENVIRONMENT": "String",
  "ENV_INFO": "String",
  "PriceAlerts": {
    "AssetService": {
      "ApiKey": "String",
      "Url": "String"
    },
    "Cqrs": {
      "ConnectionString": "String",
      "EnvironmentName": "String",
      "RetryDelay": "DateTime"
    },
    "Db": {
      "ConnectionString": "String"
    },
    "MeteorService": {
      "Url": "String"
    },
    "OidcSettings": {
      "Api-Authority": "String",
      "Client-Id": "String",
      "Client-Scope": "String",
      "Client-Secret": "String",
      "Renew-Token-Timeout-Sec": "Integer",
      "Require-Https": "Boolean",
      "Validate-Issuer-Name": "Boolean"
    },
    "PriceAlertsClient": {
      "ApiKey": "String",
      "Url": "String"
    },
    "RabbitMq": {
      "Consumers": {
        "QuotesRabbitMqSettings": {
          "ConnectionString": "String",
          "ExchangeName": "String"
        }
      },
      "Publishers": {}
    },
    "TradingCore": {
      "ApiKey": "String",
      "Url": "String"
    }
  },
  "RENEW_TOKEN_TIMEOUT_SEC": "Integer",
  "REQUIRE_HTTPS": "Boolean",
  "serilog": {
    "Enrich": [
      "String"
    ],
    "minimumLevel": {
      "default": "String"
    },
    "Properties": {
      "Application": "String"
    },
    "Using": [
      "String"
    ],
    "writeTo": [
      {
        "Args": {
          "configure": [
            {
              "Args": {
                "outputTemplate": "String"
              },
              "Name": "String"
            }
          ]
        },
        "Name": "String"
      }
    ]
  },
  "TZ": "String",
  "VALIDATE_ISSUER_NAME": "Boolean"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->

`QuotesRabbitMqSettings` describes a queue that is connected to the best price exchange of MT Core service.

*MeteorMessageExpiration* is responsible for expiration of triggered price alert notifications on FE.

Bouncer must be configured to allow access for price alerts *clientId* with client scopes for meteor: `"meteor_api meteor_api:server"`
