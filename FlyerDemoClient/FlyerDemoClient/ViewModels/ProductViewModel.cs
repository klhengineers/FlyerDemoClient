using FlyerDemoClient.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlyerDemoClient.ViewModels
{
    public class ProductViewModel
    {
        public ProductViewModel(Product model) => Model = model;

        public Product Model { get; set; }
        public int ImageNumber { get; set; } = 0;
        public string ActiveImage => Model.Images[ImageNumber];
    }
}
