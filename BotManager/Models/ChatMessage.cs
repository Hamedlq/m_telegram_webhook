using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotManager.Models
{
    public class ChatMessage
    {
        public long ChatId { set; get; }
        public string PhoneNumber { set; get; }
        public string Msg { set; get; }
        public string Time { set; get; }

    }
}
