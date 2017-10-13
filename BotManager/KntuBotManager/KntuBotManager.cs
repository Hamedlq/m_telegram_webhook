using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BotManager.Models;
using CoreManager.Helper;

namespace BotManager.KntuBotManager
{
    public class KntuBotManager
    {
        public UserModel GetUser(long tuserId, string fromUsername, string fromFirstName, string fromLastName,
            string phoneNumber)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var res = new UserModel();
                var user = dataModel.TelegramUsers.FirstOrDefault(x => x.TelegramUserId == tuserId);
                if (user == null)
                {
                    var tuser = new TelegramUser();
                    tuser.TmsgUserName = fromUsername;
                    tuser.TmsgName = fromFirstName;
                    tuser.TelegramUserId = tuserId;
                    tuser.TmsgFamily = fromLastName;
                    tuser.TcreateTime = DateTime.Now;
                    tuser.TbotId = (int) BotId.KntuBot;
                    tuser.PhoneNumber = phoneNumber;
                    dataModel.TelegramUsers.Add(tuser);
                    dataModel.SaveChanges();
                    user = tuser;
                }
                if (string.IsNullOrEmpty(user.PhoneNumber) && !string.IsNullOrEmpty(phoneNumber))
                {
                    user.PhoneNumber = phoneNumber;
                    dataModel.SaveChanges();
                }
                res.TuserId = user.TuserId;
                res.UserName = user.TmsgUserName;
                res.Name = user.TmsgName;
                res.TelegramUserId = tuserId;
                res.Family = user.TmsgFamily;
                res.CreateTime = user.TcreateTime;
                res.PhoneNumber = user.PhoneNumber;
                res.UserId = user.UserId == null ? 0 : (long) user.UserId;
                return res;
            }
        }

        public void SetMessage(UserModel user, string text, long chatId, int type)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var tmsg = new TelegramMsg();
                tmsg.TuserId = user.TuserId;
                tmsg.TmsgCreateTime = DateTime.Now;
                tmsg.TmsgMessage = text;
                tmsg.TmsgType = type;
                tmsg.ChatId = chatId;
                dataModel.TelegramMsgs.Add(tmsg);
                dataModel.SaveChanges();
            }
        }

        public MessageModel GetLastMessage(UserModel user)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var msg = new MessageModel();
                var lastMsg =
                    dataModel.TelegramMsgs.OrderByDescending(x => x.TmsgCreateTime)
                        .FirstOrDefault(x => x.TuserId == user.TuserId && x.TmsgType != (int) KntuMessageType.Historical);
                if (lastMsg != null)
                {
                    msg.Msg = lastMsg.TmsgMessage;
                    msg.MsgType = (KntuMessageType) lastMsg.TmsgType;
                }
                else
                {
                    msg.Msg = "";
                    msg.MsgType = KntuMessageType.Initial;
                }

                return msg;
            }
        }

        public bool CheckInviteCode(UserModel user, string text)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var invite = dataModel.Invites.FirstOrDefault(x => x.InviteCode == text);
                if (invite != null)
                {
                    return true;
                }
                return false;
            }
        }


        public bool SaveUser(UserModel user, long tuserId, string phoneNumber, bool mobileConfirmed)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var au = dataModel.AspNetUsers.FirstOrDefault(x => x.UserName == phoneNumber);
                if (au != null)
                {
                    var tu = dataModel.TelegramUsers.FirstOrDefault(x => x.TuserId == user.TuserId);
                    tu.UserId = au.Id;
                    tu.TelegramUserId = tuserId;
                    dataModel.SaveChanges();

                    var invo = dataModel.Invites.FirstOrDefault(x => x.UserId == au.Id);
                    if (invo != null)
                    {
                        return true;
                    }
                    else
                    {
                        var inv = new Invite();
                        inv.UserId = au.Id;
                        inv.CreateTime = DateTime.Now;
                        inv.InviteType = 3;
                        inv.InviteCode = InviteCodeGenerator();
                        dataModel.Invites.Add(inv);
                        dataModel.SaveChanges();
                    }
                }
                else
                {
                    var aspnetuser = new AspNetUser();
                    aspnetuser.Family = user.Family;
                    aspnetuser.Gender = 0;
                    aspnetuser.EmailConfirmed = false;
                    string salt;
                    var pass = HashPassword("mibarimpass", out salt);
                    aspnetuser.PasswordHash = pass;
                    aspnetuser.SecurityStamp = salt;
                    aspnetuser.UserName = phoneNumber;
                    aspnetuser.MobileConfirmed = mobileConfirmed;
                    dataModel.AspNetUsers.Add(aspnetuser);
                    dataModel.SaveChanges();
                    var ui = new UserInfo();
                    ui.UserId = aspnetuser.Id;
                    ui.UserInfoCreateTime = DateTime.Now;
                    ui.UserInfoIsDeleted = false;
                    dataModel.UserInfoes.Add(ui);
                    dataModel.SaveChanges();
                    var invi = new Invite();
                    invi.UserId = aspnetuser.Id;
                    invi.CreateTime = DateTime.Now;
                    invi.InviteType = 3;
                    invi.InviteCode = InviteCodeGenerator();
                    dataModel.Invites.Add(invi);
                    dataModel.SaveChanges();
                    var tu = dataModel.TelegramUsers.FirstOrDefault(x => x.TuserId == user.TuserId);
                    tu.UserId = aspnetuser.Id;
                    tu.TelegramUserId = tuserId;
                    dataModel.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public string GetInviteCode(UserModel user)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var tu = dataModel.TelegramUsers.FirstOrDefault(x => x.TuserId == user.TuserId);
                var invite = dataModel.Invites.FirstOrDefault(x => x.UserId == tu.UserId);
                return invite.InviteCode;
            }
        }

        private string HashPassword(string password, out string outSalt)
        {
            if (password == null) throw new ArgumentNullException("password");

            int saltSize = 16;
            int iterations = 4000;

            byte[] salt;
            byte[] bytes;

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltSize, iterations))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }
            outSalt = salt.ToString();
            byte[] inArray = new byte[saltSize + 32];
            Buffer.BlockCopy((Array) salt, 0, (Array) inArray, 0, saltSize);
            Buffer.BlockCopy((Array) bytes, 0, (Array) inArray, saltSize, 32);
            return Convert.ToBase64String(inArray);
        }

        private string InviteCodeGenerator()
        {
            string[] str = new[] {"mb", "mi", "mr", "ba", "mm", "ma", "mb", "mib"};
            using (var dataModel = new Mibarim_plusEntities())
            {
                var invitecode = dataModel.Invites.Count();
                var indict = invitecode%7;
                var number = invitecode/7;

                return str[indict] + number;
            }
            /*var random = new Random();
            string[] str = new[] {"mb", "mi", "mr", "ba", "mm"};
            var rndMember = str[random.Next(str.Length)];
            return rndMember + inviteId;*/
        }


        public ScoreModel GetScores(UserModel user)
        {
            throw new NotImplementedException();
        }

        public TripScoreModel GetTripScores(UserModel user)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var score = new TripScoreModel();
                var preMonth = DateTime.Now.AddMonths(-1);
                var trips =
                    dataModel.vwDriverTrips.Where(x => x.UserId == user.UserId && x.TCreateTime > preMonth).ToList();
                if (trips.Count > 0)
                {
                    var gdate = trips.GroupBy(x => x.TCreateTime.Date).Count();
                    var firstDateOfMonth = trips.OrderByDescending(x => x.TCreateTime).FirstOrDefault().TCreateTime;
                    score.FirstDate = firstDateOfMonth.ToShamsiDateYMD();
                    score.DaysOn = gdate;
                    score.DaysToWin = 22 - gdate;
                }
                return score;
            }
        }

        public GasScore GetGasScores(UserModel user)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var score = new GasScore();
                var begining = DateTime.Parse("2017-08-30");
                var trips =
                    dataModel.vwDriverTrips.Where(
                        x => x.UserId == user.UserId && x.TCreateTime > begining && x.TState != 5).ToList();
                if (trips.Count > 0)
                {
                    foreach (var vwDriverTrip in trips)
                    {
                        long minDistance;
                        var stationRoute =
                            dataModel.StationRoutes.FirstOrDefault(x => x.StationRouteId == vwDriverTrip.StationRouteId);
                        if (stationRoute.DistanceMin != null)
                        {
                            minDistance = (long) stationRoute.DistanceMin;
                            score.DistanceRouted += (long) (minDistance*0.001);
                            score.Payment += (long) (minDistance*0.1);
                        }
                    }
                }
                return score;
            }
        }

        public List<RouteTimes> GetRouteTimes(UserModel user, string text)
        {
            var res = new List<RouteTimes>();
            var current = DateTime.Now;
            using (var dataModel = new Mibarim_plusEntities())
            {
                var stationRouteId = GetStationRouteId(text);
                foreach (
                    var line in
                    dataModel.PassengerRoutes.Where(
                            x => x.StationRouteId == stationRouteId && x.PassTime > current && x.IsDeleted == false)
                        .OrderBy(x => x.PassTime)
                        .GroupBy(info => new {info.PassTime.Hour, info.PassTime.Minute})
                        .Select(group => new
                        {
                            Metric = group.Key,
                            Count = group.Count()
                        }).Take(10))
                {
                    res.Add(new RouteTimes()
                    {
                        Time = GetNextDateTime(line.Metric.Hour, line.Metric.Minute),
                        Count = line.Count
                    });
                }
            }
            return res;
        }

        public List<RouteTimes> GetRouteTimes(UserModel user, int stationRouteId)
        {
            var res = new List<RouteTimes>();
            var current = DateTime.Now;
            using (var dataModel = new Mibarim_plusEntities())
            {
                foreach (
                    var line in
                    dataModel.PassengerRoutes.Where(
                            x => x.StationRouteId == stationRouteId && x.PassTime > current && x.IsDeleted == false)
                        .OrderBy(x => x.PassTime)
                        .GroupBy(info => new {info.PassTime.Hour, info.PassTime.Minute})
                        .Select(group => new
                        {
                            Metric = group.Key,
                            Count = group.Count()
                        }).Take(10))
                {
                    res.Add(new RouteTimes()
                    {
                        Time = GetNextDateTime(line.Metric.Hour, line.Metric.Minute),
                        Count = line.Count
                    });
                }
            }
            return res;
        }

        public List<RouteTimes> GetRouteTimesByStId(long stationRouteId)
        {
            var res = new List<RouteTimes>();
            using (var dataModel = new Mibarim_plusEntities())
            {
                foreach (
                    var line in
                    dataModel.PassengerRoutes.Where(
                            x => x.StationRouteId == stationRouteId && x.PassTime > DateTime.Now && x.IsDeleted == false)
                        .OrderBy(x => x.PassTime)
                        .GroupBy(info => new {info.PassTime.Hour, info.PassTime.Minute})
                        .Select(group => new
                        {
                            Metric = group.Key,
                            Count = group.Count()
                        }).Take(10))
                {
                    res.Add(new RouteTimes()
                    {
                        Time = GetNextDateTime(line.Metric.Hour, line.Metric.Minute),
                        Count = line.Count
                    });
                }
            }
            return res;
        }

        public List<RouteNotif> GetNotifState(UserModel user)
        {
            var res = new List<RouteNotif>();
            using (var dataModel = new Mibarim_plusEntities())
            {
                var notifs = dataModel.PassNotifs.Where(x => x.UserId == user.TuserId);
                for (int i = 1; i <= 6; i++)
                {
                    var noti = new RouteNotif();
                    var theNotif = notifs.FirstOrDefault(x => x.StationRouteId == i);
                    noti.StationRouteId = i;
                    var stationName = GetStationName(i);
                    if (theNotif != null)
                    {
                        if (theNotif.NotifStatus == (int) NotifStatus.On)
                        {
                            noti.RouteText = stationName + " روشن ";
                        }
                        else
                        {
                            noti.RouteText = stationName + " خاموش";
                        }
                        noti.Status = theNotif.NotifStatus;
                    }
                    else
                    {
                        noti.RouteText = stationName + " خاموش";
                        noti.Status = (int) NotifStatus.Off;
                    }
                    res.Add(noti);
                }
            }
            return res;
        }

        public bool SetRouteTime(UserModel user, string text)
        {
            var res = "";
            using (var dataModel = new Mibarim_plusEntities())
            {
                var lastMsg =
                    dataModel.TelegramMsgs.OrderByDescending(x => x.TmsgCreateTime)
                        .FirstOrDefault(x => x.TuserId == user.TuserId && x.TmsgType == (int) KntuMessageType.Knturoute);
                var stationRouteId = GetStationRouteId(lastMsg.TmsgMessage);
                var existroute =
                    dataModel.PassengerRoutes.FirstOrDefault(
                        x =>
                            x.UserId == user.TuserId && x.PassTime > DateTime.Now &&
                            x.StationRouteId == stationRouteId && x.IsDeleted == false);
                if (existroute != null)
                {
                    existroute.IsDeleted = true;
                    dataModel.SaveChanges();
                }

                var time = TimeSpan.Parse(text);
                var dateTime = DateTime.Today.Add(time);
                var rtime = GetNextDateTime(dateTime.Hour, dateTime.Minute);
                var newpassroute = new PassengerRoute();
                newpassroute.UserId = user.TuserId;
                newpassroute.CreateTime = DateTime.Now;
                newpassroute.IsDeleted = false;
                newpassroute.PassTime = rtime;
                newpassroute.NotifSeq = (int) NotifSeq.NotNotifed;
                /*if (rtime > DateTime.Now &&
                    rtime < DateTime.Now.AddHours(1))
                {
                    newpassroute.NotifSeq = (int)NotifSeq.OneNotifed;
                }*/
                newpassroute.StationRouteId = stationRouteId;
                dataModel.PassengerRoutes.Add(newpassroute);
                dataModel.SaveChanges();
            }
            return true;
        }

        public string ToggleNotifSetting(UserModel user, string text)
        {
            long stId = long.Parse(text);
            string res = GetStationName(stId);
            using (var dataModel = new Mibarim_plusEntities())
            {
                var notif =
                    dataModel.PassNotifs.FirstOrDefault(x => x.UserId == user.TuserId && x.StationRouteId == stId);
                if (notif != null)
                {
                    res += notif.NotifStatus == (int) NotifStatus.On ? " خاموش " : " روشن ";
                    notif.NotifStatus = notif.NotifStatus == (int) NotifStatus.On
                        ? (int) NotifStatus.Off
                        : (int) NotifStatus.On;
                }
                else
                {
                    var passNotif = new PassNotif();
                    passNotif.UserId = user.TuserId;
                    passNotif.StationRouteId = stId;
                    passNotif.CreateTime = DateTime.Now;
                    passNotif.NotifStatus = (int) NotifStatus.On;
                    dataModel.PassNotifs.Add(passNotif);
                    res += " روشن ";
                }
                dataModel.SaveChanges();
            }
            return res;
        }

        public void SetNotifSettingOn(UserModel user, string text)
        {
            var stationRouteId = GetStationRouteId(text);
            using (var dataModel = new Mibarim_plusEntities())
            {
                var notif =
                    dataModel.PassNotifs.FirstOrDefault(
                        x => x.UserId == user.TuserId && x.StationRouteId == stationRouteId);
                if (notif == null)
                {
                    var passNotif = new PassNotif();
                    passNotif.UserId = user.TuserId;
                    passNotif.StationRouteId = stationRouteId;
                    passNotif.CreateTime = DateTime.Now;
                    passNotif.NotifStatus = (int) NotifStatus.On;
                    dataModel.PassNotifs.Add(passNotif);
                    dataModel.SaveChanges();
                }
            }
        }

        public void SetInviteCode(UserModel usermodel, string text)
        {
            var res = new UserModel();
            using (var dataModel = new Mibarim_plusEntities())
            {
                var user = dataModel.TelegramUsers.FirstOrDefault(x => x.TuserId == usermodel.TuserId);
                if (user != null)
                {
                    user.TinviteCode = text;
                    dataModel.SaveChanges();
                }
            }
        }

        public string CancelRouteTime(UserModel user)
        {
            var res = "";
            using (var dataModel = new Mibarim_plusEntities())
            {
                var lastMsg =
                    dataModel.TelegramMsgs.OrderByDescending(x => x.TmsgCreateTime)
                        .FirstOrDefault(x => x.TuserId == user.TuserId && x.TmsgType == (int) KntuMessageType.Knturoute);
                var stationRouteId = GetStationRouteId(lastMsg.TmsgMessage);
                var existroute =
                    dataModel.PassengerRoutes.FirstOrDefault(
                        x =>
                            x.UserId == user.TuserId && x.PassTime > DateTime.Now &&
                            x.StationRouteId == stationRouteId && x.IsDeleted == false);
                if (existroute != null)
                {
                    existroute.IsDeleted = true;
                    dataModel.SaveChanges();
                    res = lastMsg.TmsgMessage;
                }
            }
            return res;
        }


        public RouteNotif GetNotifRoute()
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var current = DateTime.Now;
                //var nexthour = DateTime.Now.AddHours(1);
                var nexthalfhour = DateTime.Now.AddMinutes(30);
                var readytoNotif =
                    dataModel.PassengerRoutes.Where(
                        x =>
                            x.IsDeleted == false && x.PassTime > current &&
                            x.PassTime < nexthalfhour && x.NotifSeq == (int) NotifSeq.NotNotifed).ToList();
                RouteNotif res = null;
                if (readytoNotif.Count > 0)
                {
                    foreach (var passengerRoute in readytoNotif)
                    {
                        if (res == null)
                        {
                            res = new RouteNotif();
                            res.StationRouteId = passengerRoute.StationRouteId;
                        }
                        if (res!=null && res.StationRouteId == passengerRoute.StationRouteId)
                        {
                            passengerRoute.NotifSeq = (int) NotifSeq.OneNotifed;
                            dataModel.SaveChanges();
                        }
                    }
                    return res;
                }
                /*var readyto2Notif =
                    dataModel.PassengerRoutes.FirstOrDefault(
                        x =>
                            x.IsDeleted == false && x.PassTime > current &&
                            x.PassTime < nexthalfhour && x.NotifSeq == (int)NotifSeq.OneNotifed);
                if (readyto2Notif != null)
                {
                    var res = new RouteNotif();
                    res.StationRouteId = readyto2Notif.StationRouteId;
                    readyto2Notif.NotifSeq = (int)NotifSeq.TwoNotifed;
                    dataModel.SaveChanges();
                    return res;
                }*/
            }
            return null;
        }

        public List<ChatMessage> GetUsersInfos(long stationRouteId)
        {
            var res = new List<ChatMessage>();
            using (var dataModel = new Mibarim_plusEntities())
            {
                var routeusers =
                    dataModel.PassengerRoutes.Where(
                        x => x.StationRouteId == stationRouteId && x.PassTime > DateTime.Now && x.IsDeleted == false);
                foreach (var usr in routeusers)
                {
                    var userId = usr.UserId;
                    var um = new ChatMessage();
                    var chat = dataModel.TelegramMsgs.FirstOrDefault(x => x.TuserId == userId);
                    var user = dataModel.TelegramUsers.FirstOrDefault(x => x.TuserId == userId);
                    um.ChatId = (long) chat.ChatId;
                    um.PhoneNumber = user.PhoneNumber;
                    um.Time = usr.PassTime.ToString("HH:mm");
                    res.Add(um);
                }
            }
            return res;
        }
        public List<ChatMessage> GetAllChatIds()
        {
            var res = new List<ChatMessage>();
            using (var dataModel = new Mibarim_plusEntities())
            {
                var routeusers =
                    dataModel.TelegramUsers.ToList();
                foreach (var usr in routeusers)
                {
                    var um = new ChatMessage();
                    var chat = dataModel.TelegramMsgs.FirstOrDefault(x => x.TuserId == usr.TuserId);
                    um.ChatId = (long)chat.ChatId;
                    um.PhoneNumber = usr.PhoneNumber;
                    res.Add(um);
                }
            }
            return res;
        }

        public List<long> GetNotifiChatIds(long notifRouteStationRouteId)
        {
            var res = new List<long>();
            using (var dataModel = new Mibarim_plusEntities())
            {
                var users =
                    dataModel.PassNotifs.Where(
                            x => x.StationRouteId == notifRouteStationRouteId && x.NotifStatus == (int) NotifStatus.On)
                        .Select(y => y.UserId);
                foreach (var user in users)
                {
                    var chat =
                        dataModel.TelegramMsgs.Where(x => x.TuserId == user)
                            .OrderByDescending(y => y.TmsgCreateTime)
                            .FirstOrDefault();
                    res.Add((long) chat.ChatId);
                }
            }
            return res;
        }

        private DateTime GetNextDateTime(int hour, int min)
        {
            DateTime current = DateTime.Now;
            DateTime nextDatetime = DateTime.Now;
            nextDatetime = new DateTime(current.Year, current.Month, current.Day, hour, min, 0);
            if (nextDatetime < current)
            {
                nextDatetime = new DateTime(current.Year, current.Month, current.AddDays(1).Day, hour, min, 0);
            }
            if (nextDatetime < current)
            {
                nextDatetime = new DateTime(current.Year, current.AddMonths(1).Month, current.AddDays(1).Day, hour, min,
                    0);
            }
            return nextDatetime;
        }

        private int GetStationRouteId(string text)
        {
            var stationRouteId = 0;
            if (text == "از خوابگاه به ونک")
            {
                stationRouteId = 1;
            }
            else if (text == "از ونک به خوابگاه")
            {
                stationRouteId = 2;
            }
            else if (text == "از دانشکده علوم به خوابگاه")
            {
                stationRouteId = 3;
            }
            else if (text == "از خوابگاه به دانشکده علوم")
            {
                stationRouteId = 4;
            }
            else if (text.Contains("از خوابگاه به س"))
            {
                stationRouteId = 5;
            }
            else if (text.Contains("دخندان به خوابگاه"))
            {
                stationRouteId = 6;
            }
            return stationRouteId;
        }

        public string GetStationName(long stationId)
        {
            string stationName = "";
            switch (stationId)
            {
                case 1:
                    stationName = "از خوابگاه به ونک";
                    break;
                case 2:
                    stationName = "از ونک به خوابگاه";
                    break;
                case 3:
                    stationName = "از دانشکده علوم به خوابگاه";
                    break;
                case 4:
                    stationName = "از خوابگاه به دانشکده علوم";
                    break;
                case 5:
                    stationName = "از خوابگاه به سیدخندان";
                    break;
                case 6:
                    stationName = "از سیدخندان به خوابگاه";
                    break;
            }
            return stationName;
        }


    }
}