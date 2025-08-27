//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text;

using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi.Swagger;

namespace ThingsGateway.Gateway.Application;

public partial class WebApiTask : AsyncDisposableObject
{
    internal const string LogPath = $"Logs/{nameof(WebApiTask)}";
    private ILog LogMessage;
    private ILogger _logger;
    private TextFileLogger TextLogger;

    public WebApiTask(ILogger logger, WebApiOptions webApiOptions)
    {
        _logger = logger;
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;

        _webApiOptions = webApiOptions;

    }

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_webApiOptions.Enable) return;

        _httpService ??= await GetHttpService().ConfigureAwait(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, "Start");
            }
            finally
            {
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }

        }
    }

    private HttpService? _httpService;
    private WebApiOptions _webApiOptions;


    private async Task<HttpService> GetHttpService()
    {
        var httpService = new HttpService();
        var config = new TouchSocketConfig()
               .SetListenIPHosts(_webApiOptions.ServerUri)
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<ControlController>();
                       store.RegisterServer<RuntimeInfoController>();
                       store.RegisterServer<TestController>();

                       store.RegisterServer<IManagementRpcServer>(new ManagementRpcServer());
                       store.RegisterServer<IUpgradeRpcServer>(new UpgradeRpcServer());

                       foreach (var type in App.EffectiveTypes.Where(p => typeof(IPluginRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
                       {
                           store.RegisterServer(type);
                       }
                   });

                   //添加跨域服务
                   a.AddCors(corsOption =>
                   {
                       //添加跨域策略，后续使用policyName即可应用跨域策略。
                       corsOption.Add("cors", corsBuilder =>
                       {
                           corsBuilder.AllowAnyMethod()
                               .AllowAnyOrigin();
                       });
                   });


               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();

                   a.Add(new AuthenticationPlugin(_webApiOptions));

                   a.UseWebApi();

#if DEBUG
                   a.UseSwagger().SetPrefix("api");
#else
                   if (App.WebHostEnvironment.IsDevelopment())
                       a.UseSwagger().SetPrefix("api");
#endif
                   a.UseDefaultHttpServicePlugin();
               });

        await httpService.SetupAsync(config).ConfigureAwait(false);
        return httpService;
    }

    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (_httpService.ServerState != ServerState.Running)
        {
            if (_httpService.ServerState != ServerState.Stopped)
                await _httpService.StopAsync(cancellationToken).ConfigureAwait(false);

            await _httpService.StartAsync().ConfigureAwait(false);
        }

    }


    protected override async Task DisposeAsync(bool disposing)
    {

        if (_httpService != null)
        {
            await _httpService.ClearAsync().ConfigureAwait(false);
            _httpService.SafeDispose();
            _httpService = null;
        }
        await base.DisposeAsync(disposing).ConfigureAwait(false);
        TextLogger?.Dispose();
    }
}
/// <summary>
/// 鉴权插件
/// </summary>
class AuthenticationPlugin : PluginBase, IHttpPlugin
{
    WebApiOptions _webApiOptions;
    public AuthenticationPlugin(WebApiOptions webApiOptions)
    {
        _webApiOptions = webApiOptions;
    }
    public Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        string authorizationHeader = e.Context.Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authorizationHeader))
        {

            e.Context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"ThingsGateway\"");
            return e.Context.Response
                     .SetStatus(401, "Empty Authorization Header")
                     .AnswerAsync();
        }

        if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            e.Context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"ThingsGateway\"");
            return e.Context.Response
     .SetStatus(401, "Invalid Authorization Header")
     .AnswerAsync();
        }

        var authBase64 = authorizationHeader.Substring("Basic ".Length).Trim();
        var authBytes = Convert.FromBase64String(authBase64);
        var authString = Encoding.UTF8.GetString(authBytes);
        var credentials = authString.Split(':', 2);

        if (credentials.Length != 2)
        {
            e.Context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"ThingsGateway\"");
            return e.Context.Response
.SetStatus(401, "Invalid Authorization Header")
.AnswerAsync();
        }

        var username = credentials[0];
        var password = credentials[1];

        if (username != _webApiOptions.UserName || password != _webApiOptions.Password)
        {
            e.Context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"ThingsGateway\"");
            return e.Context.Response
.SetStatus(401, "Invalid Username or Password")
.AnswerAsync();
        }

        return e.InvokeNext();
    }
}