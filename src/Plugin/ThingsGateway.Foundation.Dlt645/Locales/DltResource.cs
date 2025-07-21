//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Dlt645;

public static class AppResource
{
    public static string Error1 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error1 : EnglishResource.Error1;
    public static string Error2 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error2 : EnglishResource.Error2;
    public static string Error3 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error3 : EnglishResource.Error3;
    public static string Error4 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error4 : EnglishResource.Error4;
    public static string Error5 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error5 : EnglishResource.Error5;
    public static string Error6 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error6 : EnglishResource.Error6;
    public static string Error7 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error7 : EnglishResource.Error7;
    public static string Error8 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.Error8 : EnglishResource.Error8;
    public static string AddressDes => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.AddressDes : EnglishResource.AddressDes;
    public static string SumError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.SumError : EnglishResource.SumError;
    public static string StationNotSame => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.StationNotSame : EnglishResource.StationNotSame;
    public static string FunctionError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.FunctionError : EnglishResource.FunctionError;
    public static string FunctionNotSame => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.FunctionNotSame : EnglishResource.FunctionNotSame;
    public static string DataIdNotSame => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DataIdNotSame : EnglishResource.DataIdNotSame;
    public static string CountError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.CountError : EnglishResource.CountError;
    public static string DataIdError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DataIdError : EnglishResource.DataIdError;
    public static string BaudRateError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.BaudRateError : EnglishResource.BaudRateError;
}

public static class ChineseResource
{
    public const string Error1 = "其他错误";
    public const string Error2 = "费率数超";
    public const string Error3 = "日时段数超";
    public const string Error4 = "年时区数超";
    public const string Error5 = "通信速率不能更改";
    public const string Error6 = "密码错/未授权";
    public const string Error7 = "无请求数据";
    public const string Error8 = "其他错误";
    public const string AddressDes = """
        数据标识地址

        查看附带文档或者相关资料，下面列举一些常见的数据标识地址

        地址                       说明
        -----------------------------------------
        02010100    A相电压
        02020100    A相电流
        02030000    瞬时总有功功率
        00000000    (当前)组合有功总电能
        00010000    (当前)正向有功总电能
        """;

    public const string SumError = "和校验错误";
    public const string StationNotSame = "站号不一致";
    public const string FunctionError = "异常控制码：{0}，错误信息：{1}";
    public const string FunctionNotSame = "功能码不一致，请求功能码 {0}，返回功能码 {1}";
    public const string DataIdNotSame = "返回数据标识不符合规则";
    public const string CountError = "写入参数数量不符合地址要求";
    public const string DataIdError = "变量寄存器地址错误";
    public const string BaudRateError = "不支持此波特率 {0}";

}

public static class EnglishResource
{
    public const string Error1 = "Other error";
    public const string Error2 = "Exceeded number of rate segments";
    public const string Error3 = "Exceeded number of daily time segments";
    public const string Error4 = "Exceeded number of yearly time zones";
    public const string Error5 = "Communication baud rate cannot be changed";
    public const string Error6 = "Incorrect password / unauthorized";
    public const string Error7 = "No requested data";
    public const string Error8 = "Other error";

    public const string AddressDes = """
        Data Identifier Address

        Refer to the attached documentation or related materials. Below are some common data identifier addresses:

        Address         Description
        -----------------------------------------
        02010100        Phase A voltage
        02020100        Phase A current
        02030000        Instantaneous total active power
        00000000        (Current) Combined total active energy
        00010000        (Current) Forward total active energy
        """;

    public const string SumError = "Checksum error";
    public const string StationNotSame = "Station numbers do not match";
    public const string FunctionError = "Exception function code: {0}, Error message: {1}";
    public const string FunctionNotSame = "Function code mismatch. Requested: {0}, Returned: {1}";
    public const string DataIdNotSame = "Returned data identifier does not match the expected format";
    public const string CountError = "Incorrect number of parameters for the address";
    public const string DataIdError = "Invalid variable register address";
    public const string BaudRateError = "Unsupported baud rate {0}";
}
