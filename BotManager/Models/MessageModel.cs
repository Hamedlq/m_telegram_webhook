using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotManager.Models
{
    public class MessageModel
    {
        public KntuMessageType MsgType { set; get; }
        public string Msg { set; get; }
    }
}
