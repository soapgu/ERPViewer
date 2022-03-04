using AngleSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERPViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient httpClient;
        private HttpClientHandler handler;
        private static readonly string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

        public MainWindow()
        {
            InitHttpClient();
            InitializeComponent();
        }

        /// <summary>
        /// 初始化HttpClient
        /// </summary>
        private void InitHttpClient()
        {
            handler = new HttpClientHandler();
            httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var itCode = await this.GetItCode();
            if (itCode != null)
            {
                var success = await this.Login(itCode);
                if (success)
                {
                    var task = await this.GetTaskInfo();
                    MessageBox.Show(task);
                }
            }
        }

        /// <summary>
        /// 1 获取jquery的JSESSSIONID存储至Cookie
        /// 2 获取登陆页面的It字段验证码
        /// </summary>
        /// <returns>It字段验证码</returns>
        private async Task<String> GetItCode()
        {
            string retValue = null;
            HttpResponseMessage response = await httpClient.GetAsync("/jqerp/");
            if (response.IsSuccessStatusCode)
            {

                var html = await response.Content.ReadAsStringAsync();
                var config = AngleSharp.Configuration.Default;
                //Create a new context for evaluating webpages with the given config
                var context = BrowsingContext.New(config);

                //Parse the document from the content of a response to a virtual request
                var document = await context.OpenAsync(req => req.Content(html));
                retValue = document.GetElementsByName("lt")[0].GetAttribute("value");
            }
            return retValue;
        }

        /// <summary>
        /// 完成登陆操作
        /// 并抓取js脚本中的logonCode
        /// 把logonCode加入到key为_CUNAME的cookie中
        /// </summary>
        /// <param name="itValue"></param>
        /// <returns></returns>
        private async Task<bool> Login( string itValue ) 
        {
            bool retValue = false;
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("lt", itValue));
            values.Add(new KeyValuePair<string, string>("username", txtUserName.Text));
            values.Add(new KeyValuePair<string, string>("password", txtPassword.Password));
            values.Add(new KeyValuePair<string, string>("execution", "e1s1"));
            values.Add(new KeyValuePair<string, string>("_eventId", "submit"));
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync("/cas/login?service=https://erp.shgbit.com/jqerp/", content);
            var bodyString = await response.Content.ReadAsStringAsync();

            var match = Regex.Match(bodyString, @"(?<=_userInfo=).*(?=;)");
            if (match.Success)
            {
                var loginCode = JArray.Parse(match.Value).First().Value<string>("logonCode");
                this.handler.CookieContainer.Add(new System.Net.Cookie("_CUNAME", loginCode, "/jqerp", "erp.shgbit.com"));
                retValue = true;

            }
            return retValue;
        }

        /// <summary>
        /// 获取任务信息的json
        /// </summary>
        /// <returns></returns>
        private async Task<String> GetTaskInfo() 
        {
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("actType", "loadTaskListJSON"));
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync("/jqerp/web/taskInfoService", content);
            var bodyString = await response.Content.ReadAsStringAsync();
            return bodyString;
        }
    }
}
