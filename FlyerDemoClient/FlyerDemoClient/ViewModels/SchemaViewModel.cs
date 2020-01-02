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

        public List<ItemPropertyViewModel> Properties => ((JsonElement)Model["properties"])
                                                                      .EnumerateObject().AsEnumerable()
                                                                      .Select(KvpToTuple)
                                                                      .ToList();


        public SchemaViewModel(Dictionary<string, dynamic> model)
        {
            Model = model;
        }

        private ItemPropertyViewModel KvpToTuple(JsonProperty kvp)
        {
            string prop = kvp.Value.GetProperty("type").ValueKind switch
            {
                JsonValueKind.String => kvp.Value.GetProperty("type").GetString(),
                JsonValueKind.Array => kvp.Value.GetProperty("type").EnumerateArray().First().GetString() + "?"
            };
            return new ItemPropertyViewModel() { Name = kvp.Name, ValueType = prop };

        }

    }
}
