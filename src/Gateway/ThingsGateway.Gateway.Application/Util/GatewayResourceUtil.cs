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

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class GatewayResourceUtil
{
    /// <summary>
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildChannelSelectList(this IEnumerable<Channel> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Name)
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildDeviceSelectList(this IEnumerable<Device> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Name)
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildPluginSelectList(this IEnumerable<PluginInfo> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.FullName, item.Name)
            {
                GroupName = item.FileName,
            }
        ).ToList();
        return data;
    }
#if !Management
    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<PluginInfo>> BuildTreeItemList(this IEnumerable<PluginInfo> pluginInfos, Microsoft.AspNetCore.Components.RenderFragment<PluginInfo> render = null, TreeViewItem<PluginInfo>? parent = null)
    {
        if (pluginInfos == null) return null;
        var trees = new List<TreeViewItem<PluginInfo>>();
        foreach (var node in pluginInfos)
        {
            var item = new TreeViewItem<PluginInfo>(node)
            {
                Text = node.Name,
                Parent = parent,
                IsExpand = true,
                Template = render,
            };
            item.Items = BuildTreeItemList(node.Children, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }
#endif
}
