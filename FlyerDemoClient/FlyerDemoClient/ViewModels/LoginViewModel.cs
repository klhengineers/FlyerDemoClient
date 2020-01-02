using System;
using System.Collections.Generic;
using System.Text;

namespace FlyerDemoClient.ViewModels
{
    public class LoginViewModel
    {
        private static readonly string _tenantName = "klhengrs";
        private static readonly string _baseUrl = "https://login.microsoftonline.com";
        private static readonly string _appId = "ae52f345-85b1-40fb-86d1-987514fb4673";

        public event EventHandler? CodeObtained;

        public string? Code
        {
            get => _token;
            set
            {
                _token = value;
                CodeObtained?.Invoke(this, new EventArgs());
            }
        }
        private string? _token = null;

        public string Address { get; set; } = $"{_baseUrl}/{_tenantName}.onmicrosoft.com/oauth2/v2.0/authorize?response_type=code&client_id={_appId}&redirect_uri=http://localhost:7071/&scope=openid";
    }
}
