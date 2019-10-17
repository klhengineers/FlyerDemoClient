using FlyerDemoClient.Model;
using FlyerDemoClient.ViewModels;
using FlyerDemoClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
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

namespace FlyerDemoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _httpClient.BaseAddress = new Uri("http://localhost:7071/api/");
            Loaded += MainWindow_Loaded;
        }

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await foreach (var p in GetProductsAsync())
                _ = AddProduct(p);
        }

        public async IAsyncEnumerable<Product> GetProductsAsync()
        {
            foreach (string sku in await Get<string[]>("products"))
                yield return await Get<Product>($"products/{sku}");
        }

        public async Task<T> Get<T>(string endpoint) =>
            await JsonSerializer.DeserializeAsync<T>(
                await _httpClient.GetStreamAsync(endpoint));

        public async Task<T> Post<T>(string endpoint, T data) =>
            await JsonSerializer.DeserializeAsync<T>(
                await (await _httpClient.PostAsync(
                    endpoint, 
                    new StringContent(JsonSerializer.Serialize(data))))
                    .Content
                    .ReadAsStreamAsync());

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
            if (dialog.ShowDialog() ?? false) await Post<Product>("products", (Product)dialog.DataContext);
            Refresh_Click(sender, e);
        }
    }
}
