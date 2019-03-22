using COMWrapperSampleApp.BrowserLogic;
using COMWrapperSampleApp.Configuration;
using FM.Common.ClientServerApi.Epj;
using FM.Common.DataModel;
using FM.Common.DataModel.EpjApi;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Xml.Schema.Linq;

namespace COMWrapperSampleApp.Logic
{
    public interface IApiOperations : IEpjApi
    {
    }

    public class ApiOperations : IApiOperations
    {
        private readonly IAuthenticationOperations authentication;
        private readonly IConfigurations configurations;
        private readonly IBrowserFactory browser;

        private int httpCode;
        private string httpErrorMsg;

        public ApiOperations(IAuthenticationOperations authentication, IConfigurations configurations, IBrowserFactory browser)
        {
            this.authentication = authentication;
            this.configurations = configurations;
            this.browser = browser;
        }

        public LesCaveSvar LesCave(LesCave parameter)
        {
            return RunRemotely<LesCaveSvar>(parameter, "LesCave");
        }

        public LesVarslingerSvar LesVarslinger(LesVarslinger parameter)
        {
            return RunRemotely<LesVarslingerSvar>(parameter, "LesVarslinger");
        }

        public LesTakstSvar LesTakst(LesTakst parameter)
        {
            return RunRemotely<LesTakstSvar>(parameter, "LesTakst");
        }

        public LesVarerIBrukSvar LesVarerIBruk(LesVarerIBruk parameter, bool convertM1ToV24)
        {
            return RunRemotely<LesVarerIBrukSvar>(parameter, "LesVarerIBruk");
        }

        public SkrivBrukerInfoSvar SkrivBrukerInfo(SkrivBrukerInfo parameter)
        {
            return RunRemotely<SkrivBrukerInfoSvar>(parameter, "SkrivBrukerInfo");
        }

        public SkrivCaveSvar SkrivCave(SkrivCave parameter)
        {
            return RunRemotely<SkrivCaveSvar>(parameter, "SkrivCave");
        }

        public SkrivKorrespondanseInfoSvar SkrivKorrespondanseInfo(SkrivKorrespondanseInfo parameter)
        {
            return RunRemotely<SkrivKorrespondanseInfoSvar>(parameter, "SkrivKorrespondanseInfo");
        }

        public StartPasientSvar StartPasient(StartPasient parameter)
        {
            var fnr = parameter?.Pasient?.Id?.FirstOrDefault(x => x.TypeId == CodingSystems.Identity.Fnr);
            browser.UpdateMainFormTitle($"SFM COM Wrapper Sample (Pasient : {fnr?.Id})");
            var answer = RunRemotely<StartPasientSvar>(parameter, "StartPasient", fnr?.Id);
            if (!answer.Returkode.HasValue)
            {
                answer.Returkode = httpCode;
                answer.Feilmelding = httpErrorMsg;
            }
            return answer;
        }

        public StartInboxSvar StartInbox(StartInbox parameter)
        {
            return RunRemotely<StartInboxSvar>(parameter, "StartInbox");
        }

        public StartCaveSvar StartCave(StartCave parameter)
        {
            return RunRemotely<StartCaveSvar>(parameter, "StartCave");
        }

        public LesSisteVibCaveOppdateringSvar LesSisteVibCaveOppdatering(LesSisteVibCaveOppdatering parameter)
        {
            return RunRemotely<LesSisteVibCaveOppdateringSvar>(parameter, "LesSisteVibCaveOppdatering");
        }

        public LesAntallMeldingerForSigneringSvar LesAntallMeldingerForSignering(LesAntallMeldingerForSignering parameter)
        {
            return RunRemotely<LesAntallMeldingerForSigneringSvar>(parameter, "LesAntallMeldingerForSignering");
        }

        public SlaSammenPasienterSvar SlaSammenPasienter(SlaSammenPasienter parameter)
        {
            return RunRemotely<SlaSammenPasienterSvar>(parameter, "SlaSammenPasienter");
        }

        public LukkSvar Lukk(Lukk parameter)
        {
            return RunRemotely<LukkSvar>(parameter, "Lukk");
        }

        public SkrivPasientSvar SkrivPasient(SkrivPasient parameter)
        {
            return RunRemotely<SkrivPasientSvar>(parameter, "SkrivPasient");
        }

        public SkrivUtSvar SkrivUt(SkrivUt parameter)
        {
            return RunRemotely<SkrivUtSvar>(parameter, "SkrivUt");
        }

        public LesPasientStatusSvar LesPasientStatus(LesPasientStatus parameter)
        {
            return RunRemotely<LesPasientStatusSvar>(parameter, "LesPasientStatus");
        }

        private T RunRemotely<T>(IEpjApiRequest parameter, string methodName, string fnr = null) where T: XTypedElement, new()
        {
            //Read access and id_token from COM call 
            var accessToken = parameter?.LoginInfo?.AccessToken;
            var idToken = parameter?.LoginInfo?.IdToken;
            var tokens = $"access_token={accessToken}&id_token={idToken}";

            // There are now tokens present -> Redirect to login
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(idToken))
            {
                if (!authentication.IsAuthenticated)
                {
                    try
                    {
                        // Uses implicit grant flow
                        authentication.PromptForUserLogin();
                        
                        // Uses authorization grant flow
                        //authentication.StartAuthorizationGrant();

                        accessToken = authentication.AccessToken;
                        idToken = authentication.IdToken;
                        tokens = authentication.HttpToken;
                    }
                    catch (Exception e)
                    {
                        httpErrorMsg = e.Message;
                        httpCode = 404;
                        return new T();
                    }
                }
            }

            var sfmClientUrl = "";
            var sfmApiEndpoint = "";

            // Connect to SFM.Router to get client and api endpoints for this user/installation
            var httpHandler = new HttpClientHandler()
            {
                // Prevent 302 redirection
                AllowAutoRedirect = false
            };

            using (var httpClient = new HttpClient(httpHandler))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = httpClient.GetAsync(configurations.SfmRouterEndpoint).Result;

                if (!response.StatusCode.Equals(HttpStatusCode.Found))
                {
                    throw new ApplicationException($"Error communicating with SFM Router: {response.ReasonPhrase}");
                }

                var clientAndApiEnpoint = new Uri(response.Headers.Location.ToString());
                sfmApiEndpoint = HttpUtility.ParseQueryString(clientAndApiEnpoint.Query).Get("api_endpoint");
                sfmClientUrl = clientAndApiEnpoint.GetLeftPart(UriPartial.Authority);
            }

            if (!sfmClientUrl.EndsWith("/"))
            {
                sfmClientUrl += "/";
            }

            if (!sfmApiEndpoint.EndsWith("/"))
            {
                sfmApiEndpoint += "/";
            }


            // Connect to SFM Epj API to store/update patient and get ticket
            var sfmApiEndpointMethod = $"{sfmApiEndpoint}api/Epj/{methodName}";

            httpHandler = new HttpClientHandler() { AllowAutoRedirect = false };
            using (var httpClient = new HttpClient(httpHandler))
            {
                using (var stringContent = new StringContent(((XTypedElement)parameter).Untyped.ToString(), Encoding.UTF8, "application/xml"))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                    var response = httpClient.PostAsync(sfmApiEndpointMethod, stringContent).Result;
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        // Construct client entry point with the result from start pasient call
                        response.Headers.TryGetValues("ClientUrl", out var url);
                        sfmClientUrl += url.First() + $"&api_endpoint={sfmApiEndpoint}";

                        var hostUrl = new Uri(sfmClientUrl);

                        // Open SFM client
                        browser.UpdateMainFormTitle($"SFM COM Wrapper (Pasient : {fnr}) @{hostUrl.Host}");
                        browser.Browser.Navigate($"{sfmClientUrl}#{tokens}");
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        return new T { Untyped = XElement.Parse(responseContent) };
                    }

                    httpErrorMsg = response.ReasonPhrase;
                    httpCode = (int)response.StatusCode;
                    return new T();
                }
            }
        }
    }
}