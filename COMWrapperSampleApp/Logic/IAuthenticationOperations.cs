using CefSharp;
using CefSharp.WinForms;
using COMWrapperSampleApp.BrowserLogic;
using COMWrapperSampleApp.Configuration;
using COMWrapperSampleApp.OpenIdConnect.Helpers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace COMWrapperSampleApp.Logic
{
    public interface IAuthenticationOperations
    {
        /// <summary>
        /// Opens a web browser so that the user performs authentication
        /// </summary>
        /// <returns>Access token that can be used to access secured resources</returns>
        string PromptForUserLogin();

        /// <summary>
        /// Access token that can be used to access secured resources
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Id token that can be used to access user profile 
        /// </summary>
        string IdToken { get; }

        /// <summary>
        /// If true the user has been authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Http url representation of the access token and id token, to be passed to an another application
        /// </summary>
        string HttpToken { get; }
    }

    public class AuthenticationOperations : IAuthenticationOperations
    {
        private readonly IConfigurations configurations;
        private readonly IBrowserFactory browserFactory;

        public string AccessToken { get; private set; }
        public string IdToken { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public string HttpToken => $"access_token={AccessToken}&id_token={IdToken}";
        
        private ManualResetEvent manualResetEvent;

        public AuthenticationOperations(IConfigurations configurations, IBrowserFactory browserFactory)
        {
            this.configurations = configurations;
            this.browserFactory = browserFactory;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string PromptForUserLogin()
        {
            manualResetEvent = new ManualResetEvent(false);

            // start listening on redirect URL
            new BrowserRequestHandler(configurations.HelseIdRedirectUri);
            // Use implicit Flow
            var browser = browserFactory.Browser.WebBrowser;
            browser.OpenBrowser(BuildIUrlForImplicitFlow(), BrowserAddressChanged);
            manualResetEvent.WaitOne();
            return AccessToken;
        }

        private void BrowserAddressChanged(object e,  AddressChangedEventArgs args)
        {
            var address = args.Address;
            if (address.Contains("access_token") && address.Contains("#") && address.Contains(configurations.HelseIdRedirectUri))
            {
                address = (new Regex("#")).Replace(address, "?url=text&", 1);
                AccessToken = HttpUtility.ParseQueryString(address).Get("access_token");
                IdToken = HttpUtility.ParseQueryString(address).Get("id_token");
                IsAuthenticated = true;
                var chromium = e as ChromiumWebBrowser;
                chromium.AddressChanged -= BrowserAddressChanged;
                chromium?.Load("about:blank");
                manualResetEvent.Set();
            }
        }
            
        private string BuildIUrlForImplicitFlow()
        {
            return $"{configurations.HelseIdEndpoint}/connect/authorize?response_type=id_token%20token&client_id={configurations.HelseIdClientId}&scope={configurations.HelseIdScope}&redirect_uri={configurations.HelseIdRedirectUri}&prompt=Login&nonce={new CryptoHelper().CreateNonce()}";
        }
    }
}