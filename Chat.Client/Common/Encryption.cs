using System.Security.Cryptography;
using System.Text;

namespace Chat.Client.Common;

public class Encryption
{
    public static RSAParameters _publicKey;
    private static RSAParameters _privateKey;

    public static void GenerateKeys()
    {
        using RSA rsa = RSA.Create();
        _publicKey = rsa.ExportParameters(false);
        _privateKey = rsa.ExportParameters(true);
    }

    public static byte[] Encrypt(string plainText)
    {
        byte[] encryptedBytes;
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(_publicKey);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
        }

        return encryptedBytes;
    }

    public static byte[] Encrypt(string plainText, RSAParameters publicKey)
    {
        byte[] encryptedBytes;
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
        }

        return encryptedBytes;
    }

    public static string Decrypt(byte[] encryptedBytes)
    {
        string decryptedText;
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(_privateKey);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            decryptedText = Encoding.UTF8.GetString(decryptedBytes);
        }

        return decryptedText;
    }

    public static string Decrypt(byte[] encryptedBytes, RSAParameters privateKey)
    {
        string decryptedText;
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(privateKey);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            decryptedText = Encoding.UTF8.GetString(decryptedBytes);
        }

        return decryptedText;
    }

    public static string GetPublicKeyAsString(RSAParameters publicKey)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
            return Convert.ToBase64String(publicKeyBytes);
        }
    }

    public static RSAParameters ConvertToPublicKey(string publicKeyString)
    {
        byte[] publicKeyBytes = Convert.FromBase64String(publicKeyString);
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);
            return rsa.ExportParameters(false);
        }
    }
}