using FlyerDemoClient.Model;
using FlyerDemoClient.ViewModels;
using FlyerDemoClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Web;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using FlyerDemoClient;
using System.ComponentModel;

namespace FlyerDemoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HttpClient _AuthClient = new HttpClient();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _httpClient.BaseAddress = new Uri("http://localhost:7071/api/");
            _AuthClient.BaseAddress = new Uri("https://login.microsoftonline.com/klhengrs.onmicrosoft.com");

            Loaded += MainWindow_Loaded;
        }

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();
        private string _AuthStatus = "Not signed in";
        public string AuthStatus
        {
            get
            {
                return _AuthStatus;
            }

            set
            {
                _AuthStatus = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AuthStatus"));
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await foreach (var p in GetProductsAsync())
                _ = AddProduct(p);
        }

        public async IAsyncEnumerable<Product> GetProductsAsync()
        {
            foreach (string sku in await Data.Get<string[]>("products"))
                yield return await Data.Get<Product>($"products/{sku}");
        }

        //public async Task<T> Get<T>(string endpoint) =>
        //    await JsonSerializer.DeserializeAsync<T>(
        //        await _httpClient.GetStreamAsync(endpoint));

        //public async Task<T> Post<T>(string endpoint, T data) =>
        //    await JsonSerializer.DeserializeAsync<T>(
        //        await (await _httpClient.PostAsync(
        //            endpoint, 
        //            new StringContent(JsonSerializer.Serialize(data))))
        //            .Content
        //            .ReadAsStreamAsync());

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Products.Clear();
            await foreach (var p in GetProductsAsync())
                _ = AddProduct(p);

        }

        private async Task AddProduct(Product p)
        {
            await App.Current.Dispatcher.BeginInvoke(
                                (Action)delegate () { Products.Add(new ProductViewModel(p)); });
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductForm();
            if (dialog.ShowDialog() ?? false) await ((AddProductViewModel)dialog.DataContext).PostProduct();
            Refresh_Click(sender, e);

        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginView();
            if (dialog.ShowDialog() ?? false)
            {
                AuthStatus = "Signed in";
                string? code = ((LoginViewModel)dialog.DataContext).Code;

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get,
                                                                "https://login.microsoftonline.com/klhengrs.onmicrosoft.com/oauth2/v2.0/token");
                //msg.Headers.Add("Host", "login.microsoftonline.com");
                var dict = new Dictionary<string, string>();
                dict["redirect_uri"] = "http://localhost:7071/";
                dict["client_id"] = "ae52f345-85b1-40fb-86d1-987514fb4673";
                dict["client_secret"] = "AKwuUXSH5iB?X31LJ]Va?8S:lbC8YZVA";
                //dict["token_endpoint"] = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
                dict["scope"] = "https://funcapi.klhengrs.onmicrosoft.com/user_impersonation";
                dict["grant_type"] = "authorization_code";
                dict["code"] = code!;

                msg.Content = new FormUrlEncodedContent(dict);

                var response = await _AuthClient.SendAsync(msg);
                if (!response.IsSuccessStatusCode) throw new Exception("Bad response");

                var BodyDict = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(await response.Content.ReadAsStreamAsync());
                Data.AddAuthentication(((JsonElement)BodyDict["access_token"]).GetString());
                var s = await Data.Get<dynamic>("authenticated");


            }
        }
    }
}
