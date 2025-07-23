using MarkRestaurant.Models;

public interface IEmailSender
{
    Task SendConfirmationEmailAsync(string email, string subject, string message);
    Task SendForgotPasswordEmailAsync(string email, string resetLink);
    Task SendPasswordChangedEmailAsync(string email);
    Task SendReceiptEmailAsync(
        string email,
        string subject,
        string orderNumber,
        string customerName,
        decimal deliveryCost,
        decimal deliveryDistance,
        ICollection<OrderItem> orderItems,
        decimal totalPrice,
        PaymentMethod paymentMethod,
        decimal tipsAmount);
    Task SendCompletionEmailAsync(string email, string orderNumber, string customerName, DateTime createdAt, DateTime finishedAt);
    Task SendSupportMessageAsync(string subject, string message, string senderName, string senderEmail);
}
