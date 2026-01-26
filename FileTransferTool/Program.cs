using FileTransferTool.Interfaces;
using FileTransferTool.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
internal class Program
{
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddScoped<ITransferFileService, TransferFileService>();
                services.AddScoped<IApplicationService, ApplicationService>();
            })
            .Build();

        var app = host.Services.GetRequiredService<IApplicationService>();
        await app.RunAsync();
    }
}