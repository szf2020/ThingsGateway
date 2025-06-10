//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

public static class ModbusHelper
{
    #region 解析

    /// <summary>
    /// modbus地址格式说明
    /// </summary>
    /// <returns></returns>
    public static string GetAddressDescription()
    {
        return AppResource.AddressDes;
    }

    /// <summary>
    /// 通过错误码来获取到对应的文本消息
    /// </summary>
    public static string GetDescriptionByErrorCode(byte code)
    {
        return code switch
        {
            1 => AppResource.ModbusError1,
            2 => AppResource.ModbusError2,
            3 => AppResource.ModbusError3,
            4 => AppResource.ModbusError4,
            5 => AppResource.ModbusError5,
            6 => AppResource.ModbusError6,
            8 => AppResource.ModbusError8,
            10 => AppResource.ModbusError10,
            11 => AppResource.ModbusError11,
            _ => string.Format(ThingsGateway.Foundation.AppResource.UnknownError, code),
        };
    }

    #endregion 解析
}
