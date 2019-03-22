using CefSharp;
using CefSharp.WinForms;
using COMWrapperSampleApp.BrowserLogic;
using COMWrapperSampleApp.Configuration;
using COMWrapperSampleApp.OpenIdConnect.Helpers;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static IdentityModel.OidcClient.OidcClientOptions;

namespace COMWrapperSampleApp.Logic
{
    public interface IAuthenticationOperations
    {
        /// <summary>
        /// Opens a web browser so that the user performs authentication using implicit grant
        /// </summary>
        /// <returns>Access token that can be used to access secured resources</returns>
        string PromptForUserLogin();

        /// <summary>
        /// Opens a web browser so that the user performs authentication using auhtorization grant
        /// </summary>
        /// <returns>Access token that can be used to access secured resources</returns>
        string StartAuthorizationGrant();

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

        public string StartAuthorizationGrant()
        {
            var clientOptions = LoadOpenIdOptions();
            var browser = browserFactory.Browser.WebBrowser;
            clientOptions.Browser = browser;

            var oidcClient = new OidcClient(clientOptions);
            var discoClient = new DiscoveryClient(configurations.HelseIdEndpoint);
            var discoveryResponse = discoClient.GetAsync();
            var disco = discoveryResponse.GetAwaiter().GetResult();

            if (disco.IsError)
            {
                throw new ArgumentException(disco.Error);
            }

            var result = oidcClient.LoginAsync(new LoginRequest()
            {
                BackChannelExtraParameters = GetBackChannelExtraParameters(disco, clientOptions.ClientId, "YOURCERTIFICATETHUMBPRINT")
            });

            var res = result.GetAwaiter().GetResult();

            if (res.IsError)
            {
                throw new ArgumentException(res.Error);
            }

            AccessToken = res?.AccessToken;
            IdToken = res?.IdentityToken;
            IsAuthenticated = true;

            return res?.AccessToken;
        }

        private object GetBackChannelExtraParameters(DiscoveryResponse disco, string clientId, string certificateThumbprint)
        {
            var assertion = COMWrapperSampleApp.OpenIdConnect.ClientAssertion.CreateWithEnterpriseCertificate(clientId, disco.TokenEndpoint, certificateThumbprint);

            return new
            {
                assertion?.client_assertion,
                assertion?.client_assertion_type,
            };
        }

        private OidcClientOptions LoadOpenIdOptions()
        {
            return new OidcClientOptions
            {
                RedirectUri = "HELSEIDCLIENTREDIRECT",
                ClientId = "HELSEIDCLIENTID",
                Authority = configurations.HelseIdEndpoint,
                Scope = "openid profile e-helse/sfm.api/sfm.api helseid://scopes/identity/pid helseid://scopes/identity/security_level",
                ResponseMode = AuthorizeResponseMode.Redirect,
                Flow = AuthenticationFlow.AuthorizationCode,
                LoadProfile = true
            };
        }
    }
}