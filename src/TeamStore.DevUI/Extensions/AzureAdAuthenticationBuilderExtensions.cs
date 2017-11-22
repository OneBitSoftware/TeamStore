using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeamStore.Keeper.Interfaces;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect(options =>
            {
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = async context =>
                    {
                        // If any of the auth handshaking causes an error/exception, the methods below will log the exception.
                        // here, we still catch the failure Event, log and gracefully handle
                        if (context != null && context.Failure != null)
                        {
                            var telemetryService = context.HttpContext.RequestServices.GetService<ITelemetryService>();
                            telemetryService.TrackException(context.Failure);
                        }
                    },
                    OnTokenValidated = async context =>
                    {
                        var claimIdentity = (ClaimsIdentity)context.Principal.Identity;
                        var claimsPrincipal = context.Principal.Identity;

                        // Store login event
                        var eventService = context.HttpContext.RequestServices.GetService<IEventService>();

                        if (eventService == null) throw new Exception("EventService not found. Terminating.");

                        string accessIpAddress = string.Empty;
                        if (context.HttpContext != null)
                        {
                            accessIpAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                        }

                        var telemetryService = context.HttpContext.RequestServices.GetService<ITelemetryService>();
                        telemetryService.TraceEvent("Login success", "login", claimsPrincipal.Name);

                        await eventService.LogLoginEventAsync(claimIdentity, accessIpAddress);
                    }
                    ,
                    OnAuthorizationCodeReceived = async context =>
                    {
                        var claimIdentity = context.Principal.Claims;
                        var graphService = context.HttpContext.RequestServices.GetService<IGraphService>();

                        var code = context.ProtocolMessage.Code; // null check
                        // Can't find the object identifier enum, using string
                        // TODO null check here! -> extract to helper?
                        var identifier = context.Principal.Claims.First(item => item.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                        var redirectHost = context.Request.Scheme + "://" + context.Request.Host.Value;

                        var result = await graphService.GetTokenByAuthorizationCodeAsync(identifier, code, redirectHost);

                        if (result != null)
                        {
                            context.HandleCodeRedemption(result.AccessToken, result.IdToken);
                        }
                        else
                        {
                            var telemetryService = context.HttpContext.RequestServices.GetService<ITelemetryService>();
                            telemetryService.TraceEvent(
                                "Client Secret Error",
                                "GraphServiceError",
                                "Token was not received from the Graph Service. Check App Insights exceptions, or the directory configuration in appsettings.json.");
                        }
                    }
                };

                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            });
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
