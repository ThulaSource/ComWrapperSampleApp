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
            var endpoint = configurations.ApiEndpoint;
            if (!endpoint.EndsWith("/"))
            {
                endpoint += "/";
            }

            endpoint += methodName;

            //Read access and id_token from COM call

            var accessToken = parameter?.LoginInfo?.AccessToken;
            var idToken = parameter?.LoginInfo?.IdToken;
            var tokens = $"access_token={accessToken}&id_token={idToken}";

            // There are now tokens present -> Redirect to loogin
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(idToken))
            {
                if (!authentication.IsAuthenticated)
                {
                    try
                    {
                        authentication.PromptForUserLogin();
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

            var httpHandler = new HttpClientHandler() { AllowAutoRedirect = false };

            // Call SFM API to get SFM client URL and patient ticket
            using (var httpClient = new HttpClient(httpHandler))
            {
                using (var stringContent = new StringContent(((XTypedElement)parameter).Untyped.ToString(), Encoding.UTF8, "application/xml"))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                    var response = httpClient.PostAsync(endpoint, stringContent).Result;
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        response.Headers.TryGetValues("ClientUrl", out var url);
                        var clientUrl = url.First();

                        var host = clientUrl;
                        if (!host.Contains("://"))
                        {
                            host = $"http://{clientUrl}";
                        }

                        var hostUrl = new Uri(host);
                        browser.UpdateMainFormTitle($"SFM COM Wrapper Sample (Pasient : {fnr}) @{hostUrl.Host}");

                        browser.Browser.Navigate($"{clientUrl}#{tokens}");
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