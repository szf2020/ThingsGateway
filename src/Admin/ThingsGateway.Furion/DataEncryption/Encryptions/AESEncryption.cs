// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;

namespace ThingsGateway.DataEncryption;

/// <summary>
/// AES 加解密
/// </summary>
[SuppressSniffer]
public static class AESEncryption
{
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="text">加密文本</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <param name="isBase64"></param>
    /// <returns></returns>
    public static string Encrypt(string text, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, bool isBase64 = false)
    {
        var bKey = !isBase64 ? Encoding.UTF8.GetBytes(skey) : Convert.FromBase64String(skey);
        if (bKey.Length != 16 && bKey.Length != 24 && bKey.Length != 32) throw new ArgumentException("The key length must be 16, 24, or 32 bytes.");

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        if (mode != CipherMode.ECB)
        {
            aesAlg.IV = iv ?? aesAlg.IV;
            if (iv != null && iv.Length != 16) throw new ArgumentException("The IV length must be 16 bytes.");
        }

        byte[] cipherBytes;
        using (var encryptor = aesAlg.CreateEncryptor())
        {
            var plainBytes = Encoding.UTF8.GetBytes(text);
            cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        // 仅在未提供 IV 时拼接 IV
        if (mode != CipherMode.ECB && iv == null)
        {
            var result = new byte[aesAlg.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aesAlg.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        // 如果是 ECB 模式，直接返回密文的 Base64 编码
        return Convert.ToBase64String(cipherBytes);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="hash">加密后字符串</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <param name="isBase64"></param>
    /// <returns></returns>
    public static string Decrypt(string hash, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, bool isBase64 = false)
    {
        var fullCipher = Convert.FromBase64String(hash);
        var bKey = !isBase64 ? Encoding.UTF8.GetBytes(skey) : Convert.FromBase64String(skey);
        if (bKey.Length != 16 && bKey.Length != 24 && bKey.Length != 32) throw new ArgumentException("The key length must be 16, 24, or 32 bytes.");

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        if (mode != CipherMode.ECB)
        {
            if (iv == null)
            {
                if (fullCipher.Length < aesAlg.BlockSize / 8) throw new ArgumentException("The ciphertext length is insufficient to extract the IV.");

                iv = new byte[aesAlg.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
                aesAlg.IV = iv;
                fullCipher = cipher;
            }
            else
            {
                if (iv.Length != 16) throw new ArgumentException("The IV length must be 16 bytes.");
                aesAlg.IV = iv;
            }
        }

        using var decryptor = aesAlg.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(fullCipher, 0, fullCipher.Length);

        // 手动移除 PKCS7 填充
        int padCount = plainBytes[^1];
        if (padCount > 0 && padCount <= 16)
        {
            var validPadding = true;
            for (var i = 1; i <= padCount; i++)
            {
                if (plainBytes[^i] != padCount)
                {
                    validPadding = false;
                    break;
                }
            }
            if (validPadding)
                Array.Resize(ref plainBytes, plainBytes.Length - padCount);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="bytes">源文件 字节数组</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <param name="isBase64"></param>
    /// <returns>加密后的字节数组</returns>
    public static byte[] Encrypt(byte[] bytes, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, bool isBase64 = false)
    {
        var bKey = !isBase64 ? Encoding.UTF8.GetBytes(skey) : Convert.FromBase64String(skey);
        if (bKey.Length != 16 && bKey.Length != 24 && bKey.Length != 32) throw new ArgumentException("The key length must be 16, 24, or 32 bytes.");

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        if (mode != CipherMode.ECB)
        {
            aesAlg.IV = iv ?? (mode == CipherMode.CBC ? GenerateRandomIV() : throw new ArgumentException("IV is required for CBC mode."));
            if (aesAlg.IV.Length != 16) throw new ArgumentException("The IV length must be 16 bytes.");
        }

        byte[] cipherBytes;
        using (var encryptor = aesAlg.CreateEncryptor())
        {
            cipherBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        if (mode == CipherMode.ECB)
            return cipherBytes;

        var result = new byte[aesAlg.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aesAlg.IV.Length, cipherBytes.Length);
        return result;
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes">加密后文件 字节数组</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <param name="isBase64"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] bytes, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, bool isBase64 = false)
    {
        var bKey = !isBase64 ? Encoding.UTF8.GetBytes(skey) : Convert.FromBase64String(skey);
        if (bKey.Length != 16 && bKey.Length != 24 && bKey.Length != 32) throw new ArgumentException("The key length must be 16, 24, or 32 bytes.");

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        byte[] cipherBytes;
        if (mode != CipherMode.ECB)
        {
            if (iv == null)
            {
                if (bytes.Length < 16) throw new ArgumentException("The ciphertext length is insufficient to extract the IV.");
                iv = [.. bytes.Take(16)];
                cipherBytes = [.. bytes.Skip(16)];
            }
            else
            {
                if (iv.Length != 16) throw new ArgumentException("The IV length must be 16 bytes.");
                cipherBytes = bytes;
            }
            aesAlg.IV = iv;
        }
        else
        {
            cipherBytes = bytes;
        }

        using var decryptor = aesAlg.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        // 手动移除 PKCS7 填充
        int padCount = plainBytes[^1];
        if (padCount > 0 && padCount <= 16)
        {
            var validPadding = true;
            for (var i = 1; i <= padCount; i++)
            {
                if (plainBytes[^i] != padCount)
                {
                    validPadding = false;
                    break;
                }
            }
            if (validPadding)
                Array.Resize(ref plainBytes, plainBytes.Length - padCount);
        }

        return plainBytes;
    }

    /// <summary>
    /// 生成随机 IV
    /// </summary>
    /// <returns></returns>
    private static byte[] GenerateRandomIV()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        return aes.IV;
    }
}