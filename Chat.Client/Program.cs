using Chat.Client.Common;
using Chat.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client;

internal class Program
{
    private static HubConnection _connection;
    private static List<Models.Client> _clients = new();


    public static async Task Main()
    {
        Encryption.GenerateKeys();
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5011/chatHub")
            .Build();

        _connection.Closed += async (_) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _connection.StartAsync();
        };

        await ConnectAsync();

        Console.WriteLine("Enter your nickname");
        var nickName = Console.ReadLine();

        await GetNickName(nickName);
        await _connection.InvokeAsync(nameof(MethodNames.GetUsers));

        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;
            var arguments = input.Split(" ");
            switch (arguments[0])
            {
                case "/exit":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exiting...");
                    await _connection.StopAsync();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                    break;
                case "/getusers":
                    Console.WriteLine("Getting users...");
                    await _connection.InvokeAsync(nameof(MethodNames.GetUsers));
                    break;
                case "/pm":
                    if (arguments.Length < 3)
                    {
                        Console.WriteLine("Invalid arguments");
                        break;
                    }

                    var userName = arguments[1];
                    var msg = string.Join(" ", arguments.Skip(2));
                    await SendPrivateMessage(userName, msg);
                    break;
                case "/help":
                    PrintHelp();
                    break;
                default:
                    SendEncryptedMessage(input);
                    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    Console.WriteLine($"[{DateTime.Now:T}] {nickName}: {input}");
                    break;
            }
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
                          =======Commands=======
                          /exit - Exit the applicatio
                          /getusers - Get all users
                          /pm <user> <message> - Send a private message to a user
                          """);
    }

    private static async Task ConnectAsync()
    {
        _connection.On<string, byte[]>(nameof(MethodNames.ReceiveMessage),
            (user, message) => { Console.WriteLine($"\n[{DateTime.Now:T}] {user}: {Encryption.Decrypt(message)}"); });
        _connection.On<string, List<Models.Client>>(nameof(MethodNames.ClientJoined),
            (userName, clients) =>
            {
                _clients = clients;
                // _connection.InvokeAsync("GetUsers");
                Console.WriteLine($"\n[{DateTime.Now:T}] {userName} joined");
            });

        _connection.On<List<Models.Client>>(nameof(MethodNames.GetUsers), (clients) =>
        {
            _clients = clients;
            Console.WriteLine($"\n========Clients:======== Total Count:{clients.Count}");
            foreach (var client in clients)
            {
                Console.WriteLine($"UserName: {client.UserName}, PublicKey: {client.PublicKey}");
            }
        });

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }

    private static async Task GetNickName(string nickName)
    {
        var stringPublicKey = Encryption.GetPublicKeyAsString(Encryption._publicKey);
        await _connection.InvokeAsync(nameof(MethodNames.GetNickName), nickName, stringPublicKey);
    }

    private static async Task SendPrivateMessage(string userName, string msg)
    {
        var client = _clients.FirstOrDefault(client => client.UserName == userName);
        if (client != null)
            await _connection.InvokeAsync(nameof(MethodNames.SendPrivateMessage), client.ConnectionId, Encryption.Encrypt(msg, Encryption.ConvertToPublicKey(client.PublicKey)));
    }

    private static async void SendEncryptedMessage(string msg)
    {
        var encryptMessages = _clients.Select(client => new EncryptMessage
        {
            ConnectionId = client.ConnectionId,
            UserName = client.UserName,
            EncryptMsg = Encryption.Encrypt(msg, Encryption.ConvertToPublicKey(client.PublicKey))
        }).ToList();

        await _connection.InvokeAsync(nameof(MethodNames.SendEndcryptMessages), encryptMessages);
    }
}