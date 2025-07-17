//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Photino.NET;

using System.Text;
using System.Xml;

namespace ThingsGateway.Server;

internal sealed class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        //当前工作目录设为程序集的基目录
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        // 增加中文编码支持
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var doc = new XmlDocument();
        doc.Load("appsettings.xml");

        string? url = doc.SelectSingleNode("/settings/urls")?.InnerText;
        string? title = doc.SelectSingleNode("/settings/windowTitle")?.InnerText;

        var window = new PhotinoWindow();

        window.Load(url); // 👈 直接加载远程地址

        window.ContextMenuEnabled = false;
        window.DevToolsEnabled = false;
        window.GrantBrowserPermissions = true;
        window.SetUseOsDefaultLocation(false);
        window.SetUseOsDefaultSize(false);
        window.SetSize(new System.Drawing.Size(1920, 1080));
        window.SetTitle(title);
        window.SetIconFile("favicon.ico");

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
        };

        window.WindowClosing += (sender, e) =>
        {

            return false;
        };
        window.WaitForClose();
        Thread.Sleep(2000);
    }


}
