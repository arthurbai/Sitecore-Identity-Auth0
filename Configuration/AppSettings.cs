namespace Sitecore.Plugin.IdentityProvider.Auth0.Configuration
{
    public class AppSettings
    {
        public static readonly string SectionName = "Sitecore:ExternalIdentityProviders:IdentityProviders:Auth0";

        public Auth0IdentityProvider Auth0IdentityProvider { get; set; } = new Auth0IdentityProvider();
    }
}
