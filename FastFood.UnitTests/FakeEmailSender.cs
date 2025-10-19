using Assignment_NET104.Services;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace FastFood.UnitTests
{
    public class FakeEmailSender : EmailSender
    {
        // Callback invoked when a send is simulated
        private readonly Action? _onSend;

        public FakeEmailSender(IOptions<EmailSettings> settings, Action? onSend)
            : base(settings)
        {
            _onSend = onSend;
        }

        // Hide the base method (base method is not virtual)
        public new async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Simulate async send delay
            await Task.Delay(10);

            // Simulate invalid email error for testing
            if (string.IsNullOrEmpty(toEmail) || !toEmail.Contains("@"))
                throw new FormatException("Invalid email address");

            _onSend?.Invoke();
        }
    }
}
