using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MiniChattingAppClient.Entities
{
    public class User 
    {
        public int Id { get; set; }
        public string? Type { get; set; } = "user";
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsVerified { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? OfflineSince { get; set; }
        public List<Message>? SentMessages { get; set; }
        public List<Message>? ReceivedMessages { get; set; }

        public List<FileMessage>? ReceivedFiles { get; set; }
        public List<FileMessage>? SentFiles { get; set; }

        public string? IpAddress { get; set; }
        public string? Port { get; set; }
    }
}
