using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace CrawlMedicineData
{
    public partial class Form1 : Form
    {
        //Khởi tạo Chrome
        private ChromeDriverService chromeDriver;
        //Khởi tạo driver
        private IWebDriver driver;
        //Khởi tạo webElement
        private IWebElement element;
        //Khởi tạo List webElement

        public Form1()
        {
            InitializeComponent();
            //--------------Tắt màn hình command
            //Tạo tiến trình chạy dịch vụ mặc định
            chromeDriver = ChromeDriverService.CreateDefaultService();
            //Bật tính năng ẩn màn hình đen command
            chromeDriver.HideCommandPromptWindow = true;
            driver = new ChromeDriver(chromeDriver);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            driver.Navigate().GoToUrl("https://nhathuoclongchau.com.vn/thuoc");
            Thread.Sleep(3000);

            for(int i = 1; i <= 18; i++)
            {
                element = driver.FindElement(By.CssSelector($"#__next > div.omd\\:min-w-container-content.lg\\:w-full.flex.flex-col.min-h-screen > div.bg-layer-gray.pb-9.flex-1.relative > div:nth-child(3) > div > div.mt-6.container-lite > div > a:nth-child({i})"));
                element.Click();
                Thread.Sleep(3000);


                IReadOnlyCollection<IWebElement> medicines = driver.FindElements(By.CssSelector("div.flex.min-w-0.flex-1.flex-col.justify-between.pb-3")); 
                    
                foreach(IWebElement medicine in medicines)
                {
                    try
                    {
                        var cancels = medicine.FindElements(By.XPath("//span[text()='Thời gian không hợp lệ']"));
                        if (cancels.Count > 0)
                        {
                            continue;   
                        }

                        IWebElement medicineName = medicine.FindElement(By.TagName("h3"));

                        IWebElement medicinePrice = medicine.FindElement(By.CssSelector("span.font-semibold"));

                        Console.WriteLine($"Name: {medicineName.Text}\n" +
                            $"Price: {medicinePrice.Text}\n");
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message);
                    }
                }

                driver.Navigate().Back();
                Thread.Sleep(3000);
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            driver.Quit();
        }
    }
}
