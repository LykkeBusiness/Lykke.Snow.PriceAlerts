# Lykke.Snow.PriceAlerts

The Price alerts service allows users to receive custom notifications when an asset's price crosses a certain value.

## General configuration

<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./template.json) -->
<!-- MARKDOWN-AUTO-DOCS:END -->

`QuotesRabbitMqSettings` describes a queue that is connected to the best price exchange of MT Core service.

*MeteorMessageExpiration* is responsible for expiration of triggered price alert notifications on FE.

Bouncer must be configured to allow access for price alerts *clientId* with client scopes for meteor: `"meteor_api meteor_api:server"`
