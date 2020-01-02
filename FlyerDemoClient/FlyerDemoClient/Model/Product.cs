using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FlyerDemoClient.Model
{
    public class Product
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string ManufacturerID { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string[] Attachments { get; set; } = new string[] { };
        public string[] Images { get; set; } = new string[] { };
    }
}
