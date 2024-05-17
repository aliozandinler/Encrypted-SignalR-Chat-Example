using Chat.Server.Common;
using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Hubs;

public class ChatHub : Hub
{
    public async Task GetNickName(string userName, string publicKey)
    {
        var client = new Client
        {
            ConnectionId = Context.ConnectionId,
            UserName = userName,
            PublicKey = publicKey
        };

        ClientSource.Clients.Add(client);
        await Clients.Others.SendAsync(nameof(MethodNames.ClientJoined), userName, ClientSource.Clients);
        // await Clients.All.SendAsync("GetUsers", ClientSource.Clients);
    }

    public async Task SendEndcryptMessages(List<EncryptMessage> encryptedClients)
    {
        var client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        if (client != null)
        {
            foreach (var c in encryptedClients.Where(c => c.UserName != client.UserName))
            {
                await Clients.Client(c.ConnectionId).SendAsync(nameof(MethodNames.ReceiveMessage), client.UserName, c.EncryptMsg);
                Console.WriteLine($"SendEndcryptMessages: {c.UserName} {c.EncryptMsg}");
            }
        }
    }

    public async Task SendPrivateMessage(string connectionId, string message)
    {
        var client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        if (client != null)
            await Clients.Client(connectionId).SendAsync(nameof(MethodNames.ReceiveMessage), client.UserName, message);
    }

    public async Task GetUsers()
    {
        await Clients.Caller.SendAsync("GetUsers", ClientSource.Clients);
    }
}