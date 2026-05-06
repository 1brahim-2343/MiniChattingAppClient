using MiniChattingAppClient.Entities;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MiniChattingAppClient
{
    internal class Program
    {

        private static List<User> _users = [];
        private static List<Message> _messages = [];

        static async Task Main()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var client = new TcpClient();
            var ipAddress = IPAddress.Parse("10.0.14.43");
            var port = 44000;
            var ep = new IPEndPoint(ipAddress, port);
            Console.Write("Enter email: ", Console.ForegroundColor = ConsoleColor.White);
            Console.ResetColor();
            string email = Console.ReadLine()!;
            while (!Helper.IsValidEmail(email))
            {
                Console.Clear();
                Console.WriteLine("Invalid email", Console.ForegroundColor = ConsoleColor.DarkYellow);
                Console.WriteLine("Enter email: ", Console.ForegroundColor = ConsoleColor.White);
                Console.ResetColor();
                email = Console.ReadLine()!;
            }
            Console.Write("Enter username: ", Console.ForegroundColor = ConsoleColor.White);
            Console.ResetColor();
            string username = Console.ReadLine()!;
            _ = Task.Factory.StartNew(() => { ConnectServer(ep, client, email, username); },
                 TaskCreationOptions.LongRunning);
            await Task.Delay(10);

            Console.WriteLine("[1]. Show users");
            Console.WriteLine("[2]. Go to chat history");

            var choice = Console.ReadKey();
            switch (choice.Key)
            {
                case (ConsoleKey.D1):
                case (ConsoleKey.NumPad1):
                    {
                        SendMessage(client, "_users");
                        SendMessage(client, "_chatHistory");
                        Console.Clear();
                        ShowUsers(email);
                        ShowMessagesStatus(email);
                    UserChoice:
                        Console.Write("Choose user to chat with: ");
                        string strIndex = Console.ReadLine()!;
                        if (!int.TryParse(strIndex, out int intIndex))
                        {
                            "Invalid input".ShowRedText();
                            goto UserChoice;
                        }
                        var receiverId = _users[intIndex - 1].Id;
                        ChatPage(client, email, receiverId);
                        break;
                    }
                case (ConsoleKey.D2):
                case (ConsoleKey.NumPad2):
                    {
                        SendMessage(client, "_chatHistory");
                        Console.Clear();
                        ChatHistory(email, client);
                        break;
                    }
                default:
                    break;
            }



        }

        private static void ChatHistory(string myEmail, TcpClient tcpClient)
        {
            var myUser = _users.FirstOrDefault(u => u.Email == myEmail);
            var myReceivedMessages = _messages.Where(m => m.SenderId == myUser!.Id);
            var groupped = myReceivedMessages.GroupBy(u => u.SenderId);
            foreach (var sender in groupped)
            {
                var user = _users.FirstOrDefault(u => u.Id == sender.Key);
                Console.WriteLine(user!.Username);
                foreach (var messages in sender)
                {
                    Console.WriteLine(messages.Content);
                }
            }
        }

        private static void ChatPage(TcpClient client, string senderEmail, int receiverId)
        {
            var receiverUser = _users.FirstOrDefault(u => u.Id == receiverId);
            var senderUser = _users.FirstOrDefault(u => u.Email == senderEmail);

            var messages = _messages.Where(m => m.SenderId == senderUser.Id || m.ReceiverId == receiverId);

            Console.WriteLine($"=====Chat With \"{receiverUser!.Username}\"=====");
            foreach (var msg in messages)
            {
                if (msg.SenderId == senderUser.Id)
                {
                    msg.Content.ShowMsgSender();
                }
            }
            while (true)
            {
                Console.Write("Enter message: ");
                var msg = Console.ReadLine();
                Chat chat = new Chat
                {
                    SenderEmail = senderEmail,
                    Content = msg,
                    ReceiverEmail = receiverUser.Email
                };
                var json = JsonConvert.SerializeObject(chat);
                SendMessage(client, json);
            }
        }
        private static void ShowUsers(string email)
        {
            if (_users.Count == 0)
            {
                "No online\\offline user".ShowWarningMessage();
            }
            else if(_users.Count == 1)
            {
                "No other user except you, no one to chat with".ShowWarningMessage();
            }
            else
            {

                for (int i = 0; i < _users.Count; i++)
                {
                    if (_users[i].Email == email)
                        continue;
                    Console.Write($"{i + 1}. {_users[i].Username} is ");
                    if (_users[i].IsOnline)
                        "Online".ShowGreenText();
                    else
                    {
                        "OFFLINE".ShowRedText();
                    }

                }

            }
        }

        private static void ShowMessagesStatus(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("No new message");
                return;
            }
            if (user.ReceivedMessages == null)
            {
                Console.WriteLine("No new message");
                return;
            }
            var unreadMessages = user!.ReceivedMessages!.Where(m => m.IsRead == false).GroupBy(m => m.SenderId);
            if (unreadMessages.ToList().Count != 0)
            {
                foreach (var message in unreadMessages)
                {
                    Console.Write("New message(s) from");
                    Console.WriteLine(_users.FirstOrDefault(u => u.Id == message.Key)!.Username);
                }
            }
        }

        private static void ConnectServer(IPEndPoint ep, TcpClient client, string email, string username)
        {
            try
            {
                client.Connect(ep);
                if (client.Connected)
                {
                    Console.WriteLine("Connected successfully", Console.ForegroundColor = ConsoleColor.Green);
                    Console.ResetColor();

                    var stream = client.GetStream();
                    var bw = new BinaryWriter(stream);
                    bw.Write(email);
                    bw.Write(username ?? "Unknown");
                    var br = new BinaryReader(stream);
                    ServerReader(client);
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR: ", Console.ForegroundColor = ConsoleColor.DarkYellow);
                Console.WriteLine(ex.Message, Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
            }
        }

        private static void ServerReader(TcpClient client)
        {
            var stream = client.GetStream();
            var br = new BinaryReader(stream);
            while (true)
            {
                var result = br.ReadString();
                if (result.StartsWith(":E"))
                {
                    result.Remove(0, 3).ShowRedText();
                    _ = Main();
                }
                if (result.StartsWith(":I"))
                {
                    result = result.Remove(0, 3);
                    Console.WriteLine(result);
                }
                else if (Helper.IsValidJson(result))
                {
                    var jsonFile = JsonDocument.Parse(result);
                    var type = jsonFile.RootElement[0].GetProperty("Type").GetString();
                    if (type == "user")
                        _users = JsonConvert.DeserializeObject<List<User>>(result)!;
                    else if (type == "message")
                        _messages = JsonConvert.DeserializeObject<List<Message>>(result)!;
                }
                else
                    continue;
            }
        }

        private static void SendMessage(TcpClient client, string msg)
        {
            var bw = new BinaryWriter(client.GetStream());
            try
            {
                bw.Write(msg);
            }
            catch (Exception ex)
            {
                ex.Message.ShowRedText();
            }


        }


    }
}
