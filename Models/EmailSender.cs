using System.Net;
using System.Net.Mail;
using MarkRestaurant.Models;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendConfirmationEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            Credentials = new NetworkCredential(
                _configuration["EmailSettings:SmtpUser"],
                _configuration["EmailSettings:SmtpPass"]
            ),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpUser"]!),
            Subject = subject,
            IsBodyHtml = true,
        };

        string messageBody = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            background-color: #0d1117;
            color: #c9d1d9;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 700px;
            margin: 40px auto;
            background-color: #161b22;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 0 25px rgba(0, 255, 255, 0.15);
            border: 1px solid #00BFA5;
        }}

        h1 {{
            text-align: center;
            color: #00FFF0;
            font-size: 28px;
            letter-spacing: 2px;
            margin-bottom: 30px;
        }}

        p {{
            font-size: 16px;
            text-align: center;
            line-height: 1.6;
            color: #b0bec5;
            margin-top: 0;
            margin-bottom: 20px;
        }}

         a.button {{
                display: inline-block;
                margin: 30px auto 0;
                padding: 14px 30px;
                font-weight: 600;
                font-size: 1.1rem;
                color: #121217;
                background: #00bcd4;
                border-radius: 30px;
                border: none;
                text-decoration: none;
                box-shadow: 0 6px 12px rgba(0, 188, 212, 0.5);
                transition: background-color 0.3s ease, box-shadow 0.3s ease;
                cursor: pointer;
                text-align: center;
            }}

            a.button:hover {{
                background: #00acc1;
                box-shadow: 0 8px 18px rgba(0, 172, 193, 0.8);
            }}

        .footer {{
            margin-top: 40px;
            font-size: 13px;
            color: #546e7a;
            text-align: center;
            font-style: italic;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>MarcDonalds</h1>
        <p>{message}</p>
        <div class='footer'>
            <p>Thank you for choosing MarcDonalds!</p>
            <p>We look forward to serving you.</p>
        </div>
    </div>
</body>
</html>";

        mailMessage.Body = messageBody;
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendForgotPasswordEmailAsync(string email, string resetLink)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            Credentials = new NetworkCredential(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpUser"]!),
            Subject = "Reset Your Password",
            IsBodyHtml = true,
        };

        string messageBody = $@"
    <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #1e1c21;
                    color: #ffffff;
                    margin: 0;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #2c2a2e;
                    border-radius: 8px;
                    padding: 30px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
                }}
                h1 {{
                    color: #00bcd4;
                    text-align: center;
                    text-shadow: 0 0 8px #00bcd4aa;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.5;
                    text-align: center;
                    color: #ffffff;
                    margin: 20px 0;
                }}
                a.reset-link {{
                    display: inline-block;
                    padding: 14px 30px;
                    background-color: #00bcd4;
                    color: #121217;
                    font-weight: bold;
                    text-decoration: none;
                    border-radius: 30px;
                    box-shadow: 0 6px 12px rgba(0, 188, 212, 0.5);
                    transition: background-color 0.3s ease, box-shadow 0.3s ease;
                    cursor: pointer;
                    margin: 20px auto;
                    text-align: center;
                    display: block;
                    width: fit-content;
                }}
                a.reset-link:hover {{
                    background-color: #00acc1;
                    box-shadow: 0 8px 18px rgba(0, 172, 193, 0.8);
                }}
                .footer {{
                    margin-top: 25px;
                    font-size: 14px;
                    text-align: center;
                    color: #b0b0b0;
                    font-style: italic;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Reset Your Password</h1>
                <p>You requested a password reset. Please click the link below to reset your password:</p>
                <a href='{resetLink}' class='reset-link'>Reset Password</a>
                <div class='footer'>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Thank you for choosing MarcDonalds!</p>
                </div>
            </div>
        </body>
    </html>";

        mailMessage.Body = messageBody;
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordChangedEmailAsync(string email)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            Credentials = new NetworkCredential(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpUser"]!),
            Subject = "Your Password Has Been Changed",
            IsBodyHtml = true,
        };

        string messageBody = $@"
    <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #1e1c21;
                    color: #ffffff;
                    margin: 0;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #2c2a2e;
                    border-radius: 8px;
                    padding: 30px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
                    text-align: center;
                }}
                h1 {{
                    color: #00bcd4;
                    text-shadow: 0 0 8px #00bcd4aa;
                    margin-bottom: 25px;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.5;
                    color: #ffffff;
                    margin: 20px 0 0;
                }}
                .footer {{
                    margin-top: 25px;
                    font-size: 14px;
                    color: #b0b0b0;
                    font-style: italic;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Your Password Has Been Changed</h1>
                <p>If you did not request this change, please contact support immediately.</p>
                <div class='footer'>
                    <p>Thank you for choosing MarcDonalds!</p>
                </div>
            </div>
        </body>
    </html>";

        mailMessage.Body = messageBody;
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendReceiptEmailAsync(
        string email,
        string subject,
        string orderNumber,
        string customerName,
        decimal deliveryCost,
        decimal deliveryDistance,
        ICollection<OrderItem> orderItems,
        decimal totalPrice,
        PaymentMethod paymentMethod,
        decimal tipsAmount)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            Credentials = new NetworkCredential(
                _configuration["EmailSettings:SmtpUser"],
                _configuration["EmailSettings:SmtpPass"]
            ),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpUser"]!),
            Subject = subject,
            IsBodyHtml = true,
        };

        // Формируем HTML-список товаров (без количества)
        string orderItemsHtml = string.Empty;
        foreach (var item in orderItems)
        {
            orderItemsHtml += $@"
        <li>
            <span class='order-item-title'>{item.Product!.Title}</span>
            <span class='order-item-price'>{item.Product!.Price:F2} x {item.Quantity} AZN</span>
        </li>";
        }

        // Основное письмо
        string messageBody = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            background-color: #0d1117;
            color: #c9d1d9;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 700px;
            margin: 40px auto;
            background-color: #161b22;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 0 25px rgba(0, 255, 255, 0.15);
            border: 1px solid #00BFA5;
        }}

        h1 {{
            text-align: center;
            color: #00FFF0;
            font-size: 28px;
            letter-spacing: 2px;
            margin-bottom: 30px;
        }}

        p {{
            font-size: 16px;
            text-align: center;
            line-height: 1.6;
            color: #b0bec5;
        }}

        .order-details {{
            margin-top: 30px;
            text-align: center;
        }}

        .order-summary {{
            margin: 30px auto;
            max-width: 580px;
            border: 1px solid #1DE9B6;
            border-radius: 12px;
            overflow: hidden;
        }}

        .order-items-header li,
        .order-items-list li {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 14px 20px;
        }}

        .order-items-header {{
            background-color: #1c222c;
            font-weight: bold;
            text-transform: uppercase;
            color: #00FFF0;
            font-size: 14px;
            border-bottom: 1px solid #00FFF0;
        }}

        .order-items-list {{
            background-color: #0f141b;
        }}

        .order-items-list li {{
            border-bottom: 1px solid #2c3038;
            font-size: 15px;
            color: #e0f7fa;
            transition: background 0.2s ease;
        }}

        .order-items-list li:hover {{
            background-color: #1a1f26;
        }}

        .order-item-title {{
            flex: 1;
            text-align: left;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            padding-right: 20px;
        }}

        .order-item-price {{
            min-width: 80px;
            text-align: right;
            font-weight: bold;
            color: #00FFF0;
        }}

        .fee-line {{
            text-align: center;
            font-size: 15px;
            color: #b2dfdb;
            margin: 4px 0;
        }}

        .total {{
            margin-top: 20px;
            font-size: 20px;
            font-weight: 900;
            color: #1DE9B6;
        }}

        .payment-method {{
            font-size: 16px;
            margin-top: 10px;
            color: #64ffda;
        }}

        .footer {{
            margin-top: 40px;
            font-size: 13px;
            color: #546e7a;
            text-align: center;
            font-style: italic;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Receipt from MarcDonalds</h1>
        <p>Thank you for your order, <strong>{customerName}</strong>!</p>
        <div class='order-details'>
            <p>Order Number: <strong>{orderNumber}</strong></p>
            <p>Here is the summary of your order:</p>
            <div class='order-summary'>
                <ul class='order-items-header'>
                    <li>
                        <span class='order-item-title'>Product</span>
                        <span class='order-item-price'>Price</span>
                    </li>
                </ul>
                <ul class='order-items-list'>
                    {orderItemsHtml}
                </ul>
            </div>

            <p class='fee-line'>Packaging fee: <strong>0,50 AZN</strong></p>
            <p class='fee-line'>Service fee: <strong>0,80 AZN</strong></p>
            <p class='fee-line'>Delivery ({deliveryDistance} km): <strong>{deliveryCost:F2} AZN</strong></p>
            <p class='fee-line'>Tips to courier: {tipsAmount:F2} AZN</p>

            <p class='payment-method'>Payment Method: <strong>{paymentMethod}</strong></p>

            <p class='total'>Total Amount: {totalPrice:F2} AZN</p>
        </div>
        <div class='footer'>
            <p>We hope you enjoyed your meal!</p>
            <p>Looking forward to your next visit to MarcDonalds.</p>
        </div>
    </div>
</body>
</html>";

        mailMessage.Body = messageBody;
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }
    public async Task SendCompletionEmailAsync(string email, string orderNumber, string customerName, DateTime createdAt, DateTime finishedAt)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            Credentials = new NetworkCredential(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpUser"]!),
            Subject = $"Your order {orderNumber} has been delivered!",
            IsBodyHtml = true,
        };

        TimeSpan duration = finishedAt - createdAt;
        string durationText = $"{(int)duration.TotalMinutes} minutes{(duration.Seconds > 0 ? $" {duration.Seconds} seconds" : "")}";

        string messageBody = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            background-color: #0d1117;
            color: #c9d1d9;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 700px;
            margin: 40px auto;
            background-color: #161b22;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 0 25px rgba(0, 255, 255, 0.15);
            border: 1px solid #00BFA5;
            text-align: center;
        }}

        h1 {{
            color: #00FFF0;
            font-size: 28px;
            letter-spacing: 2px;
            margin-bottom: 30px;
        }}

        p {{
            font-size: 16px;
            color: #b0bec5;
            margin: 10px 0;
        }}

        .highlight {{
            color: #00FFF0;
            font-weight: 700;
        }}

        .summary {{
            margin-top: 30px;
            border: 1px solid #1DE9B6;
            border-radius: 12px;
            background-color: #0f141b;
            padding: 20px;
            color: #e0f7fa;
            font-size: 16px;
            line-height: 1.6;
        }}

        .summary p {{
            margin: 8px 0;
        }}

        .footer {{
            margin-top: 40px;
            font-size: 13px;
            color: #546e7a;
            font-style: italic;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Your order has arrived!</h1>
        <p>Dear <span class='highlight'>{customerName}</span>, your order <span class='highlight'>{orderNumber}</span> has been successfully delivered.</p>
        <div class='summary'>
            <p><strong>Ordered at:</strong> {createdAt:HH:mm}</p>
            <p><strong>Delivered at:</strong> {finishedAt:HH:mm}</p>
            <p><strong>Delivery time:</strong> {durationText}</p>
        </div>
        <div class='footer'>
            <p>Bon appétit from the MarcDonalds team!</p>
            <p>We hope to serve you again soon.</p>
        </div>
    </div>
</body>
</html>
";

        mailMessage.Body = messageBody;
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendSupportMessageAsync(string subject, string message, string senderName, string senderEmail)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]!))
        {
            Credentials = new NetworkCredential(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:SmtpFrom"]!),
            Subject = $"[Support] {subject}",
            IsBodyHtml = true,
        };

        string htmlBody = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            background-color: #0d1117;
            color: #c9d1d9;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 700px;
            margin: 40px auto;
            background-color: #161b22;
            padding: 40px;
            border-radius: 16px;
            box-shadow: 0 0 25px rgba(0, 255, 255, 0.15);
            border: 1px solid #00BFA5;
        }}

        h1 {{
            color: #00FFF0;
            font-size: 26px;
            text-align: center;
            margin-bottom: 30px;
        }}

        .info p {{
            font-size: 16px;
            margin: 10px 0;
            color:rgb(102, 162, 156);
        }}

        .label {{
            font-weight: bold;
            color: #80cbc4;
        }}

        .footer {{
            margin-top: 40px;
            font-size: 13px;
            color: #546e7a;
            text-align: center;
            font-style: italic;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>New Support Message</h1>
        <div class='info'>
            <p><span class='label'>Name:</span> {senderName}</p>
            <p><span class='label'>Email:</span> {senderEmail}</p>
            <p><span class='label'>Subject:</span> {subject}</p>
            <p><span class='label'>Message:</span><br/>{message}</p>
        </div>
        <div class='footer'>
            <p>This message was sent from the support form on your site.</p>
        </div>
    </div>
</body>
</html>";

        mailMessage.Body = htmlBody;
        mailMessage.To.Add("marknikulov11@gmail.com");

        await smtpClient.SendMailAsync(mailMessage);
    }

}
