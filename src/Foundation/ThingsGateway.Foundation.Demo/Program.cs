//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Demo;
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

public class Program
{

    public static async Task Main(string[] args)
    {
        ModbusMasterDemo modbusMasterDemo = new();
        await modbusMasterDemo.TestReadWrite();
        await modbusMasterDemo.TestMulRead();
        await modbusMasterDemo.TestVariableObject();

        Console.ReadKey();

        SiemensS7MasterDemo siemensS7MasterDemo = new();
        await siemensS7MasterDemo.TestReadWrite();
        await siemensS7MasterDemo.TestMulRead();
        await siemensS7MasterDemo.TestVariableObject();

        Console.ReadKey();
    }

}
