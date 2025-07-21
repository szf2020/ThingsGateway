//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.SiemensS7;

public static class AppResource
{
    public static string S7_AddressDes => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.AddressDes : EnglishResource.AddressDes;
    public static string DataLengthError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DataLengthError : EnglishResource.DataLengthError;
    public static string ERROR1 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR1 : EnglishResource.ERROR1;
    public static string ERROR10 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR10 : EnglishResource.ERROR10;
    public static string ERROR3 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR3 : EnglishResource.ERROR3;
    public static string ERROR5 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR5 : EnglishResource.ERROR5;
    public static string ERROR6 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR6 : EnglishResource.ERROR6;
    public static string ERROR7 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ERROR7 : EnglishResource.ERROR7;
    public static string HandshakeError1 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.HandshakeError1 : EnglishResource.HandshakeError1;
    public static string HandshakeError2 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.HandshakeError2 : EnglishResource.HandshakeError2;
    public static string NotString => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.NotString : EnglishResource.NotString;
    public static string WriteDataLengthMore => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.WriteDataLengthMore : EnglishResource.WriteDataLengthMore;
    public static string StringLengthReadError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.StringLengthReadError : EnglishResource.StringLengthReadError;
    public static string ReturnError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ReturnError : EnglishResource.ReturnError;
    public static string ValidateDataError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ValidateDataError : EnglishResource.ValidateDataError;
    public static string AddressError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.AddressError : EnglishResource.AddressError;
}
public static class ChineseResource
{
    public const string AddressDes = """
        S7协议寄存器地址格式
        Txxxxx Timer寄存器，例如T100/T100.1
        Cxxxxx，Counter寄存器，例如C100/C100.1
        AIxxxxx，AI寄存器，例如AI100/AI100.1
        AQxxxxx，AQ寄存器，例如AQ100/AQ100.1
        Ixxxxx，I寄存器，例如I100/I100.1
        Qxxxxx，Q寄存器，例如Q100/Q100.1
        Mxxxxx，M寄存器，例如M100/M100.1
        DBxxxxx，DB寄存器，例如DB100.1/DB100.1.1
        """;

    public const string DataLengthError = "数据块长度校验失败";
    public const string ERROR1 = "硬件错误";
    public const string ERROR10 = "对象不存在";
    public const string ERROR3 = "对象不允许访问";
    public const string ERROR5 = "无效地址，所需的地址超出此PLC的极限";
    public const string ERROR6 = "数据类型不支持";
    public const string ERROR7 = "日期类型不一致";
    public const string HandshakeError1 = "{0}：ISO_TP握手失败-{1}，请检查机架号/槽号是否正确";
    public const string HandshakeError2 = "{0}：PDU初始化失败-{1}，请检查机架号/槽号是否正确";
    public const string NotString = "PLC中的数据类型不是String";
    public const string WriteDataLengthMore = "写入长度超限";
    public const string StringLengthReadError = "不支持变长字符串的连读";
    public const string ReturnError = "PLC返回错误码，错误类型 {0} 错误代码 {1}";
    public const string ValidateDataError = "验证错误，代码 {0}，类型 {1}";
    public const string AddressError = "寄存器地址格式错误 {0}";
}
public static class EnglishResource
{
    public const string AddressDes = """
        S7 protocol register address format:
        Txxxxx - Timer register, e.g., T100/T100.1
        Cxxxxx - Counter register, e.g., C100/C100.1
        AIxxxxx - AI register, e.g., AI100/AI100.1
        AQxxxxx - AQ register, e.g., AQ100/AQ100.1
        Ixxxxx - I register, e.g., I100/I100.1
        Qxxxxx - Q register, e.g., Q100/Q100.1
        Mxxxxx - M register, e.g., M100/M100.1
        DBxxxxx - DB register, e.g., DB100.1/DB100.1.1
        """;

    public const string DataLengthError = "Data block length validation failed";
    public const string ERROR1 = "Hardware error";
    public const string ERROR10 = "Object does not exist";
    public const string ERROR3 = "Access to the object is not permitted";
    public const string ERROR5 = "Invalid address; requested address exceeds PLC limits";
    public const string ERROR6 = "Unsupported data type";
    public const string ERROR7 = "Data type mismatch";
    public const string HandshakeError1 = "{0}: ISO_TP handshake failed - {1}, please check rack/slot settings";
    public const string HandshakeError2 = "{0}: PDU initialization failed - {1}, please check rack/slot settings";
    public const string NotString = "The data type in PLC is not String";
    public const string WriteDataLengthMore = "Write length exceeds limit";
    public const string StringLengthReadError = "Reading variable-length strings continuously is not supported";
    public const string ReturnError = "PLC returned error code. Type: {0}, Code: {1}";
    public const string ValidateDataError = "Validation failed. Code: {0}, Type: {1}";
    public const string AddressError = "Register address format error: {0}";
}
