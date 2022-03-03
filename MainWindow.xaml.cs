using AngleSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            HttpResponseMessage response = await httpClient.GetAsync("/jqerp/");
            if (response.IsSuccessStatusCode)
            {
                //MessageBox.Show("Cookie:" + handler.CookieContainer.GetCookies(new Uri(baseUrl + "/jqerp/")).Count);
                //MessageBox.Show("cas Cookie:" + handler.CookieContainer.GetCookies(new Uri(baseUrl + "/cas/")).Count);
                var html = await response.Content.ReadAsStringAsync();
                //MessageBox.Show(retValue);
                //Use the default configuration for AngleSharp
                var config = AngleSharp.Configuration.Default;

                //Create a new context for evaluating webpages with the given config
                var context = BrowsingContext.New(config);

                //Parse the document from the content of a response to a virtual request
                var document = await context.OpenAsync(req => req.Content(html));
                var ltValue = document.GetElementsByName("lt")[0].GetAttribute("value");
            }
        }
    }
}
