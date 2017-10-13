using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotManager.Models
{
    public class UserModel
    {
        public long TuserId { set; get; }
        public string Name { set; get; }
        public string Family { set; get; }
        public string UserName { set; get; }
        public DateTime CreateTime { set; get; }
        public long UserId { set; get; }
        public long TelegramUserId { set; get; }
        public string PhoneNumber { set; get; }
        public long ChatId { set; get; }
    }
}
