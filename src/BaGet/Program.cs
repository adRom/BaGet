using System.Threading.Tasks;
using BaGet.Core;
using BaGet.Hosting;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaGet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "baget",
                Description = "A light-weight NuGet service",
            };

            app.HelpOption(inherited: true);

            app.Command("import", import =>
            {
                import.Command("downloads", downloads =>
                {
                    downloads.OnExecuteAsync(async cancellationToken =>
                    {
                        var host = CreateHostBuilder(args).Build();
                        var importer = host.Services.GetRequiredService<DownloadsImporter>();

                        await importer.ImportAsync(cancellationToken);
                    });
                });
            });

            app.OnExecuteAsync(async cancellationToken =>
            {
                var host = CreateWebHostBuilder(args).Build();

                await host.RunMigrationsAsync(cancellationToken);
                await host.RunAsync(cancellationToken);
            });

            // TODO next line should be
            // await app.ExecuteAsync(args);
            // but there is an issue in aspnetcore which causes an error on startup when debugging
            // https://developercommunityapi.westus.cloudapp.azure.com/content/problem/845413/aspnet-core-web-project-gets-passed-a-launcher-arg.html
            await app.ExecuteAsync(default);
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            CreateHostBuilder(args)
                .ConfigureWebHostDefaults(web =>
                {
                    web.ConfigureKestrel(options =>
                    {
                        // Remove the upload limit from Kestrel. If needed, an upload limit can
                        // be enforced by a reverse proxy server, like IIS.
                        options.Limits.MaxRequestBodySize = null;
                    });

                    web.UseStartup<Startup>();
                });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseBaGet();
    }
}
