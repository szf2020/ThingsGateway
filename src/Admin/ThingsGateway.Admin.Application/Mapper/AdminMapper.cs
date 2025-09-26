//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Riok.Mapperly.Abstractions;

namespace ThingsGateway.Admin.Application;

[Mapper(UseDeepCloning = true, EnumMappingStrategy = EnumMappingStrategy.ByName, RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class AdminMapper
{
    public static partial LoginInput AdaptLoginInput(this OpenApiLoginInput src);
    public static partial OpenApiLoginOutput AdaptOpenApiLoginOutput(this LoginOutput src);
    public static partial SessionOutput AdaptSessionOutput(this SysUser src);
    public static partial SysUser AdaptSysUser(this SysUser src);
    public static partial UserSelectorOutput AdaptUserSelectorOutput(this SysUser src);
    public static partial List<SysResource> AdaptListSysResource(this IEnumerable<SysResource> src);
    public static partial AppConfig AdaptAppConfig(this AppConfig src);
    public static partial WorkbenchInfo AdaptWorkbenchInfo(this WorkbenchInfo src);
    public static partial QueryData<UserSelectorOutput> AdaptQueryDataUserSelectorOutput(this QueryData<SysUser> src);
    public static partial LoginInput AdaptLoginInput(this LoginInput src);
}
