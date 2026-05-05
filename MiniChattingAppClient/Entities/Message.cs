using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MiniChattingAppClient.Entities
{
    public class Message 
    {
        public int Id { get; set; }
        public string? Type { get; set; } = "message";

        public string? Content { get; set; }
        public bool IsRead { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        public int ReceiverId { get; set; }
        public User? Receiver { get; set; }

    }
}
