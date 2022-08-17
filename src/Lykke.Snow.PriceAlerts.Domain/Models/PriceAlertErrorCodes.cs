namespace Lykke.Snow.PriceAlerts.Domain.Models
{
    public enum PriceAlertErrorCodes
    {
        None,
        AlreadyExists,
        DoesNotExist,
        InvalidId,
        Duplicate,
        CommentTooLong,
        InvalidValidity,
        InvalidProduct,
        InvalidPrice,
    }
}