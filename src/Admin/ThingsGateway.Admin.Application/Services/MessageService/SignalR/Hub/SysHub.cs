//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using ThingsGateway.InstantMessaging;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="ISysHub"/>
/// </summary>
[NonUnify]
[MapHub(HubConst.SysHubUrl)]
public class SysHub : Hub<ISysHub>
{
    /// <summary>
    /// 分隔符
    /// </summary>
    public const string Separate = "_s_e_p_a_r_a_t_e_";
    private IVerificatInfoService _verificatInfoService { get; set; }
    /// <inheritdoc cref="ISysHub"/>
    public SysHub(IVerificatInfoService verificatInfoService)
    {
        _verificatInfoService = verificatInfoService;
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    public override Task OnConnectedAsync()
    {
        var feature = Context.Features.Get<IHttpContextFeature>();
        var VerificatId = feature.HttpContext.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();
        if (VerificatId > 0)
        {
            var userIdentifier = Context.UserIdentifier;//自定义的Id
            VerificatInfoUtil.UpdateVerificat(userIdentifier, VerificatId);//更新cache
        }
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var feature = Context.Features.Get<IHttpContextFeature>();
        var userIdentifier = Context.UserIdentifier;//自定义的Id
        var VerificatId = feature.HttpContext.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();
        VerificatInfoUtil.UpdateVerificat(userIdentifier, VerificatId, false);//更新cache
        return base.OnDisconnectedAsync(exception);
    }
}