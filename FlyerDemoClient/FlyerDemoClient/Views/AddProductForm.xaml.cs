using FlyerDemoClient.Model;
using FlyerDemoClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlyerDemoClient.Views
{
    /// <summary>
    /// Interaction logic for AddProductForm.xaml
    /// </summary>
    public partial class AddProductForm : Window
    {
        public AddProductForm()
        {
            InitializeComponent();
            DataContext = new AddProductViewModel(new Product() { 
            
                Attachments = new string[] { },
                Images = new string[] { "https://www.economist.com/sites/default/files/images/print-edition/20150905_TQP001_0.jpg"},
                Type = "Flyer.Common.Models.Widget, Flyer.Common"
            });
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = true;
            Close();
        }
    }
}
