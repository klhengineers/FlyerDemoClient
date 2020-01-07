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
using FlyerDemoClient.ViewModels;
using CefSharp;
using System.Linq;
using System.Web;

namespace FlyerDemoClient.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
            Browser.RequestHandler = new RequestHandler((LoginViewModel)this.DataContext);
            Browser.Focus();
            ((LoginViewModel)this.DataContext).CodeObtained += (s, e) => Dispatcher.Invoke(() => { this.DialogResult = true; this.Close();});
        }
    }

    class RequestHandler : IRequestHandler
    {
        private LoginViewModel _lvm;

        public RequestHandler(LoginViewModel lvm) => _lvm = lvm;

        public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) =>
            true;

        public bool CanSetCookie(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, CefSharp.Cookie cookie) =>
            true;

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            throw new NotImplementedException();
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return null!;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) =>
            null!;

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect) =>
            false;

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            var idx = request.Url.IndexOf('?');
            if (idx >= 0)
            {
                var dict = HttpUtility.ParseQueryString(request.Url.Substring(idx));
                if (dict.AllKeys.Contains("code"))
                {
                    _lvm.Code = dict["code"];
                }
            }
            return false;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) =>
            CefReturnValue.Continue;

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) =>
            false;

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
        }

        public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, string url) =>
            false;
        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }
        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
        }
        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
        public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            _lvm.Code = HttpUtility.ParseQueryString(request.Url.Substring(request.Url.IndexOf('?')))["code"];
        }
        public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {
            _lvm.Code = HttpUtility.ParseQueryString(request.Url.Substring(request.Url.IndexOf('?')))["code"];
        }
        public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) =>
            false;

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, System.Security.Cryptography.X509Certificates.X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            callback.Dispose();
            return false;
        }
    }
}
