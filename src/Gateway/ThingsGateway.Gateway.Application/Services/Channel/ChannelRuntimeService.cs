// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

using ThingsGateway.NewLife;

namespace ThingsGateway.Gateway.Application;

public class ChannelRuntimeService : IChannelRuntimeService
{
    private ILogger _logger;
    public ChannelRuntimeService(ILogger<ChannelRuntimeService> logger)
    {
        _logger = logger;
    }
    private WaitLock WaitLock { get; set; } = new WaitLock();

    public async Task<bool> CopyAsync(List<Channel> models, Dictionary<Device, List<Variable>> devices, bool restart, CancellationToken cancellationToken)
    {
        try
        {
            await WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            var result = await GlobalData.ChannelService.CopyAsync(models, devices).ConfigureAwait(false);
            var ids = models.Select(a => a.Id).ToHashSet();

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(ids).ConfigureAwait(false);

            var deviceids = devices.Select(a => a.Key.Id).ToHashSet();
            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            await RuntimeServiceHelper.InitAsync(newChannelRuntimes, newDeviceRuntimes, _logger).ConfigureAwait(false);

            //根据条件重启通道线程
            if (restart)
            {
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

                await RuntimeServiceHelper.ChangedDriverAsync(_logger, cancellationToken).ConfigureAwait(false);

            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> InsertAsync(List<Channel> models, List<Device> devices, List<Variable> variables, bool restart, CancellationToken cancellationToken)
    {
        try
        {
            await WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            var result = await GlobalData.ChannelService.InsertAsync(models, devices, variables).ConfigureAwait(false);
            var ids = models.Select(a => a.Id).ToHashSet();

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(ids).ConfigureAwait(false);

            var deviceids = devices.Select(a => a.Id).ToHashSet();
            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            await RuntimeServiceHelper.InitAsync(newChannelRuntimes, newDeviceRuntimes, _logger).ConfigureAwait(false);

            //根据条件重启通道线程
            if (restart)
            {
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

                await RuntimeServiceHelper.ChangedDriverAsync(_logger, cancellationToken).ConfigureAwait(false);

            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> UpdateAsync(List<Channel> models, List<Device> devices, List<Variable> variables, bool restart, CancellationToken cancellationToken)
    {
        try
        {
            await WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            var result = await GlobalData.ChannelService.UpdateAsync(models, devices, variables).ConfigureAwait(false);
            var ids = models.Select(a => a.Id).ToHashSet();

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(ids).ConfigureAwait(false);

            var deviceids = devices.Select(a => a.Id).ToHashSet();
            var newDeviceRuntimes = await RuntimeServiceHelper.GetNewDeviceRuntimesAsync(deviceids).ConfigureAwait(false);

            await RuntimeServiceHelper.InitAsync(newChannelRuntimes, newDeviceRuntimes, _logger).ConfigureAwait(false);

            //根据条件重启通道线程
            if (restart)
            {
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

                await RuntimeServiceHelper.ChangedDriverAsync(_logger, cancellationToken).ConfigureAwait(false);

            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> BatchEditAsync(IEnumerable<Channel> models, Channel oldModel, Channel model, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.BatchEditAsync(models, oldModel, model).ConfigureAwait(false);
            var ids = models.Select(a => a.Id).ToHashSet();
            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(ids).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newChannelRuntimes);

            //根据条件重启通道线程
            if (restart)
            {
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);
            }

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }

    public async Task<bool> DeleteChannelAsync(IEnumerable<long> ids, bool restart, CancellationToken cancellationToken)
    {
        try
        {
            await WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            var array = ids.ToArray();
            var result = await GlobalData.ChannelService.DeleteChannelAsync(array).ConfigureAwait(false);

            var changedDriver = RuntimeServiceHelper.DeleteChannelRuntime(array);

            //根据条件重启通道线程
            if (restart)
            {
                await GlobalData.ChannelThreadManage.RemoveChannelAsync(array).ConfigureAwait(false);

                await RuntimeServiceHelper.ChangedDriverAsync(changedDriver, _logger, cancellationToken).ConfigureAwait(false);
            }

            return true;

        }
        finally
        {
            WaitLock.Release();
        }
    }

    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile) => GlobalData.ChannelService.PreviewAsync(browserFile);

    public Task<Dictionary<string, object>> ExportChannelAsync(ExportFilter exportFilter) => GlobalData.ChannelService.ExportChannelAsync(exportFilter);

    public Task<MemoryStream> ExportMemoryStream(IEnumerable<Channel> data) =>
      GlobalData.ChannelService.ExportMemoryStream(data);

    public async Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.ImportChannelAsync(input).ConfigureAwait(false);

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(result).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newChannelRuntimes);

            //根据条件重启通道线程
            if (restart)
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

        }

        finally
        {
            WaitLock.Release();
        }
    }
    public async Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart = true)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.SaveChannelAsync(input, type).ConfigureAwait(false);

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(new HashSet<long>() { input.Id }).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newChannelRuntimes);

            //根据条件重启通道线程
            if (restart)
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }


    public async Task<bool> BatchSaveChannelAsync(List<Channel> input, ItemChangedType type, bool restart)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.BatchSaveAsync(input, type).ConfigureAwait(false);

            var newChannelRuntimes = await RuntimeServiceHelper.GetNewChannelRuntimesAsync(input.Select(a => a.Id).ToHashSet()).ConfigureAwait(false);

            RuntimeServiceHelper.Init(newChannelRuntimes);

            //根据条件重启通道线程
            if (restart)
                await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

            return true;
        }
        finally
        {
            WaitLock.Release();
        }
    }


    public async Task RestartChannelAsync(IEnumerable<ChannelRuntime> oldChannelRuntimes)
    {
        RuntimeServiceHelper.RemoveOldChannelRuntimes(oldChannelRuntimes);
        var ids = oldChannelRuntimes.Select(a => a.Id).ToHashSet();
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            //网关启动时，获取所有通道
            var newChannelRuntimes = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).Where(a => ids.Contains(a.Id) || !GlobalData.IdChannels.ContainsKey(a.Id)).AdaptListChannelRuntime();

            var chanelIds = newChannelRuntimes.Select(a => a.Id).ToHashSet();
            var newDeviceRuntimes = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).Where(a => chanelIds.Contains(a.ChannelId)).AdaptListDeviceRuntime();

            await RuntimeServiceHelper.InitAsync(newChannelRuntimes, newDeviceRuntimes, _logger).ConfigureAwait(false);


            await GlobalData.ChannelThreadManage.RestartChannelAsync(newChannelRuntimes).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Start error");
        }
        finally
        {
            WaitLock.Release();
        }
    }


}