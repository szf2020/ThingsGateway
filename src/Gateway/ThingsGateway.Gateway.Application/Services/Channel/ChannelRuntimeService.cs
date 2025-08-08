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
    private Microsoft.Extensions.Logging.ILogger _logger;
    public ChannelRuntimeService(Microsoft.Extensions.Logging.ILogger<ChannelRuntimeService> logger)
    {
        _logger = logger;
    }
    private WaitLock WaitLock { get; set; } = new WaitLock(nameof(ChannelRuntimeService));


    public Task<TouchSocket.Core.LogLevel> ChannelLogLevelAsync(long id)
    {
        GlobalData.IdChannels.TryGetValue(id, out var ChannelRuntime);
        var data = ChannelRuntime?.DeviceThreadManage?.LogMessage?.LogLevel ?? TouchSocket.Core.LogLevel.Trace;
        return Task.FromResult(data);
    }

    public async Task RestartChannelAsync(long channelId)
    {
        GlobalData.IdChannels.TryGetValue(channelId, out var channelRuntime);
        await GlobalData.GetChannelThreadManage(channelRuntime).RestartChannelAsync(channelRuntime).ConfigureAwait(false);

    }

    public async Task SetChannelLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel)
    {
        if (GlobalData.IdChannels.TryGetValue(id, out var ChannelRuntime))
        {
            if (ChannelRuntime.DeviceThreadManage != null)
            {
                await ChannelRuntime.DeviceThreadManage.SetLogAsync(logLevel).ConfigureAwait(false);
            }
        }
    }




    public async Task CopyChannelAsync(int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long channelId, bool AutoRestartThread)
    {
        if (!GlobalData.IdChannels.TryGetValue(channelId, out var channelRuntime))
        {
            return;
        }
        Dictionary<Device, List<Variable>> deviceDict = new();
        Channel Model = channelRuntime.AdaptChannel();
        Model.Id = 0;

        var Devices = channelRuntime.ReadDeviceRuntimes.ToDictionary(a => a.Value.AdaptDevice(), a => a.Value.ReadOnlyVariableRuntimes.Select(a => a.Value).AdaptListVariable());

        List<Channel> channels = new();
        Dictionary<Device, List<Variable>> devices = new();
        for (int i = 0; i < CopyCount; i++)
        {
            Channel channel = Model.AdaptChannel();
            channel.Id = CommonUtils.GetSingleId();
            channel.Name = $"{CopyChannelNamePrefix}{CopyChannelNameSuffixNumber + i}";

            int index = 0;
            foreach (var item in Devices)
            {
                Device device = item.Key.AdaptDevice();
                device.Id = CommonUtils.GetSingleId();
                device.Name = $"{channel.Name}_{CopyDeviceNamePrefix}{CopyDeviceNameSuffixNumber + (index++)}";
                device.ChannelId = channel.Id;
                List<Variable> variables = new();

                foreach (var variable in item.Value)
                {
                    Variable v = variable.AdaptVariable();
                    v.Id = CommonUtils.GetSingleId();
                    v.DeviceId = device.Id;
                    variables.Add(v);
                }
                devices.Add(device, variables);
            }

            channels.Add(channel);
        }

        await GlobalData.ChannelRuntimeService.CopyAsync(channels, devices, AutoRestartThread, default).ConfigureAwait(false);
    }




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

    public async Task<bool> BatchEditAsync(IEnumerable<Channel> models, Channel oldModel, Channel model, bool restart)
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

    public async Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart)
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

    public async Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart)
    {

        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            var result = await GlobalData.ChannelService.ImportAsync(upData, insertData).ConfigureAwait(false);

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

    public async Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart)
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
            var channelIds = GlobalData.IdChannels.Keys.ToList();
            //网关启动时，获取所有通道
            var newChannelRuntimes = (await GlobalData.ChannelService.GetFromDBAsync((a => ids.Contains(a.Id) || !channelIds.Contains(a.Id))).ConfigureAwait(false)).AdaptListChannelRuntime();

            var chanelIds = newChannelRuntimes.Select(a => a.Id).ToHashSet();
            var newDeviceRuntimes = (await GlobalData.DeviceService.GetFromDBAsync((a => chanelIds.Contains(a.ChannelId))).ConfigureAwait(false)).AdaptListDeviceRuntime();

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