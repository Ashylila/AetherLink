using Dalamud.Game.Text;
using System;
#nullable disable

namespace AetherLink.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public XivChatType ChatType { get; set; }
    }
}
