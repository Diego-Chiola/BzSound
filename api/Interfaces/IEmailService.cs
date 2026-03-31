namespace api.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
    Task<bool> SendEmailConfirmationEmailAsync(string email, string emailConfirmationToken);
}