using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BotManager.Models;
using CoreManager.Helper;

namespace BotManager.CityHeroBotManager
{
    public class CityHeroManager
    {
        public UserModel GetUser(long tuserId, string fromUsername, string fromFirstName, string fromLastName)
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
                    dataModel.TelegramUsers.Add(tuser);
                    dataModel.SaveChanges();
                    user = tuser;
                }
                res.TuserId = user.TuserId;
                res.UserName = user.TmsgUserName;
                res.Name = user.TmsgName;
                res.TelegramUserId = tuserId;
                res.Family = user.TmsgFamily;
                res.CreateTime = user.TcreateTime;
                res.UserId = user.UserId == null ? 0 : (long) user.UserId;
                return res;
            }
        }

        public void SetMessage(UserModel user, string text, int type)
        {
            using (var dataModel = new Mibarim_plusEntities())
            {
                var tmsg = new TelegramMsg();
                tmsg.TuserId = user.TuserId;
                tmsg.TmsgCreateTime = DateTime.Now;
                tmsg.TmsgMessage = text;
                tmsg.TmsgType = type;
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
                        .FirstOrDefault(x => x.TuserId == user.TuserId && x.TmsgType != (int)KntuMessageType.Historical);
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
                var score=new TripScoreModel();
                var preMonth = DateTime.Now.AddMonths(-1);
                var trips = dataModel.vwDriverTrips.Where(x => x.UserId == user.UserId && x.TCreateTime> preMonth).ToList();
                if (trips.Count>0)
                {
                    var gdate = trips.GroupBy(x => x.TCreateTime.Date).Count();
                    var firstDateOfMonth = trips.OrderByDescending(x => x.TCreateTime).FirstOrDefault().TCreateTime;
                    score.FirstDate = firstDateOfMonth.ToShamsiDateYMD();
                    score.DaysOn = gdate;
                    score.DaysToWin = 22- gdate;
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
                var trips = dataModel.vwDriverTrips.Where(x => x.UserId == user.UserId && x.TCreateTime > begining && x.TState!=5).ToList();
                if (trips.Count > 0)
                {
                    foreach (var vwDriverTrip in trips)
                    {
                        long minDistance;
                        var stationRoute =
                            dataModel.StationRoutes.FirstOrDefault(x => x.StationRouteId == vwDriverTrip.StationRouteId);
                        if (stationRoute.DistanceMin != null)
                        {
                            minDistance = (long)stationRoute.DistanceMin;
                            score.DistanceRouted += (long)(minDistance * 0.001);
                            score.Payment += (long)(minDistance*0.1);
                        }
                        
                    }
                }
                return score;
            }
        }
    }
}