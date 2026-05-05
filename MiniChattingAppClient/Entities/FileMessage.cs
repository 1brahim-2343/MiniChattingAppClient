using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChattingAppClient.Entities
{
    public class FileMessage
    {
        public int Id { get; set; }
        public string? Type { get; set; } = "fileMessage";

        public string? Path { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        public int ReceiverId { get; set; }
        public User? Receiver { get; set; }
    }
}
