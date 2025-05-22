using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PCLinkServer;

using System;
using System.IO;
using System.Threading.Tasks;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;
// using FubarDev.FtpServer.ConnectionHandling;


public class FtpServer
{
    public static async Task RunFtpServerAsync(string rootPath, int port = 2121)
    {
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<DotNetFileSystemOptions>(opt =>
                {
                    opt.RootPath = rootPath;
                });

                services.AddFtpServer(builder => builder
                    .UseDotNetFileSystem()
                    .EnableAnonymousAuthentication());
                services.AddSingleton<IMembershipProvider, AllowAnonymousMembershipProvider>();

                services.Configure<FtpServerOptions>(opt =>
                {
                    opt.ServerAddress = "0.0.0.0";
                    opt.Port = port;
                });

                // services.Configure<PassiveModeOptions>(opt =>
                // {
                //     opt.PasvMinPort = 50000;
                //     opt.PasvMaxPort = 50010;
                // });
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .Build();

        Console.WriteLine($"FTP-сервер запускается на 127.0.0.1:{port}, корневая папка: {rootPath}");

        // Запускаем сервер вручную после запуска хоста
        await host.StartAsync();

        var ftpServerHost = host.Services.GetRequiredService<IFtpServerHost>();
        await ftpServerHost.StartAsync(CancellationToken.None);

        Console.WriteLine("FTP-сервер запущен. Нажмите Ctrl+C для остановки.");

        await host.WaitForShutdownAsync();
    }


}
public class AllowAnonymousMembershipProvider : IMembershipProvider
{
    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        if (username.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser));
        }

        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
    }
}

public class MyCustomMembershipProvider : IMembershipProvider
{
    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        if (username == "ftpuser" && password == "1234")
        {
            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser));
        }

        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
    }
}

