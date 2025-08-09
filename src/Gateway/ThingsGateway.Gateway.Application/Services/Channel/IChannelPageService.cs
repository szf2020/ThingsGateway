//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Gateway.Application
{
    public interface IChannelPageService
    {
        Task RestartChannelAsync(long channelId);

        Task<TouchSocket.Core.LogLevel> ChannelLogLevelAsync(long id);
        Task SetChannelLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);
        Task CopyChannelAsync(int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long channelId, bool AutoRestartThread);



        /// <summary>
        /// 保存通道
        /// </summary>
        /// <param name="input">通道对象</param>
        /// <param name="type">保存类型</param>
        /// <param name="restart">重启</param>
        Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart);

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="models">列表</param>
        /// <param name="oldModel">旧数据</param>
        /// <param name="model">新数据</param>
        /// <param name="restart">重启</param>
        /// <returns></returns>
        Task<bool> BatchEditChannelAsync(IEnumerable<Channel> models, Channel oldModel, Channel model, bool restart);




        /// <summary>
        /// 删除通道
        /// </summary>
        Task<bool> DeleteChannelAsync(IEnumerable<long> ids, bool restart, CancellationToken cancellationToken);

        Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart);
        Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(USheetDatas input, bool restart);
        Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(string filePath, bool restart);
        Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(IBrowserFile file, bool restart);



        Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option);

    }
}