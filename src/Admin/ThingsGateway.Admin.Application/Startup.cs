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
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Text;

using ThingsGateway.Extension;
using ThingsGateway.UnifyResult;

namespace ThingsGateway.Admin.Application;

[AppStartup(1000000000)]
public class Startup : AppStartup
{
    public void Configure(IServiceCollection services)
    {
        Directory.CreateDirectory("DB");

        services.AddConfigurableOptions<AdminLogOptions>();
        services.AddConfigurableOptions<TenantOptions>();

        services.AddSingleton<IUserAgentService, UserAgentService>();
        services.AddSingleton<IAppService, AppService>();

        services.AddConfigurableOptions<EmailOptions>();
        services.AddConfigurableOptions<HardwareInfoOptions>();

        services.AddSingleton<INoticeService, NoticeService>();
        services.AddSingleton<IUnifyResultProvider, UnifyResultProvider>();
        services.AddSingleton<IAuthService, AuthService>();

        services.AddSingleton<IVerificatInfoService, VerificatInfoService>();

        services.AddSingleton<IApiPermissionService, ApiPermissionService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IImportExportService, ImportExportService>();

        services.AddSingleton<IVerificatInfoService, VerificatInfoService>();
        services.AddSingleton<IUserCenterService, UserCenterService>();
        services.AddSingleton<ISysDictService, SysDictService>();
        services.AddSingleton<ISysOperateLogService, SysOperateLogService>();
        services.AddSingleton<IRelationService, RelationService>();
        services.AddSingleton<ISysResourceService, SysResourceService>();
        services.AddSingleton<ISysRoleService, SysRoleService>();
        services.AddSingleton<ISysUserService, SysUserService>();
        services.AddSingleton<ISessionService, SessionService>();

        services.AddSingleton<ISysPositionService, SysPositionService>();
        services.AddSingleton<ISysOrgService, SysOrgService>();

        services.AddSingleton<LogJob>();
        services.AddSingleton<HardwareJob>();
        services.AddSingleton<IHardwareJob, HardwareJob>(serviceProvider => serviceProvider.GetService<HardwareJob>());

        services.AddSingleton(typeof(IEventService<>), typeof(EventService<>));


        #region 控制台美化

        services.AddConsoleFormatter(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                if (App.HostApplicationLifetime.ApplicationStopping.IsCancellationRequested && logMsg.LogLevel >= LogLevel.Warning) return false;
                if (string.IsNullOrEmpty(logMsg.Message)) return false;
                else return true;
            };

            options.MessageFormat = (logMsg) =>
            {
                //如果不是LoggingMonitor日志才格式化
                if (logMsg.LogName != "System.Logging.LoggingMonitor")
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("【日志级别】：" + logMsg.LogLevel);
                    stringBuilder.AppendLine("【日志类名】：" + logMsg.LogName);
                    stringBuilder.AppendLine("【日志时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
                    stringBuilder.AppendLine("【日志内容】：" + logMsg.Message);
                    if (logMsg.Exception != null)
                    {
                        stringBuilder.AppendLine("【异常信息】：" + logMsg.Exception);
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return logMsg.Message;
                }
            };
            options.WriteHandler = (logMsg, scopeProvider, writer, fmtMsg, opt) =>
            {
                ConsoleColor consoleColor = ConsoleColor.White;
                switch (logMsg.LogLevel)
                {
                    case LogLevel.Information:
                        consoleColor = ConsoleColor.DarkGreen;
                        break;

                    case LogLevel.Warning:
                        consoleColor = ConsoleColor.DarkYellow;
                        break;

                    case LogLevel.Error:
                        consoleColor = ConsoleColor.DarkRed;
                        break;
                }
                writer.WriteWithColor(fmtMsg, ConsoleColor.Black, consoleColor);
            };
        });

        #endregion 控制台美化
        //日志写入数据库配置
        services.AddDatabaseLogging<DatabaseLoggingWriter>(options =>
        {
            options.NameFilter = (name) =>
            {
                return (
                name == "System.Logging.RequestAudit"
                );
            };
        });
    }

    public void Use(IServiceProvider serviceProvider)
    {
        NewLife.Log.XTrace.UnhandledExceptionLogEnable = () => !App.HostApplicationLifetime.ApplicationStopping.IsCancellationRequested;

        //检查ConfigId
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId);
        foreach (var configId in configIdGroup)
        {
            if (configId.Count() > 1) throw new($"Sqlsugar connect configId: {configId.Key} Duplicate!");
        }

        //遍历配置
        DbContext.DbConfigs?.ForEach(it =>
        {
            var connection = DbContext.GetDB().GetConnection(it.ConfigId);//获取数据库连接对象
            if (it.InitDatabase == true)
                connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });

        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst


        try
        {
            using var db = DbContext.GetDB<SysOperateLog>();
            if (db.CurrentConnectionConfig.DbType == SqlSugar.DbType.Sqlite)
            {
                if (!db.DbMaintenance.IsAnyIndex("idx_operatelog_optime_date"))
                {
                    var indexsql = "CREATE INDEX idx_operatelog_optime_date ON sys_operatelog(strftime('%Y-%m-%d', OpTime));";
                    db.Ado.ExecuteCommand(indexsql);
                }
            }
        }
        catch { }


        //删除在线用户统计
        var verificatInfoService = App.RootServices.GetService<IVerificatInfoService>();
        verificatInfoService.RemoveAllClientId();


    }
}
