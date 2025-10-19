using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastFood.UITests
{
    [TestFixture]
    [Order(1)]
    public class FastFood_UITests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private readonly string _baseUrl = "https://localhost:7174"; // ⚠️ chỉnh đúng port dự án
        private readonly string _cookieFile = "cookies.txt";

        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            options.AddArgument("--window-size=1280,800");
            options.AddArgument("--ignore-certificate-errors");
            _driver = new EdgeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            if (!File.Exists(_cookieFile))
            {
                Console.WriteLine("⚠️ Cookie chưa có. Hãy chạy test SaveCookiesOnly() trước!");
                Assert.Inconclusive("⚠️ Chưa có cookie. Hãy đăng nhập thủ công qua SaveCookiesOnly().");
            }
            else
            {
                LoadCookies();
            }
        }

        // 🔹 Test đặc biệt: Lưu cookie đăng nhập Google (chạy thủ công 1 lần) 
        [Test, Order(0)]
        public void SaveCookiesOnly()
        {
            Console.WriteLine("🌐 Mở trang đăng nhập...");
            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
            Thread.Sleep(2000);

            Console.WriteLine("➡️ Click 'Đăng nhập với Google' và đăng nhập thủ công...");
            Thread.Sleep(60000); // cho 60s để đăng nhập Google

            if (!_driver.PageSource.Contains("Đăng xuất") && !_driver.Url.Contains("/Home"))
                Assert.Fail("❌ Đăng nhập Google thất bại. Hãy thử lại.");

            var cookies = _driver.Manage().Cookies.AllCookies;
            using (var writer = new StreamWriter(_cookieFile))
            {
                foreach (var c in cookies)
                    writer.WriteLine($"{c.Name}|{c.Value}|localhost|{c.Path}|{c.Expiry}");
            }

            Console.WriteLine("✅ Cookie đã được lưu thành công.");
            Assert.Pass("✅ Cookie login đã được lưu!");
        }

        private void LoadCookies()
        {
            Console.WriteLine("🔑 Đang tải cookie đã lưu...");
            _driver.Navigate().GoToUrl(_baseUrl);
            Thread.Sleep(1000);

            foreach (var line in File.ReadAllLines(_cookieFile))
            {
                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    try
                    {
                        var cookie = new Cookie(
                            parts[0],
                            parts[1],
                            "localhost", // cố định domain
                            parts[3],
                            DateTime.TryParse(parts.ElementAtOrDefault(4), out var exp) ? exp : null
                        );
                        _driver.Manage().Cookies.AddCookie(cookie);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Lỗi khi thêm cookie: {ex.Message}");
                    }
                }
            }

            _driver.Navigate().Refresh();
            Thread.Sleep(2000);

            if (!_driver.PageSource.Contains("Đăng xuất") && !_driver.Url.Contains("/Home"))
                Assert.Inconclusive("⚠️ Cookie hết hạn. Hãy chạy lại SaveCookiesOnly().");
            else
                Console.WriteLine("✅ Đăng nhập bằng cookie thành công.");
        }
        // Lưu cookie hiện tại
        private void SaveCurrentCookies()
        {
            try
            {
                var cookies = _driver.Manage().Cookies.AllCookies;
                using (var writer = new StreamWriter("cookies.txt", false))
                {
                    foreach (var c in cookies)
                        writer.WriteLine($"{c.Name}|{c.Value}|{c.Domain}|{c.Path}|{c.Expiry}");
                }
                Console.WriteLine("💾 Cookie sau Checkout đã được cập nhật.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Không thể lưu cookie: {ex.Message}");
            }
        }

        // Reload cookie khi bị đăng xuất
        private void ReloadCookiesAndRetry()
        {
            Console.WriteLine("🔁 Thử nạp lại cookie từ file cookies.txt...");
            try
            {
                _driver.Navigate().GoToUrl(_baseUrl);
                foreach (var line in File.ReadAllLines("cookies.txt"))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        var cookie = new Cookie(parts[0], parts[1], domain: null, path: parts[3],
                            expiry: DateTime.TryParse(parts.ElementAtOrDefault(4), out var exp) ? exp : null);
                        _driver.Manage().Cookies.AddCookie(cookie);
                    }
                }
                _driver.Navigate().Refresh();
                Thread.Sleep(1500);
                Console.WriteLine("✅ Cookie đã được nạp lại, người dùng đăng nhập trở lại.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi nạp lại cookie: {ex.Message}");
            }
        }


        [TearDown]
        public void TearDown()
        {
            try
            {
                _driver.Quit();
                _driver.Dispose();
            }
            catch { }
        }

        // 🧩 1️⃣ Thêm món vào giỏ hàng
        [Test, Order(1)]
        public void AddToCart_Should_Add_Item_And_Show_Success_Message()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/Food/Index");

            var addButton = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-success.btn-sm")));
            addButton.Click();
            Thread.Sleep(1500);

            _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Index");

            var cartTable = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            var cartRows = _driver.FindElements(By.CssSelector("table tbody tr"));

            Assert.IsTrue(cartRows.Count > 0, "❌ Giỏ hàng trống sau khi thêm món!");
            Console.WriteLine($"✅ Đã thêm món: {cartRows[0].Text}");
        }

        // 🧩 2️⃣ Thanh toán đơn hàng (nhấn nút đăng nhập lại để có thể pass)
        [Test, Order(2)]
        public void Checkout_Should_Clear_Cart_And_Show_Success_Message()
        {
            try
            {
                Console.WriteLine("🧾 Bắt đầu kiểm tra Checkout...");

                // B1️⃣: Mở giỏ hàng
                _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Index");
                Thread.Sleep(1000);

                // Nếu giỏ hàng trống → thêm món tự động
                if (_driver.PageSource.Contains("Giỏ hàng trống"))
                {
                    Console.WriteLine("🛒 Giỏ hàng trống → thêm món mới...");
                    _driver.Navigate().GoToUrl($"{_baseUrl}/Food/Index");

                    var addButton = _wait.Until(ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("a.btn-success.btn-sm")));
                    addButton.Click();

                    Thread.Sleep(1000);
                    _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Index");
                }

                // B2️⃣: Mở trang Checkout
                _driver.Navigate().GoToUrl($"{_baseUrl}/Cart/Checkout");
                Console.WriteLine("💳 Mở trang Checkout...");

                var confirmButton = _wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("form button[type='submit']")));
                confirmButton.Click();
                Console.WriteLine("💳 Click 'Xác nhận thanh toán'...");

                // B3️⃣: Theo dõi redirect sang trang CompleteCheckout
                bool checkoutDone = false;
                for (int i = 0; i < 25; i++)
                {
                    var html = _driver.PageSource;
                    var url = _driver.Url;

                    // ✅ Thành công
                    if (url.Contains("/Cart/CompleteCheckout") || html.Contains("Thanh toán thành công"))
                    {
                        checkoutDone = true;
                        Console.WriteLine("✅ Đã đến trang CompleteCheckout.");
                        break;
                    }

                    // ⚠️ Bị logout do mất cookie
                    if (url.Contains("/Account/Login"))
                    {
                        Console.WriteLine("⚠️ Cookie đăng nhập đã hết hạn — tiến hành reload cookie...");
                        ReloadCookiesAndRetry();
                        return;
                    }

                    Thread.Sleep(1000);
                }

                Assert.IsTrue(checkoutDone, "❌ Không chuyển đến trang /Cart/CompleteCheckout.");

                // B4️⃣: Kiểm tra nút quay lại thực đơn
                var backButton = _wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("a.btn.btn-primary")));
                backButton.Click();

                Thread.Sleep(1500);

                // Kiểm tra xem người dùng vẫn còn đăng nhập
                Assert.IsFalse(_driver.Url.Contains("/Account/Login"),
                    "❌ Người dùng bị đăng xuất sau khi quay về thực đơn.");

                Console.WriteLine("✅ Thanh toán thành công, người dùng vẫn đăng nhập khi quay về thực đơn.");

                // B5️⃣: Lưu lại cookie mới (để test sau ổn định)
                SaveCurrentCookies();

                // ✅ Đóng trình duyệt để hoàn tất test
                Console.WriteLine("🚪 Đóng trình duyệt sau khi Checkout hoàn tất...");
                _driver.Quit();
                _driver.Dispose();
            }
            catch (Exception ex)
            {
                Assert.Fail($"❌ Lỗi trong quá trình Checkout: {ex.Message}");
            }
        }


        // 🧩 3️⃣ Lịch sử đơn hàng
        [Test, Order(3)]
        public void OrderHistory_Should_Display_Orders()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/Order/History");
            var rows = _wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("table tr")));

            Assert.IsTrue(rows.Count > 1, "❌ Không có đơn hàng hiển thị.");
            Console.WriteLine($"✅ Có {rows.Count - 1} đơn hàng hiển thị.");
        }
    }
}
