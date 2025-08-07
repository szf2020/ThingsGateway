//------------------------------------------------------------------------------
//此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

[Route("api/[controller]/[action]")]
[AllowAnonymous]
[ApiController]
[TouchSocket.WebApi.Router("/miniapi/[api]/[action]")]
[TouchSocket.WebApi.EnableCors("cors")]
public class TestController : ControllerBase, IRpcServer
{
    [HttpGet]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Get)]
    public void Test()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
