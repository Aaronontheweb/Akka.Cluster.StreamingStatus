using Akka.Cluster.StreamingStatus.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Akka.Cluster.StreamingStatus
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddClusterStatusServices();
            services.AddSignalR();
            services.AddSignalRAkkaStream(); // Makes IStreamDispatcher available
            
              // creates an instance of the ISignalRProcessor that can be handled by SignalR
            services.AddSingleton<IClusterMonitor, AkkaService>();

            // starts the IHostedService, which creates the ActorSystem and actors
            services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IClusterMonitor>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(ep =>
            {
                ep.MapRazorPages();
                ep.MapHub<ClusterStatusHub>("/hubs/clusterStatus");
            });
        }
    }
}
