using CefSharp.WinForms;
using System;

namespace COMWrapperSampleApp.BrowserLogic
{
    public class EmbeddedBrowser : Browser
    {
        private ChromiumWebBrowser browser;

        public EmbeddedBrowser(ChromiumWebBrowser browser, string uri)
            : base(uri)
        {
            this.browser = browser;
        }

        public void UpdateRedirectUri(string uri)
        {
            RedirectUri = uri;
        }

        public override void OpenBrowser(string url)
        {
            browser.Load(url);
        }

        public void OpenBrowser(string url, EventHandler<CefSharp.AddressChangedEventArgs> AddressChanged)
        {
            browser.AddressChanged += AddressChanged;
            browser.Load(url);
        }
    }
}
