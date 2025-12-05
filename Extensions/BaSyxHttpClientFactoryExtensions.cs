using System;
using BaSyx.Clients.AdminShell.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using System.Net.Http;
using AasSharpClient.Adapters;
using AasSharpClient.Models.Remote;

namespace AasSharpClient.Extensions
{
    public static class BaSyxHttpClientFactoryExtensions
    {
        // Register a factory Func<Uri, SubmodelHttpClient> that uses IHttpClientFactory to create
        // named HttpClient instances and wraps them with HttpClientMessageHandlerAdapter so they can
        // be passed into BaSyx SubmodelHttpClient constructors.
        public static IServiceCollection AddBaSyxSubmodelClientFactory(this IServiceCollection services, string httpClientName = "basyx")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Register the factory which resolves IHttpClientFactory from the DI container
            services.TryAddSingleton<Func<Uri, SubmodelHttpClient>>(sp => (uri) =>
            {
                var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpFactory.CreateClient(httpClientName);
                var handlerAdapter = new HttpClientMessageHandlerAdapter(httpClient);
                // SubmodelHttpClient has a ctor (Uri endpoint, HttpMessageHandler messageHandler)
                return new SubmodelHttpClient(uri, handlerAdapter);
            });

            return services;
        }

        // Convenience registration for library consumers: registers the BaSyx client factory
        // and the default RemoteScheduleSyncService so a consumer only needs to call this
        // single extension from their application's DI setup.
        public static IServiceCollection AddBaSyxClientServices(this IServiceCollection services, string httpClientName = "basyx")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Register the submodel client factory
            services.AddBaSyxSubmodelClientFactory(httpClientName);

            // Register the remote sync service (scoped by default)
            services.TryAddScoped<IRemoteScheduleSyncService, RemoteScheduleSyncService>();

            return services;
        }
    }
}
