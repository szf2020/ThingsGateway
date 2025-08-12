//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

using ThingsGateway.NewLife;

using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

public partial class UpgradeRpcServer : IRpcServer, IUpgradeRpcServer
{

    [DmtpRpc]
    public async Task UpgradeAsync(ICallContext callContext, UpdateZipFile updateZipFile)
    {
        if (callContext.Caller is IDmtpActorObject dmtpActorObject)
            await Update(dmtpActorObject.DmtpActor, updateZipFile).ConfigureAwait(false);
    }
    [DmtpRpc]
    public Task<UpdateZipFileInput> GetUpdateZipFileInputAsync(ICallContext callContext)
    {
        return Task.FromResult(new UpdateZipFileInput()
        {
            Version = Assembly.GetEntryAssembly().GetName().Version,
            DotNetVersion = Environment.Version,
            OSPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "OSX" : "Unknown",
            Architecture = RuntimeInformation.ProcessArchitecture,
            AppName = "ThingsGateway"
        });
    }

    public static async Task Update(IDmtpActor dmtpActor, UpdateZipFile updateZipFile, Func<Task<bool>> check = null)
    {
        try
        {
            await UpdateWaitLock.WaitAsync().ConfigureAwait(false);

            if (WaitLock.Waited)
            {
                throw new("Updating, please try again later");
            }
            try
            {
                await WaitLock.WaitAsync().ConfigureAwait(false);
                RestartServerHelper.DeleteAndBackup();

                var result = await FileServerHelpers.ClientPullFileFromService(dmtpActor, updateZipFile.FilePath, FileConst.UpgradePath).ConfigureAwait(false);
                if (result)
                {
                    if (check != null)
                        result = await check.Invoke().ConfigureAwait(false);
                    if (result)
                    {
                        RestartServerHelper.ExtractUpdate();
                    }
                }
            }
            finally
            {
                WaitLock.Release();
            }
        }
        finally
        {
            UpdateWaitLock.Release();
        }
    }



    private static readonly WaitLock WaitLock = new(nameof(ManagementTask));
    private static readonly WaitLock UpdateWaitLock = new(nameof(ManagementTask));
}


