namespace Sitecore.Plugin.IdentityProvider.Auth0
{
    public class Auth0IdentityProvider : IdentityProviders.IdentityProvider
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Domain { get; set; }

        public string Scopes { get; set; }

        public string ClaimsIssuer { get; set; }

        public string SignedOutRedirectUri { get; set; }
    }
}
