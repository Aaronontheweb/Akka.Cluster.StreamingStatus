using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.StreamingStatus.Actors;
using Akka.Cluster.StreamingStatus.Hubs;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace Akka.Cluster.StreamingStatus
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAkkaSettings();
            services.AddClusterStatusServices();
            services.AddSignalR();
            services.AddSignalRAkkaStream(); // Makes IStreamDispatcher available

            services.AddAkka("ClusterSys", (builder, provider) =>
            {
                var akkaSettings = provider.GetRequiredService<IOptions<AkkaSettings>>().Value;
                
                builder
                    .WithRemoting(akkaSettings.RemoteOptions!)
                    .WithClustering(akkaSettings.ClusterOptions)
                    .WithActors((system, registry, resolver) =>
                    {
                        var clusterStatusManager = system.ActorOf(Props.Create(() => new ClusterStatusManager()),
                            "clusterStatusManager");
                        registry.Register<ClusterStatusManager>(clusterStatusManager);
                    })
                    .AddPetabridgeCmd(cmd =>
                    {
                        cmd.RegisterCommandPalette(ClusterCommands.Instance);
                        cmd.RegisterCommandPalette(new RemoteCommands());
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(ep =>
            {
                ep.MapHub<ClusterStatusHub>("/hubs/clusterStatus");
            });
        }
    }
}
