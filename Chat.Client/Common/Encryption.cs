using System.Security.Cryptography;
using System.Text;

namespace Chat.Client.Common;

public class Encryption
{
    public static RSAParameters _publicKey;
    private static RSAParameters _privateKey;
    private static RSA _rsa = RSA.Create();

    public static void GenerateKeys()
    {
        _publicKey = _rsa.ExportParameters(false);
        _privateKey = _rsa.ExportParameters(true);
    }

    public static byte[] Encrypt(string plainText)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(_publicKey);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
    }

    public static byte[] Encrypt(string plainText, RSAParameters publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(publicKey);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
    }

    public static string Decrypt(byte[] encryptedBytes)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(_privateKey);
        var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string Decrypt(byte[] encryptedBytes, RSAParameters privateKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(privateKey);
        var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string GetPublicKeyAsString(RSAParameters publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(publicKey);
        var publicKeyBytes = rsa.ExportRSAPublicKey();
        return Convert.ToBase64String(publicKeyBytes);
    }

    public static RSAParameters ConvertToPublicKey(string publicKeyString)
    {
        var publicKeyBytes = Convert.FromBase64String(publicKeyString);
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        return rsa.ExportParameters(false);
    }
}