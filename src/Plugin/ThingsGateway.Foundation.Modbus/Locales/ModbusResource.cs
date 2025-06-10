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


public static class AppResource
{
    public static string CrcError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.CrcError : EnglishResource.CrcError;
    public static string FunctionError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.FunctionError : EnglishResource.FunctionError;
    public static string FunctionNotSame => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.FunctionNotSame : EnglishResource.FunctionNotSame;
    public static string ModbusError1 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError1 : EnglishResource.ModbusError1;
    public static string ModbusError10 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError10 : EnglishResource.ModbusError10;
    public static string ModbusError11 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError11 : EnglishResource.ModbusError11;
    public static string ModbusError2 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError2 : EnglishResource.ModbusError2;
    public static string ModbusError3 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError3 : EnglishResource.ModbusError3;
    public static string ModbusError4 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError4 : EnglishResource.ModbusError4;
    public static string ModbusError5 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError5 : EnglishResource.ModbusError5;
    public static string ModbusError6 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError6 : EnglishResource.ModbusError6;
    public static string ModbusError8 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ModbusError8 : EnglishResource.ModbusError8;
    public static string StationNotSame => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.StationNotSame : EnglishResource.StationNotSame;
    public static string AddressDes => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.AddressDes : EnglishResource.AddressDes;
    public static string ValueOverlimit => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ValueOverlimit : EnglishResource.ValueOverlimit;

}


public static class ChineseResource
{

    public const string CrcError = "Crc校验失败";
    public const string FunctionError = "功能码错误";
    public const string FunctionNotSame = "功能码不一致，请求功能码 {0}，返回功能码 {1}";
    public const string ModbusError1 = "不支持的功能码";
    public const string ModbusError10 = "网关路径不可用";
    public const string ModbusError11 = "网关目标设备响应失败";
    public const string ModbusError2 = "读取寄存器越界";
    public const string ModbusError3 = "读取长度超限";
    public const string ModbusError4 = "设备故障";
    public const string ModbusError5 = "设备已确认，但未执行";
    public const string ModbusError6 = "设备忙";
    public const string ModbusError8 = "存储奇偶性错误";
    public const string StationNotSame = "站号不一致，请求站号 {0} ，返回站号 {1}";
    public const string AddressDes = """
        线圈寄存器使用从 00001 开始的地址编号。
        离散输入寄存器使用从 10001 开始的地址编号。
        输入寄存器使用从 30001 开始的地址编号。
        保持寄存器使用从 40001 开始的地址编号。
        举例：40001=>保持寄存器第一个寄存器
        额外格式
        设备站号 ，比如40001;s=2; ，代表设备地址为2的保持寄存器第一个寄存器
        写入功能码 ，比如40001;w=16; ，代表保持寄存器第一个寄存器，写入值时采用0x10功能码
        """;
    public const string ValueOverlimit = "{0} 不能超过 {1}";

}



public static class EnglishResource
{
    public const string CrcError = "CRC check failed";
    public const string FunctionError = "Function code error";
    public const string FunctionNotSame = "Function code mismatch. Requested: {0}, Returned: {1}";
    public const string ModbusError1 = "Unsupported function code";
    public const string ModbusError10 = "Gateway path unavailable";
    public const string ModbusError11 = "Gateway target device failed to respond";
    public const string ModbusError2 = "Read register out of range";
    public const string ModbusError3 = "Read length exceeded";
    public const string ModbusError4 = "Device failure";
    public const string ModbusError5 = "Device acknowledged but did not execute";
    public const string ModbusError6 = "Device is busy";
    public const string ModbusError8 = "Memory parity error";
    public const string StationNotSame = "Station number mismatch. Requested: {0}, Returned: {1}";
    public const string AddressDes = """
        Coil registers start at address 00001.
        Discrete input registers start at address 10001.
        Input registers start at address 30001.
        Holding registers start at address 40001.
        Example: 40001 => first holding register

        Extra format:
        Device station number, e.g., 40001;s=2; means device address 2, first holding register.
        Write function code, e.g., 40001;w=16; means writing to the first holding register using function code 0x10.
        """;
    public const string ValueOverlimit = "{0} cannot exceed {1}";
}
