//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

using System.Globalization;

using ThingsGateway.Foundation;

namespace ThingsGateway.Debug;

[AppStartup(100000000)]
public class Startup : AppStartup
{
    public void Configure(IServiceCollection services)
    {
        Foundation.LocalizerUtil.SetLocalizerFactory((a) => App.CreateLocalizerByType(a));

        if (CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            AppResource.Lang = Language.Chinese;
        }
        else
        {
            AppResource.Lang = Language.English;
        }

        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<IDownloadPlatformService, DownloadPlatformService>();
        services.AddScoped<PlatformService, PlatformService>();
        services.AddSingleton<ITextFileReadService, TextFileReadService>();
        services.AddSingleton<TextFileReadService, TextFileReadService>();
    }
}
