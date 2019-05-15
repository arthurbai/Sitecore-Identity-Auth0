using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Framework.Runtime.Configuration;
using System;
using System.Threading.Tasks;

namespace Sitecore.Plugin.IdentityProvider.Auth0
{
    using Configuration;

    public class ConfigureSitecore
    {
        private readonly ILogger<ConfigureSitecore> _logger;
        private readonly AppSettings _appSettings;

        public ConfigureSitecore(ISitecoreConfiguration scConfig, ILogger<ConfigureSitecore> logger)
        {
            _logger = logger;
            _appSettings = new AppSettings();
            scConfig.GetSection(AppSettings.SectionName);
            scConfig.GetSection(AppSettings.SectionName).Bind(_appSettings.Auth0IdentityProvider);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Auth0IdentityProvider auth0Provider = _appSettings.Auth0IdentityProvider;
            if (!auth0Provider.Enabled)
                return;

            _logger.LogDebug("Configure '" + auth0Provider.DisplayName + "'. AuthenticationScheme = " + auth0Provider.AuthenticationScheme + ", ClientId = " + auth0Provider.ClientId, Array.Empty<object>());
            new AuthenticationBuilder(services).AddOpenIdConnect(auth0Provider.AuthenticationScheme, auth0Provider.DisplayName, options =>
            {
                options.Authority = $"https://{auth0Provider.Domain}";
                options.ClientId = auth0Provider.ClientId;
                options.ClientSecret = auth0Provider.ClientSecret;
                options.ResponseType = "id_token";
                options.SignInScheme = "idsrv.external";
                options.SignedOutRedirectUri = auth0Provider.SignedOutRedirectUri;

                options.Scope.Clear();
                if (string.IsNullOrEmpty(auth0Provider.Scopes))
                {
                    foreach (string scope in auth0Provider.Scopes.Split(' '))
                    {
                        options.Scope.Add(scope);
                    }
                }

                options.CallbackPath = new PathString("/callback");
                options.ClaimsIssuer = auth0Provider.ClaimsIssuer;

                options.Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = (context) =>
                    {
                        var debugClaims = context.Principal?.Claims;
                        return Task.CompletedTask;
                    },
                    OnTokenResponseReceived = (context) =>
                    {
                        var debugClaims = context.Principal?.Claims;
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{auth0Provider.Domain}/v2/logout?client_id={auth0Provider.ClientId}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
