using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace FlyerDemoClient.ViewModels
{
    public class SchemaViewModel
    {
        Dictionary<string, dynamic> Model { get; set; }

        public List<ItemPropertyViewModel> Properties { get; set; }


        public SchemaViewModel(Dictionary<string, dynamic> model)
        {
            Model = model;
            Properties = ((JsonElement)Model["properties"])
                            .EnumerateObject().AsEnumerable()
                            .Select(KvpToTuple)
                            .ToList();
        }

        private ItemPropertyViewModel KvpToTuple(JsonProperty kvp)
        {
            string prop = kvp.Value.GetProperty("type").ValueKind switch
            {
                JsonValueKind.String => kvp.Value.GetProperty("type").GetString(),
                JsonValueKind.Array => kvp.Value.GetProperty("type").EnumerateArray().First().GetString() + "?",
                _ => throw new NotImplementedException()
            };
            return new ItemPropertyViewModel() { Name = kvp.Name, ValueType = prop };

        }

    }
}
