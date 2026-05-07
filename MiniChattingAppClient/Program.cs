using Microsoft.VisualBasic;
using MiniChattingAppClient.Entities;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace MiniChattingAppClient
{
    internal class Program
    {
        static ManualResetEventSlim _usersReady = new ManualResetEventSlim(false);
        static ManualResetEventSlim _messagesReady = new ManualResetEventSlim(false);
        static ManualResetEventSlim _fileMessagesReady = new ManualResetEventSlim(false);
        static ManualResetEventSlim _fileDownloadingReady = new ManualResetEventSlim(false);// events to prevent race condition
        private const string SaveFolder = "ReceivedFiles";
        private const string SaveRecords = "Records";

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
            await Task.Delay(50);
            if (!CheckVerificationStatus(client, email))
            {
            VerificationStart:
                "You are not verified".ShowWarningMessage();
                int code = SendVerificationCode(email, username);
                Console.Write("Enter the code: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int intInput) && intInput == code)
                {
                    User myUser = _users.FirstOrDefault(u => u.Email == email)!;
                    var jsonUser = JsonConvert.SerializeObject(myUser);
                    SendMessage(client, jsonUser);
                    Console.WriteLine("Verified");
                }
                else
                {
                    goto VerificationStart;
                }
            }

            while (true)
            {
                await Task.Delay(400);
                Console.Clear();
                Console.WriteLine("[1]. Show users");
                Console.WriteLine("[2]. Go to received files");

                var choice = Console.ReadKey();
                switch (choice.Key)
                {
                    case (ConsoleKey.D1):
                    case (ConsoleKey.NumPad1):
                        {
                            _usersReady.Reset();
                            _messagesReady.Reset();
                            SendMessage(client, "_users");
                            SendMessage(client, "_chatHistory");
                            _usersReady.Wait();
                            _messagesReady.Wait();
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
                                        ChatPageMessage(client, email, receiverId);
                                        UserStatusEmail(email, receiverId);
                                        break;
                                    }
                                case (ConsoleKey.D2):
                                case (ConsoleKey.NumPad2):
                                    {
                                        SendMessage(client, "--file");
                                        _isReceivingFile = true;
                                        await FileSenderAsync(client, email, receiverId);
                                        UserStatusEmail(email, receiverId);
                                        break;
                                    }
                                case (ConsoleKey.D3):
                                case (ConsoleKey.NumPad3):
                                    {
                                        SendMessage(client, "--voice");
                                        await VoicePage(client, email, receiverId);
                                        UserStatusEmail(email, receiverId);
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
                            _usersReady.Reset();
                            _fileMessagesReady.Reset();
                            SendMessage(client, "_users");
                            _usersReady.Wait();

                            SendMessage(client, "_allFiles");
                            Console.Clear();
                            _fileMessagesReady.Wait(200);
                            User me = _users.FirstOrDefault(u => u.Email == email)!;
                            var myFiles = me.ReceivedFiles;
                            for (int i = 0; i < myFiles.Count; i++)
                            {
                                if (myFiles[i].Path!.EndsWith(".wav"))
                                    Console.WriteLine($"Voice message: {i + 1}. {myFiles[i].Path}");
                                else
                                    Console.WriteLine($"{i + 1}. {myFiles[i].Path}");
                            }


                            if (myFiles.Count > 0)
                            {
                                var fileMessageChoice = int.Parse(Console.ReadLine()!);
                                var chosenFile = myFiles[fileMessageChoice - 1];
                                var readyFileString = "_fileID:" + chosenFile.Id;
                                SendMessage(client, readyFileString);
                                _fileDownloadingReady.Wait();
                                _fileDownloadingReady.Reset();
                                if (!chosenFile.Path!.EndsWith(".wav"))
                                {
                                    var dbPath = myFiles[fileMessageChoice - 1].Path!.Split("\\");
                                    var fullPath = dbPath[0] + " " + email + "\\" + dbPath[1];
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = fullPath,
                                        UseShellExecute = true
                                    });
                                }
                                else
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = myFiles[fileMessageChoice - 1].Path!,
                                        UseShellExecute = true
                                    });
                                }
                            }
                            else
                            {
                                Console.WriteLine("NO received files");
                            }
                            break;
                        }
                    default:
                        break;
                }
            }



        }

        private static int SendVerificationCode(string email, string username)
        {
            string fromEmail = "azrvf4409@gmail.com";
            string appPassword = "dypd akbl ujtc hyhl";
            var mail = new MailMessage();
            Random rnd = new Random();
            int verificationCode = rnd.Next(100000, 999999);

            mail.From = new MailAddress(fromEmail);
            mail.To.Add(email);

            mail.Subject = "Verify Your MiniChat Account";
            mail.IsBodyHtml = true;
            mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 500px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #4A90D9;
            padding: 24px;
            text-align: center;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 22px;
        }}
        .body {{
            padding: 30px;
            text-align: center;
        }}
        .body p {{
            color: #555555;
            font-size: 16px;
            line-height: 1.6;
        }}
        .code {{
            display: inline-block;
            background-color: #f0f6ff;
            color: #4A90D9;
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 10px;
            padding: 16px 32px;
            border-radius: 10px;
            border: 2px dashed #4A90D9;
            margin: 20px 0;
        }}
        .warning {{
            font-size: 13px;
            color: #aaaaaa;
            margin-top: 10px;
        }}
        .footer {{
            background-color: #f4f4f4;
            text-align: center;
            padding: 14px;
            font-size: 12px;
            color: #aaaaaa;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📬 MiniChat</h1>
        </div>
        <div class='body'>
            <p>Hey there,</p>
            <p>Welcome to MiniChat! Use the verification code below to confirm your account:</p>
            <div class='code'>{verificationCode}</div>
            <p class='warning'>This code expires in 10 minutes. Do not share it with anyone.</p>
        </div>
        <div class='footer'>
            MiniChatting App &mdash; Stay connected
        </div>
    </div>
</body>
</html>";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            try
            {
                smtpClient.Send(mail);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message, Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
            }
            return verificationCode;
        }

        private static bool CheckVerificationStatus(TcpClient client, string email)
        {
            _usersReady.Reset();
            SendMessage(client, "_users");
            _usersReady.Wait();
            User myUser = _users.FirstOrDefault(u => u.Email == email)!;
            if (myUser != null && myUser.IsVerified == true)
                return true;
            else if (myUser != null && myUser.IsVerified == false)
                return false;
            return false;
        }


        private static async Task VoicePage(TcpClient client, string email, int receiverId)
        {
            Directory.CreateDirectory("Records");

            DateTime now = DateTime.Now;
            string fileName = now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + email + ".wav";
            string filePath = $"Records/{fileName}";

            string infoOfVoice = now.ToString("yyyy-MM-dd_HH-mm-ss") + "\n" + email + "\n" + receiverId;
            SendMessage(client, infoOfVoice);

            Console.WriteLine("Press any key to start recording (max 2 mins)...");
            Console.ReadKey();

            var waveFormat = new WaveFormat(44100, 1);

            using (var waveFile = new WaveFileWriter(filePath, waveFormat))
            using (var waveIn = new WaveInEvent())
            {
                waveIn.WaveFormat = waveFormat;
                waveIn.DataAvailable += (s, e) =>
                {
                    waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                };

                waveIn.StartRecording();
                Console.WriteLine("Recording... Press any key to stop");
                Console.ReadKey();
                waveIn.StopRecording();
                Console.WriteLine("Stopped");
            }

            Console.WriteLine($"Saved at Records\\{fileName}");

            var audioBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            _isReceivingFile = true;
            var stream = client.GetStream();
            var bw = new BinaryWriter(stream);
            bw.Write((long)audioBytes.Length);
            await stream.WriteAsync(audioBytes);
            _isReceivingFile = false;

            Console.WriteLine("Voice sent successfully");
        }


        private static async Task FileGetter(TcpClient client, string email)
        {
            var perUserFolder = SaveFolder + " " + email;
            Directory.CreateDirectory(perUserFolder);
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

            string savePath = Path.Combine(perUserFolder, fileName);

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
            _fileDownloadingReady.Set();

        }

        private static async Task FileSenderAsync(TcpClient client, string email, int receiverId)
        {
            var address = email + "\n" + receiverId;
            SendMessage(client, address);
            Console.WriteLine("Enter file path : ");
            string? filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
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
            var receiverUser = _users.FirstOrDefault(u => u.Id == receiverId);
            var senderUser = _users.FirstOrDefault(u => u.Email == email);
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
                    break;
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

        private static void UserStatusEmail(string myEmail, int receiverId)
        {
            var senderUser = _users.FirstOrDefault(u => u.Email == myEmail);
            var receiverUser = _users.FirstOrDefault(u => u.Id == receiverId);
            if (receiverUser!.IsOnline == false)
            {
                string fromEmail = "azrvf4409@gmail.com";
                string appPassword = "dypd akbl ujtc hyhl";
                string toEmail = receiverUser.Email!;
                var mail = new MailMessage();

                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);

                mail.Subject = "New Message";
                mail.IsBodyHtml = true;
                mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 500px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #4A90D9;
            padding: 24px;
            text-align: center;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 22px;
        }}
        .body {{
            padding: 30px;
            text-align: center;
        }}
        .body p {{
            color: #555555;
            font-size: 16px;
            line-height: 1.6;
        }}
        .badge {{
            display: inline-block;
            background-color: #4A90D9;
            color: white;
            padding: 10px 24px;
            border-radius: 20px;
            font-size: 15px;
            margin-top: 16px;
        }}
        .footer {{
            background-color: #f4f4f4;
            text-align: center;
            padding: 14px;
            font-size: 12px;
            color: #aaaaaa;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📬 MiniChat</h1>
        </div>
        <div class='body'>
            <p>Hey there,</p>
            <p>You missed a message while you were offline.</p>
            <p>Log back in to see what you missed!</p>
            <div class='badge'>You have a new message</div>
        </div>
        <div class='footer'>
            MiniChatting App &mdash; Stay connected
        </div>
    </div>
</body>
</html>";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true
                };

                try
                {
                    smtpClient.Send(mail);
                    Console.WriteLine("Email notification sent");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(ex.Message, Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                }
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
                    Console.Write("New message(s) from ");

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
                    _ = ServerReader(client, email);
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR: ", Console.ForegroundColor = ConsoleColor.DarkYellow);
                Console.WriteLine(ex.Message, Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
            }
        }

        private static async Task ServerReader(TcpClient client, string email)
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
                if (result == "--FileStartReady")
                {
                    _isReceivingFile = true;
                    await FileGetter(client, email);
                }
                else if (result.StartsWith(":I"))
                {
                    result = result.Remove(0, 3);
                    Console.WriteLine(result);
                }
                else if (Helper.IsValidJson(result))
                {
                    var jsonFile = JsonDocument.Parse(result);
                    if (jsonFile.RootElement.ValueKind == JsonValueKind.Array &&
                        jsonFile.RootElement.GetArrayLength() != 0)
                    {
                        if (jsonFile.RootElement.GetArrayLength() == 0)
                        {
                            _usersReady.Set();
                            _messagesReady.Set();
                        }
                        else
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
