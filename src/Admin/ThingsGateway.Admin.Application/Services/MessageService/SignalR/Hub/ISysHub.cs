//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 即时通讯集线器
/// </summary>
public interface ISysHub
{
    /// <summary>
    /// 退出登录
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task LoginOut(string context);
    /// <summary>
    /// 发送新消息通知给指定用户
    /// </summary>
    Task NavigationMesage(string uri, string message);

    /// <summary>
    /// 发送新消息通知给指定用户
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <returns>异步操作结果</returns>
    Task NewMesage(AppMessage message);
}