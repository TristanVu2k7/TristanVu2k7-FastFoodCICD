using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;

namespace FastFood.UIAutomation
{
    [TestFixture]
    public class LoginUITest
    {
        private const string BaseUrl = "http://localhost:7174";
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            options.AddArgument("--headless"); // chạy ngầm (ẩn trình duyệt)
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [Test]
        public void LoginPage_Should_DisplayTitle()
        {
            try
            {
                // ✅ Sửa lỗi chính tả: "Acoount" → "Account"
                driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

                // Đợi cho đến khi tiêu đề trang được load
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => !string.IsNullOrEmpty(d.Title));

                // ✅ Kiểm tra tiêu đề trang có chứa "Login"
                StringAssert.Contains("Login", driver.Title,
                    $"Expected page title to contain 'Login', but was '{driver.Title}'.");
            }
            catch (WebDriverException ex)
            {
                Assert.Fail($"❌ Could not connect to the web server. " +
                            $"Make sure the MVC app is running at {BaseUrl}. " +
                            $"Exception: {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit(); // dùng Quit() để đóng toàn bộ session WebDriver
        }
    }
}
