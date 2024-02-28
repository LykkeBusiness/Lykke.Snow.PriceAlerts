## 1.6.0 - Nova 2. Delivery 40 (February 28, 2024)
### What's changed
* LT-5281: Step: deprecated packages validation is failed.
* LT-5254: Update lykke.httpclientgenerator to 5.6.2.




## 1.5.0 - Nova 2. Delivery 39 (January 30, 2024)
### What's changed
* LT-5146: Changelog.md for lykke.snow.pricealerts.
* LT-4494: Read secrets from .json with hyphens.

### Deployment

User secrets will be read from .json configuration file. 

OidcSettings section looks the following way:

```json
"OidcSettings": 
    {
      "Api-Authority": "http://bouncer.mt.svc.cluster.local:5005",
      "Client-Id": "price_alerts_api",
      "Client-Secret": "secret",
      "Client-Scope": "meteor_api meteor_api:server",
      "Validate-Issuer-Name": false,
      "Require-Https": false,
      "Renew-Token-Timeout-Sec": 1800
    }
```

Hyphen-styled naming is optional. First of all Pascal-case-styled property (e.g. `ApiAuthority`) names will be checked. If not found, we'll fall back to hyphen-styled property names.



## 1.4.0 - Nova 2. Delivery 36 (2023-08-31)
### What's changed
* LT-4896: Update nugets.


**Full change log**: https://github.com/lykkebusiness/lykke.snow.pricealerts/compare/v1.3.0...v1.4.0


## 1.3.0 - Nova 2. Delivery 35 (2023-07-12)
### What's changed
* LT-4847: Bump lykke.snow.common -> 2.7.3.


**Full change log**: https://github.com/lykkebusiness/lykke.snow.pricealerts/compare/v1.2.1...v1.3.0


## 1.2.1 - Nova 2. Delivery 34 (2023-06-05)
### What's Changed

* LT-4737: [PriceAlerts] upgrade Lykke.MarginTrading.AssetService.Contracts

**Full Changelog**: https://github.com/LykkeBusiness/Lykke.Snow.PriceAlerts/compare/v1.2.0...v1.2.1


## 1.2.0 - Nova 2. Delivery 32 (2023-03-01)
### What's Changed

* LT-4407: Do not let the host keep running if startupmanager failed to start.
* LT-4380: Validaterskipandtake implementation replace.


**Full change log**: https://github.com/lykkebusiness/lykke.snow.pricealerts/compare/v1.0.3...v1.2.0
