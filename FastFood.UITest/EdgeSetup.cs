using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFood.UITest
{
    public static class EdgeSetup
    {
        public static IWebDriver CreateDriver()
        {
            var options = new EdgeOptions();
            options.AddArgument("--headless"); // Chạy ẩn (tùy chọn)
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            return new EdgeDriver(options);
        }
    }
}
