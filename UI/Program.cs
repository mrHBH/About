using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pluralize.NET;
using HBHplayground.Abstractions;
using HBHplayground.Client;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.OS;
using Stl.DependencyInjection;
using Stl.Fusion.Blazor;
using MudBlazor;
using MudBlazor.Services;

namespace HBHplayground.UI
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            if (!OSInfo.IsWebAssembly)
                throw new ApplicationException("This app runs only in browser.");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            ConfigureServices(builder.Services, builder);
            builder.RootComponents.Add<App>("#app");
            var host = builder.Build();

            var runTask = host.RunAsync();
            Task.Run(async () => {
                // We "manually" start IHostedServices here, because Blazor host doesn't do this.
                var hostedServices = host.Services.GetRequiredService<IEnumerable<IHostedService>>();
                foreach (var hostedService in hostedServices)
                    await hostedService.StartAsync(default);
            });
            return runTask;
        }

        public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
            var apiBaseUri = new Uri($"{baseUri}api/");

            var fusion = services.AddFusion();
            var fusionClient = fusion.AddRestEaseClient(
                (c, o) => {
                    o.BaseUri = baseUri;
                    o.MessageLogLevel = LogLevel.Information;
                }).ConfigureHttpClientFactory(
                (c, name, o) => {
                    var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                    var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                    o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
                });
            var fusionAuth = fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeScanner(Scopes.ClientSideOnly).AddServicesFrom(typeof(ITimeClient).Assembly);
            ConfigureSharedServices(services);
        }

        public static void ConfigureSharedServices(IServiceCollection services)
        {

            services.AddMudBlazorDialog();
            services.AddMudBlazorSnackbar();
            services.AddMudBlazorResizeListener();

            // Default delay for update delayers
            services.AddSingleton(c => new UpdateDelayer.Options() {
                Delay = TimeSpan.FromSeconds(0.1),
            });

            services.AddSingleton<IPluralize, Pluralizer>();

            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeScanner().AddServicesFrom(Assembly.GetExecutingAssembly());
        }
    }
}
