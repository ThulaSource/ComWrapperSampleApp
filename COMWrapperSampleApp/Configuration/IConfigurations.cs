
namespace COMWrapperSampleApp.Configuration
{
    public interface IConfigurations
    {
        /// <summary>
        /// The SFM Router endpoint
        /// </summary>
        string SfmRouterEndpoint { get; }

        /// <summary>
        /// The HelseId endpoint
        /// </summary>
        string HelseIdEndpoint { get; }

        /// <summary>
        /// The HelseId redirect URL to handle access tokens
        /// </summary>
        string HelseIdRedirectUri { get; }

        /// <summary>
        /// The HelseId registered client id
        /// </summary>
        string HelseIdClientId { get; }

        /// <summary>
        /// The HelseId scopes registered for the supplied HelseId client id
        /// </summary>
        string HelseIdScope { get; }
    }

    public class Configurations : IConfigurations
    {
        private readonly IConfigurationsProvider configProvider;

        public Configurations(IConfigurationsProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public string HelseIdEndpoint => configProvider.Configurations.AppSettings(Constants.HelseIdEndpointKey);
        public string HelseIdRedirectUri => configProvider.Configurations.AppSettings(Constants.HelseIdRedirectUriKey);
        public string HelseIdClientId => configProvider.Configurations.AppSettings(Constants.HelseIdClientIdKey);
        public string HelseIdScope => configProvider.Configurations.AppSettings(Constants.HelseIdScopeKey);

        public string SfmRouterEndpoint => configProvider.Configurations.AppSettings(Constants.SfmRouterEndpointKey);
    }
}