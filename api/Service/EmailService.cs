using System.Text;
using System.Net;
using System.Net.Mail;
using api.Interfaces;

namespace api.Service;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
    {
        var frontendBaseUrl = _configuration["UrlSettings:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            _logger.LogWarning("UrlSettings:FrontendBaseUrl is not configured. Cannot build password reset link.");
            return false;
        }

        var resetLink = $"{frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        var safeEmail = WebUtility.HtmlEncode(email);

        var emailBody = new StringBuilder();
        string htmlBody = ResetPasswordEmailBody(resetLink, safeEmail);
        emailBody.Append(htmlBody);

        var subject = "Reset your BzSound password";
        var plainTextBody = BuildPlainTextPasswordResetBody(resetLink);

        _logger.LogInformation("Sending password reset email to {Email}", email);
        _logger.LogInformation("Password reset link: {ResetLink}", resetLink);
        _logger.LogInformation("Password reset email subject: {Subject}", subject);

        return await SendEmailAsync(
            toEmail: email,
            subject: subject,
            htmlBody: emailBody.ToString(),
            plainTextBody: plainTextBody);
    }

    public async Task<bool> SendEmailConfirmationEmailAsync(string email, string emailConfirmationToken)
    {
        var frontendBaseUrl = _configuration["UrlSettings:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            _logger.LogWarning("UrlSettings:FrontendBaseUrl is not configured. Cannot build email confirmation link.");
            return false;
        }

        var confirmationLink = $"{frontendBaseUrl}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(emailConfirmationToken)}";
        var subject = "Confirm your BzSound email";
        var safeEmail = WebUtility.HtmlEncode(email);
        var htmlBody = EmailConfirmationEmailBody(confirmationLink, safeEmail);
        var plainTextBody = BuildPlainTextEmailConfirmationBody(confirmationLink);

        return await SendEmailAsync(
            toEmail: email,
            subject: subject,
            htmlBody: htmlBody,
            plainTextBody: plainTextBody);
    }

    private async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string plainTextBody)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration.GetValue<int?>("EmailSettings:SmtpPort");
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];
        var enableSsl = _configuration.GetValue("EmailSettings:EnableSsl", true);

        if (string.IsNullOrWhiteSpace(smtpHost) ||
            smtpPort is null ||
            string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning("EmailSettings are missing. Configure SMTP settings in appsettings before sending emails.");
            return false;
        }

        try
        {
            using var smtpClient = new SmtpClient(smtpHost, smtpPort.Value)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName ?? "BzSound"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, "text/plain"));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html"));

            await smtpClient.SendMailAsync(message);
            smtpClient.Dispose();
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }

    private static string BuildPlainTextPasswordResetBody(string resetLink)
    {
        return new StringBuilder()
            .AppendLine("Hello,")
            .AppendLine()
            .AppendLine("We received a request to reset your BzSound account password.")
            .AppendLine("Use the link below to choose a new password:")
            .AppendLine(resetLink)
            .AppendLine()
            .AppendLine("If you did not request a password reset, you can safely ignore this email.")
            .AppendLine()
            .AppendLine("BzSound Team")
            .ToString();
    }

    private static string ResetPasswordEmailBody(string resetLink, string safeEmail)
    {
        return $@"
        <!doctype html>
        <html lang=""en"">
            <head>
                <meta charset=""utf-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
                <title>Password reset</title>
            </head>
            <body style=""margin:0;padding:0;background-color:#f6f8fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:24px 12px;"">
                    <tr>
                        <td align=""center"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;background:#ffffff;border:1px solid #e5e7eb;border-radius:12px;overflow:hidden;"">
                                <tr>
                                    <td style=""padding:24px 24px 8px 24px;font-size:22px;font-weight:700;color:#111827;"">Reset your password</td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 8px 24px;font-size:14px;line-height:1.6;"">
                                        We received a request to reset the password for <strong>{safeEmail}</strong>.
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 24px 8px 24px;font-size:14px;line-height:1.6;"">
                                        Click the button below to choose a new password:
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:12px 24px 16px 24px;"">
                                        <a href=""{resetLink}"" style=""display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;padding:12px 18px;border-radius:8px;font-size:14px;font-weight:600;"">Reset password</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 8px 24px;font-size:13px;line-height:1.6;color:#4b5563;"">
                                        If the button does not work, copy and paste this link into your browser:
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 16px 24px;word-break:break-all;font-size:12px;line-height:1.5;color:#1d4ed8;"">{resetLink}</td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 24px 24px;font-size:12px;line-height:1.6;color:#6b7280;"">
                                        This link is time-sensitive. If you did not request a password reset, you can safely ignore this email.
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
        </html>";
    }

    private static string BuildPlainTextEmailConfirmationBody(string confirmationLink)
    {
        return new StringBuilder()
            .AppendLine("Hello,")
            .AppendLine()
            .AppendLine("Welcome to BzSound! Please confirm your email address using the link below:")
            .AppendLine(confirmationLink)
            .AppendLine()
            .AppendLine("If you did not create this account, you can ignore this email.")
            .AppendLine()
            .AppendLine("BzSound Team")
            .ToString();
    }

    private static string EmailConfirmationEmailBody(string confirmationLink, string safeEmail)
    {
        return $@"
        <!doctype html>
        <html lang=""en"">
            <head>
                <meta charset=""utf-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
                <title>Email confirmation</title>
            </head>
            <body style=""margin:0;padding:0;background-color:#f6f8fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:24px 12px;"">
                    <tr>
                        <td align=""center"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;background:#ffffff;border:1px solid #e5e7eb;border-radius:12px;overflow:hidden;"">
                                <tr>
                                    <td style=""padding:24px 24px 8px 24px;font-size:22px;font-weight:700;color:#111827;"">Confirm your email</td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 8px 24px;font-size:14px;line-height:1.6;"">
                                        Welcome to BzSound! Please confirm the email address for <strong>{safeEmail}</strong>.
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 24px 8px 24px;font-size:14px;line-height:1.6;"">
                                        Click the button below to confirm your account:
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:12px 24px 16px 24px;"">
                                        <a href=""{confirmationLink}"" style=""display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;padding:12px 18px;border-radius:8px;font-size:14px;font-weight:600;"">Confirm email</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 8px 24px;font-size:13px;line-height:1.6;color:#4b5563;"">
                                        If the button does not work, copy and paste this link into your browser:
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:0 24px 16px 24px;word-break:break-all;font-size:12px;line-height:1.5;color:#1d4ed8;"">{confirmationLink}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
        </html>";
    }
}