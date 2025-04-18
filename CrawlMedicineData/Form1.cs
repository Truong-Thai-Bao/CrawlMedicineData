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
using System.Data.SqlClient;
//In ra console có tiếng việt
using System.Diagnostics;
namespace CrawlMedicineData
{
    public partial class Form1 : Form
    {
        //Khởi tạo Chrome
        private ChromeDriverService chromeDriver;
        //Khởi tạo ChromeOptions
        private ChromeOptions chromeOptions;
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
            //Tạo ChromeOptions
            chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("start-maximized");
            driver = new ChromeDriver(chromeDriver,chromeOptions);
        }

        //Hàm lưu vào csdl
        private void SaveToDB(string name, int price, string type)
        {
            //Kết nối đến csdl
            using (SqlConnection connection = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=CMSystem;Integrated Security=True"))
            {
                connection.Open();
                //Tạo câu lệnh sql
                string sql = "INSERT INTO Medicine (MedicineName, Price, Type) VALUES (@name, @price, @type)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@type", type);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            driver.Navigate().GoToUrl("https://nhathuoclongchau.com.vn/thuoc");
            Thread.Sleep(3000);

            for(int i = 1; i <= 18; i++)
            {
                //Tìm kiếm thuốc theo từng loại
                element = driver.FindElement(By.CssSelector($"#__next > div.omd\\:min-w-container-content.lg\\:w-full.flex.flex-col.min-h-screen > div.bg-layer-gray.pb-9.flex-1.relative > div:nth-child(3) > div > div.mt-6.container-lite > div > a:nth-child({i})"));
                element.Click();
                Thread.Sleep(3000);

                //Lấy danh sách thuốc
                IReadOnlyCollection<IWebElement> medicines = driver.FindElements(By.CssSelector("div.flex.min-w-0.flex-1.flex-col.justify-between.pb-3"));

                //Duyệt qua từng thuốc
                foreach (IWebElement medicine in medicines)
                {
                    try
                    {
                        //Bỏ qua các sản phẩm không thể mua tự do 
                        var cancels = medicine.FindElements(By.XPath("//div[text()='Cần tư vấn từ dược sĩ']"));
                        //Bỏ qua các sản phẩm không có hàng
                        var cancels2 = medicine.FindElements(By.XPath("//div[text()='Tạm hết hàng']"));
                        if (cancels.Count > 0||cancels2.Count>0)
                        {
                            continue;   
                        }

                        //Lấy tên thuốc
                        IWebElement medicineName = medicine.FindElement(By.TagName("h3"));
                        //Lấy giá thuốc
                        IWebElement medicinePrice = medicine.FindElement(By.CssSelector("span.font-semibold"));
                        //Lấy loại thuốc
                        IWebElement medicineType = medicine.FindElement(By.CssSelector("span.text-label2"));

                        string priceText = medicinePrice.Text.Replace("đ", "").Replace(".", "").Trim();
                        // Dùng TryParse để tránh lỗi format
                        if (int.TryParse(priceText, out int price))
                        {
                            SaveToDB(
                                medicineName.Text.Trim(),
                                price,
                                medicineType.Text.Trim().Substring(1)
                            );
                        }
                        else
                        {
                            Debug.WriteLine($"Lỗi parse giá: {medicinePrice.Text}");
                        }

                        Debug.WriteLine($"Name: {medicineName.Text.Trim()}\n" +
                            $"Price: {medicinePrice.Text.Trim()}\n" +
                            $"Type: {medicineType.Text.Trim().Substring(1)}\n");
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
