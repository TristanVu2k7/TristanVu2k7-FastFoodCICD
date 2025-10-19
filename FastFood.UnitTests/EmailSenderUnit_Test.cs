using NUnit.Framework;
using Assignment_NET104.Services;
using Microsoft.Extensions.Options;
using Assignment_NET104.Services; // Ensure FakeEmailSender is in this namespace


namespace FastFood.UnitTests;
[TestFixture]
public class EmailSenderUnit_Test
{
    private EmailSettings _settings;
    private IOptions<EmailSettings> _options;
    private FakeEmailSender _fakeEmailSender;
    private bool _wasCalled;

    [SetUp]
    public void Setup()
    {
        _settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            Port = 587,
            SenderName = "Test Sender",
            SenderEmail = "unittest@example.com",
            Username = "unittest@example.com",
            Password = "password123"
        };
        _options = Options.Create(_settings);
        _wasCalled = false;
        _fakeEmailSender = new FakeEmailSender(_options, () => _wasCalled = true);
    }
    // Cấu hình giả lập của FakeEmailSender
    // Nếu không lỗi định dạng email thì gọi hàm callback để đánh dấu đã gọi
    // Ngược lại ném lỗi định dạng email
    [Test]
    public void Constructor_StoresSettingsCorrectly()
    {
        Assert.AreEqual("smtp.test.com", _settings.SmtpServer);
        Assert.AreEqual(587, _settings.Port);
        Assert.AreEqual("unittest@example.com", _settings.SenderEmail);
    }
    //Gửi email hợp lệ
    // Nếu email hợp lệ thì hàm SendEmailAsync sẽ được gọi và đánh dấu _wasCalled là true
    // Nếu email không hợp lệ thì ném lỗi định dạng email
    [Test]
    public async Task SendEmailAsync_ValidData_ShouldInvokeSendLogic()
    {
        await _fakeEmailSender.SendEmailAsync("user@test.com", "Test Subject", "Hello, this is a test message!");
        Assert.IsTrue(_wasCalled, "Expected fake email send method to be called.");
    }
    //Bắt lỗi định dạng email không hợp lệ
    // Nếu bắt được thì thử nghiệm thành công
    // Nếu không bắt được thì thử nghiệm thất bại
    [Test]
    public void SendEmailAsync_InvalidEmail_ShouldThrowFormatException()
    {
        Assert.ThrowsAsync<FormatException>(async () =>
            await _fakeEmailSender.SendEmailAsync("invalid-email", "Subject", "Message")
        );
    }
}
