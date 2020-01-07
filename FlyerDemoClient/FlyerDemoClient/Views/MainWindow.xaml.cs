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
        private readonly HttpClient _AuthClient = new HttpClient();

        public event PropertyChangedEventHandler PropertyChanged = null!;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _AuthClient.BaseAddress = new Uri("https://login.microsoftonline.com/klhengrs.onmicrosoft.com");

            Loaded += MainWindow_Loaded;
        }

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

        public bool LoggedIn => AuthStatus == "Signed in";

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
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("LoggedIn"));
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
                var dict = new Dictionary<string, string>
                {
                    ["redirect_uri"] = "http://localhost:7071/",
                    ["client_id"] = "ae52f345-85b1-40fb-86d1-987514fb4673",
                    ["client_secret"] = "AKwuUXSH5iB?X31LJ]Va?8S:lbC8YZVA",
                    //dict["token_endpoint"] = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
                    ["scope"] = "https://funcapi.klhengrs.onmicrosoft.com/user_impersonation",
                    ["grant_type"] = "authorization_code",
                    ["code"] = code!
                };

                msg.Content = new FormUrlEncodedContent(dict);

                var response = await _AuthClient.SendAsync(msg);
                if (!response.IsSuccessStatusCode) throw new Exception("Bad response");

                var BodyDict = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(await response.Content.ReadAsStreamAsync());
                Data.AddAuthentication(((JsonElement)BodyDict["access_token"]).GetString());
                await Data.Get<dynamic>("authenticated");


            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductForm();
            ((AddProductViewModel)dialog.DataContext).Model = ((dynamic)sender).DataContext.Model;
            if (dialog.ShowDialog() ?? false) await((AddProductViewModel)dialog.DataContext).PatchProduct();
            Refresh_Click(sender, e);
        }
    }
}
