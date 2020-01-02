using FlyerDemoClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

        public Product Model { get; set; }

        public List<string> ProductSchemas
        {
            get; private set;
        }
        private string _SelectedSchema = string.Empty;
        public string SelectedSchema
        {

            get
            {
                return _SelectedSchema;
            }

            set
            {
                _SelectedSchema = value;
                var tempStr = Task.Run(() => Data.Get<Dictionary<string, dynamic>>($"schemas/{_SelectedSchema}")).Result;
                schema = new SchemaViewModel(tempStr);
            }
        }

        public SchemaViewModel? schema
        {
            get => _schema;
            set
            {
                _schema = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("schema"));
            }
        }
        private SchemaViewModel? _schema;
        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task<bool> PostProduct()
        {
            Dictionary<string, dynamic> ProductPropertyDict = schema!.Properties.ToDictionary(prop => prop.Name, prop => (dynamic)prop.Value);

            ProductPropertyDict["Images"] = ((string)ProductPropertyDict["Images"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict["Attachments"] = ((string)ProductPropertyDict["Attachments"]).Split(",").Select(s => s.Trim()).ToList();

            ProductPropertyDict["$type"] = $"Flyer.Common.Models.{_SelectedSchema}, Flyer.Common";

            ProductPropertyDict = ProductPropertyDict
                .OrderByDescending(kvp => kvp.Key.StartsWith("$"))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var result = await Data.Post("products", ProductPropertyDict);
            return result != null;
        }
    }
}
