using System.Security.Cryptography;

namespace Chat.Server.Models;

public class EncryptMessage
{
    public string ConnectionId { get; set; }
    public string UserName { get; set; }
    public RSAParameters PublicKey { get; set; }
    public byte[] EncryptMsg { get; set; }
}