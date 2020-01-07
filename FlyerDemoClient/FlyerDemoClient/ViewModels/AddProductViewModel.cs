using FlyerDemoClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Navigation;

namespace FlyerDemoClient.ViewModels
{
    class AddProductViewModel : INotifyPropertyChanged
    {
        public AddProductViewModel(Product model)
        {
            var temp = Task.Run(() => Data.Get<List<string>>(Data.SchemaEndpoint)).Result;
            this.Model = model;
            this.ProductSchemas = temp;
        }

        private Product _model = null!;
        public Product Model
        {
            get => _model;
            set
            {
                _model = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Model"));
                try { SelectedSchema = _model.Type.Split('.', ',')[3]; }
                catch { }

                foreach (var prop in Schema?.Properties ?? new List<ItemPropertyViewModel>())
                {
                    prop.Value = _model.GetType().GetProperty(prop.Name)?.GetValue(_model) switch
                    {
                        IEnumerable<string> list => string.Join(',', list),
                        object o => o.ToString() ?? "Error",
                        _ => throw new NotImplementedException()
                    };
                }
            }
        }

        public List<string> ProductSchemas
        {
            get; private set;
        }
        private string _SelectedSchema = string.Empty;
        public string SelectedSchema
        {

            get => _SelectedSchema;
            set
            {
                _SelectedSchema = value;
                var tempStr = Task.Run(() => Data.Get<Dictionary<string, dynamic>>($"schemas/{_SelectedSchema}")).Result;
                Schema = new SchemaViewModel(tempStr);
            }
        }

        public SchemaViewModel? Schema
        {
            get => _schema;
            set
            {
                _schema = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("schema"));
            }
        }
        private SchemaViewModel? _schema;
        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task<bool> PostProduct()
        {
            Dictionary<string, dynamic> ProductPropertyDict = Schema!.Properties.ToDictionary(prop => prop.Name, prop => (dynamic)prop.Value);

            ProductPropertyDict["Images"] = ((string)ProductPropertyDict["Images"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict["Attachments"] = ((string)ProductPropertyDict["Attachments"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict["$type"] = $"Flyer.Common.Models.{_SelectedSchema}, Flyer.Common";

            // force $type property to be first so the server reads it properly
            ProductPropertyDict = ProductPropertyDict
                .OrderByDescending(kvp => kvp.Key.StartsWith("$"))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            try
            {
                var result = await Data.Post("products", ProductPropertyDict);
                return result != null;
            }
            catch { System.Windows.MessageBox.Show("Failed to post product"); }
            return false;
        }

        public async Task<bool> PatchProduct()
        {
            Dictionary<string, dynamic> ProductPropertyDict = Schema!.Properties.ToDictionary(prop => prop.Name, prop => (dynamic)prop.Value);

            ProductPropertyDict["Images"] = ((string)ProductPropertyDict["Images"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict["Attachments"] = ((string)ProductPropertyDict["Attachments"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict = ProductPropertyDict
                .OrderByDescending(kvp => kvp.Key.StartsWith("$"))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            try
            {
                var result = await Data.Patch($"products/{ProductPropertyDict["SKU"]}", ProductPropertyDict);
                return result != null;
            }
            catch { System.Windows.MessageBox.Show("Failed to patch product"); }
            return false;
        }
    }
}
