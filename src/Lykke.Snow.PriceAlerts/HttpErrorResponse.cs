namespace Lykke.Snow.PriceAlerts
{
    public class HttpErrorResponse
    {
        public string ErrorMessage { get; set; }

        public override string ToString() => ErrorMessage;
    }
}