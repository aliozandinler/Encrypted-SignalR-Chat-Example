using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Hubs;

public class ChatHub : Hub
{
    public async Task GetNickName(string userName, string publicKey)
    {
        Client client = new Client
        {
            ConnectionId = Context.ConnectionId,
            UserName = userName,
            PublicKey = publicKey
        };

        ClientSource.Clients.Add(client);
        await Clients.Others.SendAsync("clientJoined", userName, ClientSource.Clients);
        // await Clients.All.SendAsync("getUsers", ClientSource.Clients);
    }

    public async Task SendEndcryptMessages(List<EncryptMessage> encryptedClients)
    {
        Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        foreach (EncryptMessage c in encryptedClients)
        {
            if (c.UserName != client.UserName)
            {
                await Clients.Client(c.ConnectionId).SendAsync("receiveMessage", client.UserName, c.EncryptMsg);
                Console.WriteLine($"SendEndcryptMessages: {c.UserName} {c.EncryptMsg}");
            }
        }

        Console.WriteLine("SendEndcryptMessages");
    }

    public async Task SendPM(string connectionId, string message)
    {
        Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        await Clients.Client(connectionId).SendAsync("receiveMessage", client.UserName, message);
    }

    public async Task GetUsers()
    {
        await Clients.Caller.SendAsync("getUsers", ClientSource.Clients);
    }
}