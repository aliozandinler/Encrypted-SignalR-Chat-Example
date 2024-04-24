namespace Chat.Client.Models;

public class EncryptMessage
{
    public string ConnectionId { get; set; }
    public string UserName { get; set; }
    public byte[] EncryptMsg { get; set; }
}