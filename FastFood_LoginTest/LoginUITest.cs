using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace FastFood_LoginTest
{
    public class LoginUITest
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            options.AddArgument("headless");
            options.AddArgument("disable-gpu");
            options.AddArgument("no-sandbox");
            // Adjust path/options as needed for your environment
            driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void LoginPage_UI_Test()
        {
            // Arrange
            var loginUrl = "https://localhost:7243/Identity/Account/Login";
            // Replace these with valid test credentials for your application
            var validEmail = "testuser@example.com";
            var validPassword = "P@ssw0rd!";

            // Act
            driver.Navigate().GoToUrl(loginUrl);

            // Wait for inputs and button
            var emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
            var passwordInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Password")));
            var loginButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("login-submit")));

            emailInput.Clear();
            emailInput.SendKeys(validEmail);

            passwordInput.Clear();
            passwordInput.SendKeys(validPassword);

            loginButton.Click();

            // Assert:
            // Prefer detecting a known post-login element (e.g., a logout link with id "logout-link").
            // If that element doesn't exist in the app UI, fall back to checking the URL changed.
            bool loggedIn = false;
            try
            {
                // Wait for either the logout element or a URL change away from the login page.
                wait.Until(driver =>
                {
                    try
                    {
                        var logout = driver.FindElement(By.Id("logout-link"));
                        if (logout.Displayed) return true;
                    }
                    catch (NoSuchElementException) { /* ignore */ }

                    return !driver.Url.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase);
                });

                // Final evaluation
                try
                {
                    loggedIn = driver.FindElement(By.Id("logout-link")).Displayed;
                }
                catch (NoSuchElementException)
                {
                    loggedIn = !driver.Url.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (WebDriverTimeoutException)
            {
                loggedIn = false;
            }

            Assert.IsTrue(loggedIn, "Login did not succeed or the UI did not update as expected after submitting credentials.");
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                try { driver.Quit(); } catch { /* ignore */ }
                try { driver.Dispose(); } catch { /* ignore */ }
                driver = null;
            }
        }
    }
}