namespace Lykke.Snow.PriceAlerts.Contract.Models.Responses
{
    public class ErrorCodeResponse<T>
    {
        /// <summary>
        ///     Error code
        /// </summary>
        public T ErrorCode { get; set; }
    }
}