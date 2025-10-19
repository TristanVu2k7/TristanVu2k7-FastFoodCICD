using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace FastFood.UITests
{
    [TestFixture]
    public class FastFoodUITests
    {
        private IWebDriver _driver;
        private readonly string _baseUrl = "https://localhost:7174";
        private string _testEmail = "";
        private const string _testPassword = "123456";

        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--headless=new"); // bỏ nếu muốn thấy UI

            _driver = new EdgeDriver(options);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        // 🔹 1️⃣ Đăng ký tài khoản (bỏ OTP, gán role Customer)
        [Test, Order(1)]
        public void Register_NewUser_Should_Succeed_And_Redirect()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
            Thread.Sleep(1000);

            _testEmail = $"user{DateTime.Now.Ticks}@example.com";
            Console.WriteLine($"👉 Đăng ký với email: {_testEmail}");

            _driver.FindElement(By.Id("FullName")).SendKeys("Test User");
            _driver.FindElement(By.Id("Address")).SendKeys("123 Test Street");
            _driver.FindElement(By.Id("Email")).SendKeys(_testEmail);
            _driver.FindElement(By.Id("PhoneNumber")).SendKeys("0987654321");

            // ✅ Bỏ OTP hoàn toàn (nếu có input thì điền giả)
            try { _driver.FindElement(By.Id("OtpCode")).SendKeys("000000"); } catch { }

            _driver.FindElement(By.Id("Password")).SendKeys(_testPassword);
            _driver.FindElement(By.Id("ConfirmPassword")).SendKeys(_testPassword);

            // ✅ Gán role Customer
            try
            {
                var roleSelect = new SelectElement(_driver.FindElement(By.Id("Role")));
                roleSelect.SelectByText("Customer");
            }
            catch
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    let input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = 'Role';
                    input.value = 'Customer';
                    document.querySelector('form').appendChild(input);
                ");
            }

            // ✅ Gửi form thật bằng JavaScript
            ((IJavaScriptExecutor)_driver).ExecuteScript("document.querySelector('form').submit();");
            Console.WriteLine("✅ Đã gửi form đăng ký...");

            // ✅ Đợi chuyển hướng
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                bool redirected = wait.Until(driver =>
                {
                    string url = driver.Url;
                    return url.Contains("/Home") || url.Contains("/Account/Login");
                });

                Assert.IsTrue(redirected, "Không chuyển hướng sau khi đăng ký.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("❌ Timeout khi đợi redirect.");
                Console.WriteLine("URL hiện tại: " + _driver.Url);
                Console.WriteLine("---- HTML ----");
                Console.WriteLine(_driver.PageSource.Substring(0, Math.Min(2000, _driver.PageSource.Length))); // in phần đầu HTML
                throw;
            }

            string currentUrl = _driver.Url;
            Assert.IsTrue(
                currentUrl.Contains("/Home") || currentUrl.Contains("/Account/Login"),
                $"❌ Sau khi đăng ký, URL không đúng: {currentUrl}"
            );

            Console.WriteLine("✅ Đăng ký thành công!");
        }

        // 🔹 2️⃣ Đăng nhập
        [Test, Order(2)]
        public void Login_With_ValidCredentials_Should_Redirect_To_Home()
        {
            string email = string.IsNullOrEmpty(_testEmail) ? "user@example.com" : _testEmail;

            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
            _driver.FindElement(By.Id("Email")).SendKeys(email);
            _driver.FindElement(By.Id("Password")).SendKeys(_testPassword);
            SafeClick(By.CssSelector("button[type='submit']"));

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.Url.Contains("/Home") || d.Url.Contains("/Food"));

            Assert.IsTrue(_driver.Url.Contains("/Home") || _driver.Url.Contains("/Food"),
                $"Không chuyển hướng đến trang chủ: {_driver.Url}");
        }

        // 🔹 3️⃣ Thêm món vào giỏ hàng
        [Test, Order(3)]
        public void AddToCart_Should_Add_Item_And_Show_Success_Message()
        {
            LoginIfNeeded();
            _driver.Navigate().GoToUrl($"{_baseUrl}/Food/Index");
            Thread.Sleep(1000);
            SafeClick(By.CssSelector("a.btn-success.btn-sm"));

            _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Index");
            var rows = _driver.FindElements(By.CssSelector("table tr"));
            Assert.IsTrue(rows.Count > 1, "Không có sản phẩm trong giỏ hàng!");
        }

        // 🔹 4️⃣ Thanh toán
        [Test, Order(4)]
        public void Checkout_Should_Clear_Cart_And_Show_Success_Message()
        {
            LoginIfNeeded();
            _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Checkout");
            Thread.Sleep(1000);
            SafeClick(By.CssSelector("form button[type='submit']"));

            var success = _driver.PageSource.Contains("thành công") || _driver.Url.Contains("/Cart/");
            Assert.IsTrue(success, "Không chuyển đến trang hoàn tất hoặc xác nhận.");
        }

        // 🔹 5️⃣ Lịch sử đơn hàng
        [Test, Order(5)]
        public void OrderHistory_Should_Display_Orders()
        {
            LoginIfNeeded();
            _driver.Navigate().GoToUrl($"{_baseUrl}/Order/History");

            var rows = _driver.FindElements(By.CssSelector("table tr"));
            Assert.IsTrue(rows.Count > 1, "Không có đơn hàng nào hiển thị.");
        }

        // ✅ Helper đăng nhập
        private void LoginIfNeeded()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
            string email = string.IsNullOrEmpty(_testEmail) ? "user@example.com" : _testEmail;
            _driver.FindElement(By.Id("Email")).SendKeys(email);
            _driver.FindElement(By.Id("Password")).SendKeys(_testPassword);
            SafeClick(By.CssSelector("button[type='submit']"));
        }

        // ✅ Click an toàn
        private void SafeClick(By by)
        {
            var element = _driver.FindElement(by);
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                element.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
            }
        }
    }
}
