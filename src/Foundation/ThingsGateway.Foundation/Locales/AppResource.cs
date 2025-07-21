//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;

namespace ThingsGateway.Foundation;

public enum Language { Chinese, English }

public static class AppResource
{
    private static IStringLocalizer localizer;

    /// <summary>
    /// Localizer
    /// </summary>
    public static IStringLocalizer Localizer
    {
        get
        {
            if (localizer == null)
            {
                localizer = LocalizerUtil.GetLocalizer.Invoke(typeof(AppResource));
            }
            return localizer;
        }
    }

    public static Language Lang { get; set; } = Language.Chinese;

    public static string DefaultAddressDes => Lang == Language.Chinese ? ChineseResource.DefaultAddressDes : EnglishResource.DefaultAddressDes;
    public static string StringAddressError => Lang == Language.Chinese ? ChineseResource.StringAddressError : EnglishResource.StringAddressError;
    public static string ErrorMessage => Lang == Language.Chinese ? ChineseResource.ErrorMessage : EnglishResource.ErrorMessage;
    public static string Exception => Lang == Language.Chinese ? ChineseResource.Exception : EnglishResource.Exception;
    public static string ServiceStarted => Lang == Language.Chinese ? ChineseResource.ServiceStarted : EnglishResource.ServiceStarted;
    public static string ServiceStoped => Lang == Language.Chinese ? ChineseResource.ServiceStoped : EnglishResource.ServiceStoped;
    public static string StringTypePackError => Lang == Language.Chinese ? ChineseResource.StringTypePackError : EnglishResource.StringTypePackError;
    public static string UnknownError => Lang == Language.Chinese ? ChineseResource.UnknownError : EnglishResource.UnknownError;
    public static string DtuConnected => Lang == Language.Chinese ? ChineseResource.DtuConnected : EnglishResource.DtuConnected;
    public static string DtuDisconnecting => Lang == Language.Chinese ? ChineseResource.DtuDisconnecting : EnglishResource.DtuDisconnecting;
    public static string DtuNoConnectedWaining => Lang == Language.Chinese ? ChineseResource.DtuNoConnectedWaining : EnglishResource.DtuNoConnectedWaining;
    public static string DataTypeNotSupported => Lang == Language.Chinese ? ChineseResource.DataTypeNotSupported : EnglishResource.DataTypeNotSupported;
    public static string TransBytesError => Lang == Language.Chinese ? ChineseResource.TransBytesError : EnglishResource.TransBytesError;
}

public static class ChineseResource
{
    public const string DefaultAddressDes = """
        ————————————————————
            4字节数据转换格式：data=ABCD;可选ABCD=>Big-Endian;BADC=>Big-Endian Byte Swap;CDAB=>Little-Endian Byte Swap;DCBA=>Little-Endian。
            字符串长度：len=1。
            数组长度：arraylen=1。
            Bcd格式：bcd=C8421，可选C8421;C5421;C2421;C3;Gray。
            字符格式：encoding=UTF-8，可选UTF-8;ASCII;Default;Unicode等。
        ————————————————————
        """;

    public const string StringAddressError = "字符串读写必须在寄存器地址中指定长度，例如 len=10;";
    public const string ErrorMessage = "错误信息";
    public const string Exception = "异常堆栈";
    public const string ServiceStarted = "启动";
    public const string ServiceStoped = "停止";

    public const string StringTypePackError = "数据类型为字符串时，必须指定字符串长度，才能进行打包，例如 len=10;";
    public const string UnknownError = "未知错误，错误代码：{0}";
    public const string DtuConnected = "Dtu标识 {0} 连接成功";
    public const string DtuDisconnecting = "Dtu标识 {0} 正在断开连接";
    public const string DtuNoConnectedWaining = "客户端(Dtu)未连接，id：{0}";
    public const string DataTypeNotSupported = "{0} 数据类型未实现";
    public const string TransBytesError = "转换失败-原始字节数组  {0}，长度 {1}";
}

public static class EnglishResource
{
    public const string DefaultAddressDes = """
        ————————————————————
        4-byte data conversion format: data=ABCD; options: ABCD=>Big-Endian; BADC=>Big-Endian Byte Swap; CDAB=>Little-Endian Byte Swap; DCBA=>Little-Endian.
        String length: len=1.
        Array length: arraylen=1.
        BCD format: bcd=C8421, options: C8421; C5421; C2421; C3; Gray.
        Character encoding: encoding=UTF-8, options: UTF-8; ASCII; Default; Unicode, etc.
        ————————————————————
        """;

    public const string StringAddressError = "String read/write operations must specify length in the register address, e.g., len=10;";
    public const string ErrorMessage = "Error message";
    public const string Exception = "Exception stack trace";
    public const string ServiceStarted = "Started";
    public const string ServiceStoped = "Stopped";

    public const string StringTypePackError = "When data type is string, the string length must be specified for packing, e.g., len=10;";
    public const string UnknownError = "Unknown error, error code: {0}";
    public const string DtuConnected = "DTU identifier {0} connected successfully";
    public const string DtuDisconnecting = "DTU identifier {0} is disconnecting";
    public const string DtuNoConnectedWaining = "Client (DTU) not connected, id: {0}";
    public const string DataTypeNotSupported = "{0} data type not implemented";
    public const string TransBytesError = "Conversion failed - original byte array {0}, length {1}";
}