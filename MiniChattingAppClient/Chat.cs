using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChattingAppClient
{
    internal class Chat
    {
        public string? Type { get; set; } = "chat";
        public string? SenderEmail { get; set; }
        public string? Content { get; set; }
        public string? ReceiverEmail { get; set; }
        public DateTime SendingTime { get; set; }
    }
}
