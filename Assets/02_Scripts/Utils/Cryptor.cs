using System;
using System.Security.Cryptography;
using System.Text;
using CoffeeCat.Utils.Defines;

public static class Cryptor
{
    public static string Decrypt(string textToDecrypt)
    {
#if UNITY_EDITOR
        return textToDecrypt;
#else
        return Decrypt2(textToDecrypt);
#endif
    }

    public static string Decrypt2(string textToDecrypt)
    {
        if (textToDecrypt == string.Empty) return null;

        RijndaelManaged rijndaelCipher = new RijndaelManaged
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,

            KeySize = 128,
            BlockSize = 128
        };

        byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
        byte[] pwdBytes = Encoding.UTF8.GetBytes(Defines.ENC_KEY);
        byte[] keyBytes = new byte[16];
        int len = pwdBytes.Length;

        if (len > keyBytes.Length)
        {
            len = keyBytes.Length;
        }

        Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        rijndaelCipher.IV = keyBytes;

        byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        return Encoding.UTF8.GetString(plainText);
    }

    public static string Encrypt(string textToEncrypt)
    {
#if UNITY_EDITOR
        return textToEncrypt;
#else
        return Encrypt2(textToEncrypt);
#endif
    }

    public static string Encrypt2(string textToEncrypt)
    {
        RijndaelManaged rijndaelCipher = new RijndaelManaged
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            KeySize = 128,
            BlockSize = 128
        };

        byte[] pwdBytes = Encoding.UTF8.GetBytes(Defines.ENC_KEY);
        byte[] keyBytes = new byte[16];
        int len = pwdBytes.Length;

        if (len > keyBytes.Length)
        {
            len = keyBytes.Length;
        }

        Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        rijndaelCipher.IV = keyBytes;

        ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

        byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);

        return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
    }
}
