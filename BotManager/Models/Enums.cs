using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotManager.Models
{
    public enum KntuMessageType
    {
        Initial = 5,
        Start = 10,
        HasInvite=15,
        NoInvite=20,
        BotRegistered = 25,
        IncreaseScore = 30,
        MyStatus = 35,
        
        CityState = 35,
        UnknownMessage=500,
        Historical=600,
        Knturoute=1001,
        TimeSet=1005,
        SelectRoute=1010,
        NotifSetting=1015,
    }

    public enum BotId
    {
        BenzinBaMa=1,
        KntuBot=2,
    }
    public enum NotifStatus
    {
        On = 1,
        Off = 2,
    }

    public enum NotifSeq
    {
        NotNotifed = 1,
        OneNotifed = 2,
        TwoNotifed = 3,
    }
}
