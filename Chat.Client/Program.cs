using Chat.Client.Common;
using Chat.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client;

internal class Program
{
    private static HubConnection _connection;
    private static List<Models.Client> _clients = new();


    public static async Task Main(string[] args)
    {
        Encryption.GenerateKeys();
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5011/chatHub")
            .Build();

        _connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _connection.StartAsync();
        };

        await ConnectAsync();

        Console.WriteLine("Enter your nickname");
        string nickName = Console.ReadLine();

        await GetNickName(nickName);
        await _connection.InvokeAsync("GetUsers");

        while (true)
        {
            string input = Console.ReadLine();
            string[] arguments = input.Split(" ");
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
                    await _connection.InvokeAsync("GetUsers");
                    break;
                case "/pm":
                    if (arguments.Length < 3)
                    {
                        Console.WriteLine("Invalid arguments");
                        break;
                    }

                    string userName = arguments[1];
                    string msg = string.Join(" ", arguments.Skip(2));
                    await SendPM(userName, msg);
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
        Console.WriteLine("\n=======Commands=======");
        Console.WriteLine("/exit - Exit the application");
        Console.WriteLine("/getusers - Get all users");
        Console.WriteLine("/pm <user> <message> - Send a private message to a user");
    }

    private static async Task ConnectAsync()
    {
        _connection.On<string, byte[]>("receiveMessage",
            (user, message) => { Console.WriteLine($"\n[{DateTime.Now:T}] {user}: {Encryption.Decrypt(message)}"); });
        _connection.On<string, List<Models.Client>>("clientJoined",
            (userName, clients) =>
            {
                _clients = clients;
                // _connection.InvokeAsync("GetUsers");
                Console.WriteLine($"\n[{DateTime.Now:T}] {userName} joined");
            });

        _connection.On<List<Models.Client>>("getUsers", (clients) =>
        {
            _clients = clients;
            Console.WriteLine($"\n========Clients:========{clients.Count}");
            foreach (Models.Client client in clients)
            {
                Console.WriteLine($"{client.UserName} - {client.PublicKey}");
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
        try
        {
            string stringPublicKey = Encryption.GetPublicKeyAsString(Encryption._publicKey);
            await _connection.InvokeAsync("GetNickName", nickName, stringPublicKey);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }

    private static async Task SendPM(string userName, string msg)
    {
        try
        {
            Models.Client? client = _clients.FirstOrDefault(client => client.UserName == userName);
            await _connection.InvokeAsync("SendPM", client.ConnectionId, Encryption.Encrypt(msg, Encryption.ConvertToPublicKey(client.PublicKey)));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }

    private static async void SendEncryptedMessage(string msg)
    {
        try
        {
            List<EncryptMessage> encryptMessages = _clients.Select(client => new EncryptMessage
            {
                ConnectionId = client.ConnectionId,
                UserName = client.UserName,
                EncryptMsg = Encryption.Encrypt(msg, Encryption.ConvertToPublicKey(client.PublicKey))
            }).ToList();

            await _connection.InvokeAsync("SendEndcryptMessages", encryptMessages);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }
}