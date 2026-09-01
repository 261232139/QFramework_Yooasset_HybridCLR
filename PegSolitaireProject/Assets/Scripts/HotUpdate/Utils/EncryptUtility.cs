#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#endregion


public class EncryptUtility
{
    private static UTF8Encoding UTF8Encoding = new UTF8Encoding(false);
    #region Encrypt Decrypt 加密解密

    /// <summary>
    /// 加密字符串
    /// </summary>
    public static string EncryptStringByAES(string strData, string privateKey)
    {
        byte[] dataAsByteArray = UTF8Encoding.UTF8.GetBytes(strData);
        byte[] dataAfterEncrypt = EncryptDataByAES(dataAsByteArray, privateKey);
        string strResult = UTF8Encoding.UTF8.GetString(dataAfterEncrypt);
        return strResult;
    }

    /// <summary>
    /// 解密字符串
    /// </summary>
    public static string DecryptStringByAES(string strData, string privateKey)
    {
        byte[] dataAsByteArray = UTF8Encoding.UTF8.GetBytes(strData);
        byte[] dataAfterDecrypt = DecryptDataByAES(dataAsByteArray, privateKey);
        string strResult = UTF8Encoding.UTF8.GetString(dataAfterDecrypt);
        return strResult;
    }

    /// <summary>
    /// 加密, 根据私钥, 把data加密
    /// 1. privateKey 32位
    /// </summary>
    public static byte[] EncryptDataByAES(byte[] data, string privateKey)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(privateKey);
        RijndaelManaged decipher = new RijndaelManaged();
        decipher.Key = keyArray;
        //加密和块填充模式,参考http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        decipher.Mode = CipherMode.ECB;
        decipher.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = decipher.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
        return resultArray;
    }

    /// <summary>
    /// 解密, 根据私钥, 把data解密
    /// 1. privateKey 32位
    /// </summary>
    public static byte[] DecryptDataByAES(byte[] data, string privateKey)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(privateKey);
        RijndaelManaged decipher = new RijndaelManaged();
        decipher.Key = keyArray;
        //加密和块填充模式,参考http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        decipher.Mode = CipherMode.ECB;
        decipher.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = decipher.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
        return resultArray;
    }

    #endregion

    #region 另一套加解密
    //加密头
    private static char[] charHeadArray = new char[] {'E', 'N', 'C', 'O', 'D', 'E', '1', '6'};

    private static string EncodeKey = "owkqidlwizyeneos";

    /// <summary>文件解密</summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string CharXorDecrypt(string input, char[] key)
    {
        char[] inputArray = input.ToCharArray();

        bool isXorEncrypt = true;
        if (inputArray.Length < charHeadArray.Length)
        {
            isXorEncrypt = false;
        }
        else
        {
            for (int i = 0; i < charHeadArray.Length; i++)
            {
                if (inputArray[i] != charHeadArray[i])
                {
                    isXorEncrypt = false;
                    break;
                }
            }
        }

        if (isXorEncrypt)
        {
            char[] encryptArray = new char[inputArray.Length - charHeadArray.Length];
            char[] decryptArray = new char[encryptArray.Length];
            Buffer.BlockCopy(inputArray, charHeadArray.Length * sizeof(char), encryptArray, 0,
                encryptArray.Length * sizeof(char));

            for (int i = 0; i < encryptArray.Length; i++)
            {
                decryptArray[i] = (char) (encryptArray[i] ^ key[i & 15]);
            }

            return new string(decryptArray);
        }
        else
        {
            return input;
        }
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="rawStrContent"></param>
    /// <returns></returns>
    public static byte[] Encrypt(string rawStrContent)
    {
        if (string.IsNullOrEmpty(rawStrContent)) //长度不够处理
        {
            return new byte[0];
        }


        byte[] bytes = GetBytes(rawStrContent); //信息改为utf8 bytes

        uint[] intForData = Byte2Uint(bytes, true); //数据 转成uint数组
        int dataLen = intForData.Length - 1; //最后1位存储 数据长度
        if (dataLen < 1) //长度为0 返回空数组
        {
            return new byte[0];
        }

        uint[] intForKey = Byte2Uint(GetBytes(EncodeKey), false); //密钥转为uint数组

        for (int i = 0, iMax = intForData.Length; i < iMax; i++)
        {
            intForData[i] = (intForData[i] + intForKey[i % 4]) % uint.MaxValue;
        }

        return Uint2Byte(intForData, false);
    }

    /// <summary>
    /// 加密, 返回字符串
    /// </summary>
    /// <param name="rawStrContent"></param>
    /// <returns></returns>
    public static string EncryptWithStringResult(string rawStrContent)
    {
        byte[] bytesResult = Encrypt(rawStrContent);
        return Convert.ToBase64String(bytesResult);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string Decrypt(byte[] bytes)
    {
        string info = string.Empty;
        if (bytes == null || bytes.Length < 1) //长度不够处理
        {
            return info;
        }


        uint[] intForData = Byte2Uint(bytes, false);
        uint[] intForKey = Byte2Uint(GetBytes(EncodeKey), false);

        for (int i = 0, iMax = intForData.Length; i < iMax; i++)
        {
            uint middleValue = intForKey[i % 4];
            if (intForData[i] < middleValue)
            {
                intForData[i] += uint.MaxValue - middleValue;
            }
            else
            {
                intForData[i] -= middleValue;
            }
        }

        return GetString(Uint2Byte(intForData, true));
    }

    /// <summary>
    /// 解密, 以字符串为参数
    /// </summary>
    /// <param name="rawStrContent"></param>
    /// <returns></returns>
    public static string DecryptWithStringContent(string encrypStrContent)
    {
        byte[] bytesResult = Convert.FromBase64String(encrypStrContent);
        return Decrypt(bytesResult);
    }


    /// <summary>
    /// 混合运算
    /// </summary>
    /// <param name="sum"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="p"></param>
    /// <param name="e"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    private static uint DoRun(uint sum, uint y, uint z, int p, uint e, uint[] k)
    {
        uint num0 = (z >> 5 ^ y << 2);
        uint num1 = (y >> 3 ^ z << 4) ^ (sum ^ y);
        long num2 = unchecked(p & 3 ^ (long) ((ulong) e));
        int num3 = (int) (checked(num2));
        //Debug.LogError("num3  " + num3.ToString());
        uint num4 = k[num3];

        return num0 + num1 + (num4 ^ z);
    }
    
    #endregion

    #region 新的加密

    private static readonly byte[] newHeadArray = { (byte)'M', (byte)'2', (byte)'Z', 0x00 };
    private static readonly RijndaelManaged aes = new RijndaelManaged
    {
        KeySize = 128,
        BlockSize = 128,
        Mode = CipherMode.CFB,
        Padding = PaddingMode.PKCS7,
        Key = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(EncodeKey))
    };

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="rawStrContent"></param>
    /// <returns></returns>
    public static byte[] NewEncrypt(string rawStrContent)
    {
        if (string.IsNullOrEmpty(rawStrContent)) return Array.Empty<byte>();
        aes.GenerateIV();
        var rawBytes = Encoding.UTF8.GetBytes(rawStrContent);
        byte[] encryptedBytes;
        using (var encryptor = aes.CreateEncryptor())
            encryptedBytes = encryptor.TransformFinalBlock(rawBytes, 0, rawBytes.Length);
        return newHeadArray.Concat(aes.IV).Concat(encryptedBytes).ToArray();
    }

    /// <summary>
    /// 解密
    /// </summary>
    public static string NewDecrypt(byte[] bytes)
    {
        if (!CheckIsNewEncrypt(bytes)) return string.Empty;
        aes.IV = bytes.Skip(4).Take(16).ToArray();
        byte[] decryptedBytes;
        using (var decryptor = aes.CreateDecryptor())
            decryptedBytes = decryptor.TransformFinalBlock(bytes, 20, bytes.Length - 20);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// 检查是否是新加密
    /// </summary>
    public static bool CheckIsNewEncrypt(byte[] bytes)
    {
        return bytes != null &&
            bytes.Length > 20 &&
            bytes[0] == newHeadArray[0] &&
            bytes[1] == newHeadArray[1] &&
            bytes[2] == newHeadArray[2] &&
            bytes[3] == newHeadArray[3];
    }

    #endregion

    #region 基础功能

    /// <summary>
    /// 字符转byte
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static byte[] GetBytes(string info)
    {
        if (!string.IsNullOrEmpty(info))
        {
            return UTF8Encoding.GetBytes(info);
        }

        return null;
    }

    /// <summary>
    /// byte转string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string GetString(byte[] bytes)
    {
        if (bytes != null)
        {
            return UTF8Encoding.GetString(bytes);
        }

        return string.Empty;
    }

    /// <summary>
    /// 将byte数组转换为 无符号整形数组
    /// </summary>
    /// <param name="data"></param>
    /// <param name="includeLength"></param>
    /// <returns></returns>
    public static uint[] Byte2Uint(byte[] data, bool includeLength)
    {
        if (data == null)
        {
            return null;
        }

        int dataLen = data.Length; //原始数据长度
        int saveLen = ((dataLen & 3) != 0) ? ((dataLen >> 2) + 1) : (dataLen >> 2); //四分之一长度(能整除就4分之1，不能就扩一位，存储余数)
        uint[] array;
        if (includeLength) //如果需要写入长度，就再次扩容，然后将原始数据长度写在最后一位
        {
            array = new uint[saveLen + 1];
            array[saveLen] = (uint) dataLen;
        }
        else
        {
            array = new uint[saveLen];
        }

        for (int i = 0; i < dataLen; i++) //填充数据
        {
            array[i >> 2] |= (uint) data[i] << ((i & 3) << 3);
        }

        return array;
    }

    /// <summary>
    /// 无符号整形 转 byte数组
    /// </summary>
    /// <param name="data"></param>
    /// <param name="includeLength"></param>
    /// <returns></returns>
    public static byte[] Uint2Byte(uint[] data, bool includeLength)
    {
        if (data == null)
        {
            return null;
        }

        int dataLen = data.Length << 2; //数据长度为存储长度的4的整数倍
        if (includeLength) //如果存储了长度，最后一位是长度
        {
            int lenInfo = (int) data[data.Length - 1]; //存储的原始数据长度
            dataLen -= 4;
            if (lenInfo < dataLen - 3 || lenInfo > dataLen) //过小就说明存储有问题
            {
                return null;
            }

            dataLen = lenInfo; //将存档中记录的 数据长度 覆盖计算得到的 数据长度
        }

        byte[] array = new byte[dataLen];
        for (int i = 0; i < dataLen; i++) //填充数据
        {
            array[i] = (byte) (data[i >> 2] >> ((i & 3) << 3));
        }

        return array;
    }

    #endregion
}
