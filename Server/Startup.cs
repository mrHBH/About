using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using HBHplayground.Abstractions;
using HBHplayground.Server.Services;
using Microsoft.AspNetCore.StaticFiles;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.EntityFramework.Internal;
using Stl.Fusion.Server;
using Stl.IO;


namespace HBHplayground.Server
{
    public class Startup
    {
        private IConfiguration Cfg { get; }
        private IWebHostEnvironment Env { get; }
        private ILogger Log { get; set; } = NullLogger<Startup>.Instance;

        public Startup(IConfiguration cfg, IWebHostEnvironment environment)
        {
            Cfg = cfg;
            Env = environment;
        }
        
        private string GetHerokuConnectionString()
        {
            // Get the connection string from the ENV variables
            string connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            // parse the connection string
            var databaseUri = new Uri(connectionUrl);

            string db = databaseUri.LocalPath.TrimStart('/');
            string[] userInfo = databaseUri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);

            return $"User ID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};Database={db};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;";
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(logging => {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                if (Env.IsDevelopment())
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
            });

            // DbContext & related services
            var appTempDir = PathEx.GetApplicationTempDirectory("", true);
            var dbPath = appTempDir & "App_v09x.db";
            try
            {
                services.AddDbContextFactory<AppDbContext>(b => {
                       b.UseSqlite($"Data Source={dbPath}", sqlite => { });
                //  b.UseNpgsql(GetHerokuConnectionString());
                    // services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(GetHerokuConnectionString()));
                });

            }
            catch (Exception)
            {

                throw;
            }
         
            if (Env.IsDevelopment()) {
                // Let's slow down DbOperationLogWatcher in dev mode -
                // al least to avoid log pollution with its SQL commands.
                services.AddSingleton(_ => {
                    var options = new DbOperationLogWatcher<AppDbContext>.Options();
                    options.CheckInterval = TimeSpan.FromMinutes(1);
                    options.MaxCommitDuration = options.CheckInterval * 2.1; // The multiplier should be >2 :)
                    return options;
                });
            }
            services.AddDbContextServices<AppDbContext>(b => {
                // This is the best way to add DbContext-related services from Stl.Fusion.EntityFramework
                b.AddOperations();
                b.AddEntityResolver<long, ChatUser>();
                b.AddEntityResolver<long, ChatMessage>();
            });

            // Fusion services
            services.AddSingleton(c => {
                var serverSettings = c.GetRequiredService<ServerSettings>();
                return new Publisher.Options() { Id = serverSettings.PublisherId };
            });
            services.AddSingleton(new PresenceService.Options() { UpdatePeriod = TimeSpan.FromMinutes(1) });
            var fusion = services.AddFusion();
            var fusionServer = fusion.AddWebSocketServer();
            var fusionClient = fusion.AddRestEaseClient();
            var fusionAuth = fusion.AddAuthentication().AddServer();
            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeScanner().AddServicesFrom(Assembly.GetExecutingAssembly());
            // Registering shared services from the client
            UI.Program.ConfigureSharedServices(services);

            services.AddAuthentication(options => {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options => {
                    options.LoginPath = "/signin";
                    options.LogoutPath = "/signout";
                })
                .AddGitHub(options => {
                    options.Scope.Add("read:user");
                    // options.Scope.Add("user:email");
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                });
            // We want to get ClientId and ClientSecret from ServerSettings,
            // and they're available only when IServiceProvider is already created,
            // that's why this overload of Configure<TOptions> is used here.
            services.Configure<GitHubAuthenticationOptions>((c, name, options) => {
                var serverSettings = c.GetRequiredService<ServerSettings>();
                options.ClientId = serverSettings.GitHubClientId;
                options.ClientSecret = serverSettings.GitHubClientSecret;
            });

            // Web
            services.AddRouting();
            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());
            services.AddServerSideBlazor(o => o.DetailedErrors = true);
            fusionAuth.AddBlazor(o => {}); // Must follow services.AddServerSideBlazor()!


            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "HBHplayground.Server API", Version = "v1"
                });
            });
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> log)
        {
            Log = log;

            // This server serves static content from Blazor Client,
            // and since we don't copy it to local wwwroot,
            // we need to find Client's wwwroot in bin/(Debug/Release) folder
            // and set it as this server's content root.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var wwwRootPath = Path.Combine(baseDir, "wwwroot");
            if (!Directory.Exists(Path.Combine(wwwRootPath, "_framework")))
                // This is a regular build, not a build produced w/ "publish",
                // so we remap wwwroot to the client's wwwroot folder
                wwwRootPath = Path.GetFullPath(Path.Combine(baseDir, $"../../../../UI/{binCfgPart}/net5.0/wwwroot"));
            Env.WebRootPath = wwwRootPath;
            Env.WebRootFileProvider = new PhysicalFileProvider(Env.WebRootPath);
            StaticWebAssetsLoader.UseStaticWebAssets(Env, Cfg);

            if (Env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            
            var provider = new FileExtensionContentTypeProvider();
            
            provider.Mappings.Add(".dae", "text/xml");
            provider.Mappings.Add(".glb", "text/xml");
            provider.Mappings.Add(".fbx", "text/xml");
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            
            
            app.UseWebSockets(new WebSocketOptions() {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
            });
            app.UseFusionSession();

            // Static + Swagger
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            // API controllers
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
