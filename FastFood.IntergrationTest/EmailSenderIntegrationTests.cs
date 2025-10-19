using Assignment_NET104.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FastFood.IntergrationTest
{
    [TestFixture]
    public class EmailSenderIntegrationTests
    {
        private EmailSettings _emailSettings;
        private EmailSender _emailSender;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _emailSettings = config.GetSection("EmailSettings").Get<EmailSettings>();
            // Kiểm tra email test đã được cấu hình chưa
            Assert.IsNotNull(_emailSettings, "EmailSettings section is missing!");
            Assert.IsNotEmpty(_emailSettings.SmtpServer, "SmtpServer is required!");
            Assert.IsNotEmpty(_emailSettings.Username, "Username is required!");

            _emailSender = new EmailSender(Options.Create(_emailSettings));
        }
        // Gửi email thực tế để kiểm tra tích hợp
        // Nếu không có lỗi thì đã gửi mã thành công
        // Nếu có lỗi thì ném ngoại lệ
        [Test]
        public async Task SendEmailAsync_ShouldSendSuccesfully()
        {
            var toEmail = _emailSettings.SenderEmail; // Gửi đến chính email đã cấu hình để kiểm tra
            var subject = "Integration Test Email";
            var message = "<h1>This is a test email from integration test.</h1>";

            Assert.DoesNotThrowAsync(async () =>
            {
                await _emailSender.SendEmailAsync(toEmail, subject, message);
            }, "Sending email threw an exception.");
        }
        // Kiểm tra xử lý email không hợp lệ
        // Nếu email không hợp lệ thì ném FormatException
        // Nếu ném đúng thì thử nghiệm thành công
        [Test]
        public void SendEmailAsync_InvalidEmail_ShouldThrowException()
        {
            Assert.ThrowsAsync<FormatException>(async () =>
                await _emailSender.SendEmailAsync("invalid-email", "Subject", "Message")
            );
        }

    }
}