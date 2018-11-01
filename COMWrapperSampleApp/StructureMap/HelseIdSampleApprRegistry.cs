using COMWrapperSampleApp.BrowserLogic;
using COMWrapperSampleApp.Configuration;
using COMWrapperSampleApp.Logic;
using StructureMap.Configuration.DSL;

namespace COMWrapperSampleApp.StructureMap
{
    public class COMWrapperSampleAppRegistry : Registry
    {
        public COMWrapperSampleAppRegistry()
        {
            For<IBrowserFactory>().Singleton().Use<BrowserFactory>();
            For<IAuthenticationOperations>().Singleton().Use<AuthenticationOperations>();
            For<IConfigurationsProvider>().Singleton().Use<ConfigurationsProvider>();
            For<IApiOperations>().Singleton().Use<ApiOperations>();
            For<IEpjApiSerializer>().Singleton().Use<EpjApiSerializer>();
            For<IConfigurations>().Use<Configurations>();
        }
    }
}