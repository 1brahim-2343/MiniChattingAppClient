using MiniChattingAppClient.Entities;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MiniChattingAppClient
{
    internal class Program
    {
        static ManualResetEventSlim _usersReady = new ManualResetEventSlim(false);
        static ManualResetEventSlim _messagesReady = new ManualResetEventSlim(false);
        static ManualResetEventSlim _fileMessagesReady = new ManualResetEventSlim(false); // events to prevent race condition
        private const string SaveFolder = "ReceivedFiles";

        private static List<User> _users = [];
        private static List<Message> _messages = [];
        private static List<FileMessage> _fileMessages = [];
        static bool _isReceivingFile = false;
        static async Task Main()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var client = new TcpClient();
            var ipAddress = IPAddress.Parse("192.168.0.239");
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
            while (true)
            {
                Console.WriteLine("[1]. Show users");
                Console.WriteLine("[2]. Go to received files");

                var choice = Console.ReadKey();
                switch (choice.Key)
                {
                    case (ConsoleKey.D1):
                    case (ConsoleKey.NumPad1):
                        {
                            SendMessage(client, "_users");
                            SendMessage(client, "_chatHistory");
                            _usersReady.Wait();
                            _messagesReady.Wait(100);
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
                            Console.WriteLine("[1]. For sending message");
                            Console.WriteLine("[2]. For sending file");
                            Console.WriteLine("[3]. For sending audio");
                            Console.Write("Choose: ");
                            var choice2 = Console.ReadKey();
                            switch (choice2.Key)
                            {
                                case (ConsoleKey.D1):
                                case (ConsoleKey.NumPad1):
                                    {


                                        //UserChoice:
                                        //    Console.Write("Choose user to chat with: ");
                                        //    string strIndex = Console.ReadLine()!;
                                        //    if (!int.TryParse(strIndex, out int intIndex))
                                        //    {
                                        //        "Invalid input".ShowRedText();
                                        //        goto UserChoice;
                                        //    }
                                        //    var receiverId = _users[intIndex - 1].Id;
                                        ChatPageMessage(client, email, receiverId);
                                        break;
                                    }
                                case (ConsoleKey.D2):
                                case (ConsoleKey.NumPad2):
                                    {
                                        SendMessage(client, "--file");
                                        _isReceivingFile = true;
                                        await FileSender(client, email, receiverId);
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;

                        }
                    case (ConsoleKey.D2):
                    case (ConsoleKey.NumPad2):
                        {
                            SendMessage(client, "_users");
                            _usersReady.Wait();

                            SendMessage(client, "_allFiles");
                            Console.Clear();
                            _fileMessagesReady.Wait(200);
                            User me = _users.FirstOrDefault(u => u.Email == email)!;
                            var myFiles = me.ReceivedFiles;
                            foreach (var myFile in myFiles)
                            {
                                Console.WriteLine($"{myFile.Id}. {myFile.Path}");
                            }
                            var fileMessageChoice = int.Parse(Console.ReadLine()!);
                            var readyFileString = "_fileID:" + fileMessageChoice;
                            SendMessage(client, readyFileString);
                            await FileGetter(client);
                            break;
                        }
                    default:
                        break;
                }
            }



        }

        private static async Task FileGetter(TcpClient client)
        {
            Directory.CreateDirectory(SaveFolder);
            var stream = client.GetStream();

            //1. Read filename len

            byte[] fileNameLengthBuffer = new byte[4];
            await stream.ReadExactlyAsync(fileNameLengthBuffer, 0, 4);
            int length = BitConverter.ToInt32(fileNameLengthBuffer, 0);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);

            //2. Read filename
            byte[] fileNameBuffer = new byte[fileNameLength];
            await stream.ReadExactlyAsync(fileNameBuffer, 0, fileNameLength);

            string fileName = Encoding.UTF8.GetString(fileNameBuffer);

            fileName = Path.GetFileName(fileName);

            //3. Read file size - long 8 byte

            byte[] fileSizeBuffer = new byte[8];
            await stream.ReadExactlyAsync(fileSizeBuffer, 0, 8);

            long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);

            Console.WriteLine($"Receiving file: {fileName}");
            Console.WriteLine($"File size: {fileSize} bytes");

            string savePath = Path.Combine(SaveFolder, fileName);

            //4. Read file bytes and save
            byte[] buffer = new byte[8192];
            long totalReceived = 0;

            await using var fileStream = new FileStream(
                savePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                useAsync: true
                );
            while (totalReceived < fileSize)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, fileSize - totalReceived);

                int received = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead));

                if (received == 0)
                {
                    throw new IOException("Disconnected before file transfer completion");
                }

                await fileStream.WriteAsync(buffer.AsMemory(0, received));

                totalReceived += received;
            }
            Console.WriteLine($"File saved:{savePath}");
            _isReceivingFile = false;

        }

        private static async Task FileSender(TcpClient client, string email, int receiverId)
        {
            var address = email + "\n" + receiverId;
            SendMessage(client, address);
            Console.WriteLine("Enter file path : ");
            string? filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File not found");
                return;
            }

            string fileName = Path.GetFileName(filePath);
            long fileSize = new FileInfo(filePath).Length;

            var stream = client.GetStream();

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);// name to bytes
            byte[] fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length);//name.Length to bytes

            await stream.WriteAsync(fileNameLengthBytes);
            await stream.WriteAsync(fileNameBytes);


            byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
            await stream.WriteAsync(fileSizeBytes);

            byte[] buffer = new byte[8192];
            long totalSent = 0;

            await using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: true
                );

            int read;

            while ((read = await fileStream.ReadAsync(buffer)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, read));
                totalSent += read;
            }

            Console.WriteLine("File sent successfully");
        }

        private static void ChatPageMessage(TcpClient client, string myEmail, int receiverId)
        {
            var receiverUser = _users.FirstOrDefault(u => u.Id == receiverId);
            var senderUser = _users.FirstOrDefault(u => u.Email == myEmail);

            var messages = _messages.Where(m => m.SenderId == senderUser!.Id || m.ReceiverId == senderUser.Id);
            var messagesToBeRead = _messages.Where(m => m.ReceiverId == senderUser!.Id);
            foreach (var readMessage in messagesToBeRead)
            {
                readMessage.IsRead = true;
            }
            var json = JsonConvert.SerializeObject(messagesToBeRead);
            SendMessage(client, json);
            Console.WriteLine($"=====Chat With \"{receiverUser!.Username}\"=====");
            if (receiverUser != null && senderUser != null)
            {
                foreach (var msg in messages)
                {

                    var fullMsg = msg.SentTime.ToString() + ": " + msg.Content;
                    if (msg.SenderId == senderUser.Id && msg.ReceiverId == receiverUser.Id)
                    {
                        fullMsg.ShowMsgSender();
                    }
                    else if (msg.SenderId == receiverUser.Id)
                    {
                        Console.WriteLine(fullMsg);
                    }
                }   
            }
            while (true)
            {
                Console.Write("Enter message(_exit to leave chat): ");
                var msg = Console.ReadLine();
                if (msg == "_exit")
                {

                    return;
                }
                Chat chat = new Chat
                {
                    SenderEmail = myEmail,
                    Content = msg,
                    ReceiverEmail = receiverUser!.Email,
                    SendingTime = DateTime.UtcNow.AddHours(4)
                };
                json = JsonConvert.SerializeObject(chat);
                SendMessage(client, json);
            }
        }
        private static void ShowUsers(string email)
        {
            if (_users.Count == 0)
            {
                "No online\\offline user".ShowWarningMessage();
            }
            else if (_users.Count == 1)
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
            var unreadMessages = user!.ReceivedMessages!.Where(m => m.IsRead == false && m.ReceiverId == user.Id).GroupBy(m => m.SenderId);
            if (unreadMessages.ToList().Count != 0)
            {
                foreach (var message in unreadMessages)
                {
                    Console.Write("New message(s) from");
                    Console.WriteLine(_users.FirstOrDefault(u => u.Id == message.Key)!.Username);
                }
            }
            else
            {
                Console.WriteLine("No new message");
                return;
            }
        }

        #region ConnectionRelated

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
                if (_isReceivingFile)
                {
                    Thread.Sleep(50);
                    continue;
                }
                var result = br.ReadString();
                if (result.StartsWith(":E"))
                {
                    result.Remove(0, 3).ShowRedText();
                    Environment.Exit(0);
                }
                if (result.StartsWith(":I"))
                {
                    result = result.Remove(0, 3);
                    Console.WriteLine(result);
                }
                else if (Helper.IsValidJson(result))
                {
                    var jsonFile = JsonDocument.Parse(result);
                    if (jsonFile.RootElement.GetArrayLength() != 0)
                    {
                        var type = jsonFile.RootElement[0].GetProperty("Type").GetString();
                        if (type == "user")
                        {
                            _users = JsonConvert.DeserializeObject<List<User>>(result)!;
                            _usersReady.Set();
                        }
                        else if (type == "message")
                        {
                            _messages = JsonConvert.DeserializeObject<List<Message>>(result)!;
                            _messagesReady.Set();
                        }
                        else if (type == "fileMessage")
                        {
                            _fileMessages = JsonConvert.DeserializeObject<List<FileMessage>>(result)!;
                            _fileMessagesReady.Set();
                        }
                    }
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

        #endregion

    }
}
