using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.Contracts.Responses;
using Lykke.Snow.PriceAlerts.Contract.Api;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Requests;
using Lykke.Snow.PriceAlerts.Contract.Models.Responses;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Snow.PriceAlerts.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PriceAlertsController : ControllerBase, IPriceAlertsApi
    {
        private const int MaxTake = 100;
        private const int DefaultTake = 20;
        private readonly IMapper _mapper;

        private readonly IPriceAlertsService _priceAlertsService;

        public PriceAlertsController(IPriceAlertsService priceAlertsService, IMapper mapper)
        {
            _priceAlertsService = priceAlertsService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetPriceAlertByIdResponse), (int) HttpStatusCode.OK)]
        public async Task<GetPriceAlertByIdResponse> GetByIdAsync([FromRoute] [Required] string id)
        {
            var result = await _priceAlertsService.GetByIdAsync(id);

            var response = new GetPriceAlertByIdResponse();

            if (result.IsSuccess) response.PriceAlert = _mapper.Map<PriceAlertContract>(result.Value);

            if (result.IsFailed) response.ErrorCode = _mapper.Map<PriceAlertErrorCodesContract>(result.Error);

            return response;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ErrorCodeResponse<PriceAlertErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> AddPriceAlertAsync(
            [FromBody] AddPriceAlertRequest request)
        {
            var priceAlert = _mapper.Map<PriceAlert>(request);
            var result = await _priceAlertsService.InsertAsync(priceAlert);

            var response = new ErrorCodeResponse<PriceAlertErrorCodesContract>();

            if (result.IsFailed) response.ErrorCode = _mapper.Map<PriceAlertErrorCodesContract>(result.Error);

            return response;
        }

        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ErrorCodeResponse<PriceAlertErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> UpdateAsync(
            [FromRoute] [Required] string id, [FromBody] UpdatePriceAlertRequest request)
        {
            var priceAlert = _mapper.Map<PriceAlert>(request, opt => opt.Items[nameof(PriceAlert.Id)] = id);

            var result = await _priceAlertsService.UpdateAsync(priceAlert);

            var response = new ErrorCodeResponse<PriceAlertErrorCodesContract>();

            if (result.IsFailed) response.ErrorCode = _mapper.Map<PriceAlertErrorCodesContract>(result.Error);

            return response;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ErrorCodeResponse<PriceAlertErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> CancelAsync(
            [FromRoute] [Required] string id)
        {
            var result = await _priceAlertsService.CancelAsync(id);

            var response = new ErrorCodeResponse<PriceAlertErrorCodesContract>();

            if (result.IsFailed) response.ErrorCode = _mapper.Map<PriceAlertErrorCodesContract>(result.Error);

            return response;
        }

        [HttpDelete("by-product")]
        [ProducesResponseType(typeof(CancelPriceAlertsByProductResponse), (int) HttpStatusCode.OK)]
        public async Task<CancelPriceAlertsByProductResponse> CancelByProductAsync(
            [FromBody] CancelPriceAlertsByProductRequest request)
        {
            var result = await _priceAlertsService.CancelByProductAndAccountAsync(request.ProductId, request.AccountId);

            return new CancelPriceAlertsByProductResponse() {CancelledAlerts = result};
        }

        [HttpGet("by-account-id/{accountId}")]
        [ProducesResponseType(typeof(GetPriceAlertsResponse), (int) HttpStatusCode.OK)]
        public async Task<GetPriceAlertsResponse> GetByAccountIdAsync([FromRoute] [Required] string accountId,
            [FromQuery] GetPriceAlertsRequest request)
        {
            var skip = request.Skip ?? 0;
            var take = Math.Min(request.Take ?? DefaultTake, MaxTake);

            var result =
                await _priceAlertsService.GetByPageAsync(accountId, request.ProductId,
                    _mapper.Map<List<AlertStatusContract>, List<AlertStatus>>(request.Statuses), skip, take);

            var response = new GetPriceAlertsResponse(result.Contents
                    .Select(p => _mapper.Map<PriceAlert, PriceAlertContract>(p))
                    .ToList(),
                result.Start,
                result.Size,
                result.TotalSize);

            return response;
        }

        [HttpPost("active/count")]
        [ProducesResponseType(typeof(GetActivePriceAlertsCountResponse), (int) HttpStatusCode.OK)]
        public async Task<GetActivePriceAlertsCountResponse> GetActiveAlertsCountAsync(
            [FromBody] GetActivePriceAlertsCountRequest request)
        {
            var result = await _priceAlertsService.GetActiveCountAsync(request.Products, request.AccountId);

            return new GetActivePriceAlertsCountResponse()
            {
                ActivePriceAlertsByProduct = result,
            };
        }
    }
}