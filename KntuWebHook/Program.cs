using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BotManager.KntuBotManager;
using BotManager.Models;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace KntuWebHook
{
    static class Bot
    {
        public static readonly TelegramBotClient Api =
            new TelegramBotClient("440077947:AAFJjw_EaaMAEh648W3tVJI7r1Vfm5-CECE");
    }

    static class Program
    {
        static void Main(string[] args)
        {
            // Endpoint musst be configured with netsh:
            // netsh http add urlacl url=https://+:8443/ user=<username>
            // netsh http add sslcert ipport=0.0.0.0:8443 certhash=<cert thumbprint> appid=<random guid>

            using (WebApp.Start<Startup>("https://+:443"))
            {
                // Register WebHook
                //Bot.Api.SetWebhookAsync("https://YourHostname:8443/WebHook").Wait();

                Console.WriteLine("Server Started");

                // Stop Server after <Enter>
                Console.ReadLine();

                // Unregister WebHook
                //Bot.Api.SetWebhookAsync().Wait();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();

            configuration.Routes.MapHttpRoute("WebHook", "{controller}");

            app.UseWebApi(configuration);
        }
    }

    public class WebHookController : ApiController
    {
        public async Task<IHttpActionResult> Post(Update update)
        {
            try
            {
                //Console.WriteLine("omad");
                //    var data = await request.Content.ReadAsStringAsync();
                //    Console.WriteLine(data);
                //Console.WriteLine(JsonConvert.SerializeObject(data));
                //Console.WriteLine(JsonConvert.SerializeObject(update.CallbackQuery));
                //NetTelegramBotApi.Types.Update update=null;

                var bot = Bot.Api;
                long tuserId = 0;
                long offset = 0;
                var botManager = new KntuBotManager();


                if (update == null)
                {
                    return Ok();
                }
                string phoneNumber = null;
                offset = update.Id + 1;
                if (update.Message == null && update.CallbackQuery == null)
                {
                    return Ok();
                }
                User from;
                string text = "";
                long chatId;
                long messageId;
                DateTimeOffset date;

                if (update.Message != null)
                {
                    from = update.Message.From;
                    text = update.Message.Text;
                    chatId = update.Message.Chat.Id;
                    messageId = update.Message.MessageId;
                    date = update.Message.Date;
                    if (update.Message.Contact != null)
                    {
                        tuserId = update.Message.Contact.UserId;
                        phoneNumber = update.Message.Contact.PhoneNumber;
                    }
                }
                else
                {
                    from = update.CallbackQuery.From;
                    text = update.CallbackQuery.Data;
                    chatId = update.CallbackQuery.Message.Chat.Id;
                    messageId = update.CallbackQuery.Message.MessageId;
                    date = update.CallbackQuery.Message.Date;
                    if (update.CallbackQuery.Message.Contact != null)
                    {
                        tuserId = update.Message.Contact.UserId;
                        phoneNumber = update.Message.Contact.PhoneNumber;
                    }
                }
                var user = new UserModel();
                try
                {
                    user = botManager.GetUser(from.Id, from.Username, from.FirstName, from.LastName, phoneNumber);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(
                    "Msg from {0} {1} ({2}) at {4}: {3}",
                    from.FirstName,
                    from.LastName,
                    from.Username,
                    text,
                    date);
                // منوی اصلی
                var mainMenu = new ReplyKeyboardMarkup()
                {
                    Keyboard = new[]
                    {
                        new[] {new KeyboardButton("از ونک به خوابگاه"), new KeyboardButton("از خوابگاه به ونک")},
                        new[]
                        {
                            new KeyboardButton("از دانشکده علوم به خوابگاه"),
                            new KeyboardButton("از خوابگاه به دانشکده علوم")
                        },
                        new[]
                        {
                            new KeyboardButton("از سیدخندان به خوابگاه"),
                            new KeyboardButton("از خوابگاه به سیدخندان")
                        },
                        new[] {new KeyboardButton("تنظیمات اعلان‌ها"), new KeyboardButton("تماس با پشتیبانی")},
                    },
                    OneTimeKeyboard = true,
                    ResizeKeyboard = true
                };
                /*var mainAction = bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0,
                    mainMenu);*/
                //منوی زمان مسیر
                var setRouteMenu = new ReplyKeyboardMarkup()
                {
                    Keyboard = new[]
                    {
                        new[] {new KeyboardButton("لغو درخواست"), new KeyboardButton("تماس با پشتیبانی")},
                        new[] {new KeyboardButton("انتخاب مسیر"), new KeyboardButton("تنظیمات اعلان‌ها")},
                    },
                    OneTimeKeyboard = true,
                    ResizeKeyboard = true
                };
                var setRouteMenuByPhone = new ReplyKeyboardMarkup()
                {
                    Keyboard = new[]
                    {
                        new[] {new KeyboardButton("لغو درخواست"), new KeyboardButton("تماس با پشتیبانی")},
                        new[] {new KeyboardButton("انتخاب مسیر"), new KeyboardButton("تنظیمات اعلان‌ها")},
                        new[] {new KeyboardButton("ارسال شماره تلفن همراه") {RequestContact = true}},
                    },
                    OneTimeKeyboard = true,
                    ResizeKeyboard = true
                };
                if (text == "/start")
                {
                    if (user.UserId != null && user.UserId != 0)
                    {
                        bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0,
                            mainMenu).Wait();
                    }
                    else
                    {
                        botManager.SetMessage(user, text, chatId, (int) KntuMessageType.Start);
                        var keyb = new ReplyKeyboardMarkup()
                        {
                            Keyboard = new[]
                            {
                                new[] {new KeyboardButton("کد معرفی ندارم")},
                            },
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };
                        bot.SendTextMessageAsync(chatId, "کد معرفی‌ات رو وارد کن", ParseMode.Default, false, false, 0,
                            keyb).Wait();
                    }
                }
                else if (text == "کد معرفی ندارم")
                {
                    bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0, mainMenu)
                        .Wait();
                    botManager.SetMessage(user, text, chatId, (int) KntuMessageType.NoInvite);
                }
                else if (text == "انتخاب مسیر")
                {
                    botManager.SetMessage(user, text, chatId, (int) KntuMessageType.SelectRoute);
                    bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0, mainMenu)
                        .Wait();
                    //bot.MakeRequestAsync(mainAction).Wait();
                }
                else if (text == "از خوابگاه به ونک" || text == "از ونک به خوابگاه" ||
                         text == "از دانشکده علوم به خوابگاه" || text == "از خوابگاه به دانشکده علوم" ||
                         text.Contains("خندان به خوابگاه") || text.Contains("از خوابگاه به س"))
                {
                    botManager.SetMessage(user, text, chatId, (int) KntuMessageType.Knturoute);
                    var getTimes = botManager.GetRouteTimes(user, text);
                    botManager.SetNotifSettingOn(user, text);
                    var items = new Dictionary<string, string>();
                    foreach (var routeTimese in getTimes)
                    {
                        items.Add(routeTimese.Count + " نفر ساعت " + routeTimese.Time.ToString("HH:mm"),
                            routeTimese.Time.ToString("HH:mm"));
                    }
                    var inlineKeyboardMarkup = InlineKeyboardMarkupMaker(items);
                    var timemsg = string.Format(
                        messages.ResourceManager.GetString("TimeInfo"), text);
                    bot.SendTextMessageAsync(chatId, timemsg, ParseMode.Default, false, false, 0, inlineKeyboardMarkup)
                        .Wait();
                }
                else if (text == "تماس با پشتیبانی")
                {
                    bot.SendTextMessageAsync(chatId, messages.ResourceManager.GetString("ContactSupport"),
                        ParseMode.Default, false, false, 0, mainMenu).Wait();
                }
                else if (text == "لغو درخواست")
                {
                    var route = botManager.CancelRouteTime(user);
                    var str = string.Format(
                        messages.ResourceManager.GetString("RequestCanceled"), route);

                    bot.SendTextMessageAsync(chatId, str, ParseMode.Default, false, false, 0, mainMenu).Wait();
                }
                else if (text == "تنظیمات اعلان‌ها")
                {
                    botManager.SetMessage(user, text, chatId, (int) KntuMessageType.NotifSetting);
                    var notifs = botManager.GetNotifState(user);
                    var notifitems = new Dictionary<string, string>();
                    foreach (var routeNotif in notifs)
                    {
                        notifitems.Add(routeNotif.RouteText,
                            routeNotif.StationRouteId.ToString());
                    }
                    var notifinlineKeyboardMarkup = InlineKeyboardMarkupMaker(notifitems);
                    bot.SendTextMessageAsync(chatId, messages.ResourceManager.GetString("NotifSetting"),
                        ParseMode.Default, false, false, 0, notifinlineKeyboardMarkup).Wait();
                }
                else if (text == "110110")
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        var stName = botManager.GetStationName(i);
                        var getTimes = botManager.GetRouteTimes(user, i);
                        var items = new Dictionary<string, string>();
                        foreach (var routeTimese in getTimes)
                        {
                            items.Add(routeTimese.Count + " نفر ساعت " + routeTimese.Time.ToString("HH:mm"),
                                routeTimese.Time.ToString("HH:mm"));
                        }
                        var inlineKeyboardMarkup = InlineKeyboardMarkupMaker(items);
                        bot.SendTextMessageAsync(chatId, stName, ParseMode.Default, false, false, 0,
                            inlineKeyboardMarkup).Wait();
                        /*bot.SendTextMessageAsync(chatId, "تنظیمات اعلان ها برای هر مسیر",
                                        ParseMode.Default, false, false, 0, setRouteMenu).Wait();*/
                        var userInfos = botManager.GetUsersInfos(i);
                        foreach (var userModel in userInfos)
                        {
                            bot.SendTextMessageAsync(chatId,
                                userModel.Time + "=>" + userModel.PhoneNumber + " - " + userModel.ChatId).Wait();
                        }
                    }
                }
                else if (text != null)
                {
                    var message = botManager.GetLastMessage(user);
                    switch (message.MsgType)
                    {
                        case KntuMessageType.Start:
                            botManager.SetInviteCode(user, text);
                            bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0,
                                mainMenu).Wait();
                            break;
                        case KntuMessageType.NotifSetting:
                            //Console.WriteLine(update.CallbackQuery.InlineMessageId);
                            var msg = botManager.ToggleNotifSetting(user, text);
                            var notifs = botManager.GetNotifState(user);
                            var notifitems = new Dictionary<string, string>();
                            foreach (var routeNotif in notifs)
                            {
                                notifitems.Add(routeNotif.RouteText,
                                    routeNotif.StationRouteId.ToString());
                            }
                            var notifinlineKeyboardMarkup = InlineKeyboardMarkupMaker(notifitems);
                            bot.EditMessageReplyMarkupAsync(chatId, (int) messageId, notifinlineKeyboardMarkup).Wait();
                            break;
                        case KntuMessageType.Knturoute:
                        case KntuMessageType.TimeSet:
                        case KntuMessageType.NoInvite:
                            Regex regex = new Regex(@"^([0-9]|0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$");
                            Match match = regex.Match(text);
                            if (match.Success)
                            {
                                var res = botManager.SetRouteTime(user, text);
                                botManager.SetMessage(user, text, chatId, (int) KntuMessageType.TimeSet);
                                if (string.IsNullOrEmpty(user.PhoneNumber))
                                {
                                    bot.SendTextMessageAsync(chatId,
                                        messages.ResourceManager.GetString("TimeSetWithPhone"), ParseMode.Default, false,
                                        false, 0, setRouteMenuByPhone).Wait();
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, messages.ResourceManager.GetString("TimeSet"),
                                        ParseMode.Default, false, false, 0, setRouteMenu).Wait();
                                }
                            }
                            else
                            {
                                var getTimes = botManager.GetRouteTimes(user, text);
                                var items = new Dictionary<string, string>();
                                foreach (var routeTimese in getTimes)
                                {
                                    items.Add(
                                        routeTimese.Count + " نفر ساعت " + routeTimese.Time.ToString("HH:mm"),
                                        routeTimese.Time.ToString("HH:mm"));
                                }
                                var inlineKeyboardMarkup = InlineKeyboardMarkupMaker(items);
                                bot.SendTextMessageAsync(chatId, "فرمت ساعت وارد شده صحیح نیست", ParseMode.Default,
                                    false, false, 0, mainMenu).Wait();
                            }
                            break;
                        case KntuMessageType.BotRegistered:
                        case KntuMessageType.UnknownMessage:
                            botManager.SetMessage(user, text, chatId, (int) KntuMessageType.UnknownMessage);

                            bot.SendTextMessageAsync(chatId, "مسیرتو انتخاب کن", ParseMode.Default, false, false, 0,
                                mainMenu).Wait();

                            //bot.MakeRequestAsync(mainAction).Wait();
                            break;
                    }
                }
                botManager.SetMessage(user, text, chatId, (int) KntuMessageType.Historical);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.Message + " - " + e.InnerException.Message);
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
            }
            return Ok();
        }

        public static InlineKeyboardMarkup InlineKeyboardMarkupMaker(Dictionary<string, string> items)
        {
            InlineKeyboardButton[][] ik = items.Select(item => new[]
            {
                new InlineKeyboardCallbackButton(item.Key, item.Value)
            }).ToArray();
            return new InlineKeyboardMarkup() {InlineKeyboard = ik};
        }

        public class WebHookAdminController : ApiController
        {
            [HttpGet]
            public async Task<IHttpActionResult> Post([FromUri] ChatMessage model)
            {
                try
                {
                    var bot = Bot.Api;
                    Console.WriteLine("message=>" + model.ChatId + " - " + model.Msg);
                    bot.SendTextMessageAsync(model.ChatId, model.Msg).Wait();
                }
                catch (Exception ee)
                {
                    if (ee.InnerException != null)
                    {
                        Console.WriteLine(ee.Message + " - " + ee.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ee.Message);
                    }
                }
                return Ok();
            }
        }

        /*public class WebHookSendToallController : ApiController
        {
            [HttpGet]
            public async Task<IHttpActionResult> Post([FromUri] ChatMessage model)
            {
                try
                {
                    var bot = Bot.Api;
                    if (model.ChatId == 123123123)
                    {
                        var botManager = new KntuBotManager();
                        //منوی زمان مسیر
                        var setRouteMenu = new ReplyKeyboardMarkup()
                        {
                            Keyboard = new[]
                            {
                                new[] {new KeyboardButton("انتخاب مسیر"), new KeyboardButton("تنظیمات اعلان‌ها")},
                            },
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };
                        var setRouteMenuByPhone = new ReplyKeyboardMarkup()
                        {
                            Keyboard = new[]
                            {
                                new[] {new KeyboardButton("انتخاب مسیر"), new KeyboardButton("تنظیمات اعلان‌ها")},
                                new[] {new KeyboardButton("ارسال شماره تلفن همراه") {RequestContact = true}},
                            },
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        var chats = botManager.GetAllChatIds();
                        foreach (var chat in chats)
                        {
                            try
                            {
                                Thread.Sleep(1000);
                                Console.WriteLine("message=>" + chat.ChatId + " - " + model.Msg);
                                if (string.IsNullOrEmpty(chat.PhoneNumber))
                                {
                                    bot.SendTextMessageAsync(chat.ChatId, model.Msg, ParseMode.Default, false,
                                        false, 0, setRouteMenuByPhone).Wait();
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chat.ChatId, model.Msg,
                                        ParseMode.Default, false, false, 0, setRouteMenu).Wait();
                                }
                            }
                            catch (Exception ie)
                            {
                                Console.WriteLine(chat.ChatId + " " + ie.Message);
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    if (ee.InnerException != null)
                    {
                        Console.WriteLine(ee.Message + " - " + ee.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ee.Message);
                    }
                }
                return Ok();
            }
        }*/

        public class NotifSenderController : ApiController
        {
            [HttpGet]
            public async Task<IHttpActionResult> Post()
            {
                try
                {
                    Console.WriteLine("omadNotif");
                    var bot = Bot.Api;
                    var botManager = new KntuBotManager();
                    var notifRoute = botManager.GetNotifRoute();
                    if (notifRoute != null)
                    {
                        //منوی زمان مسیر
                        var setRouteMenu = new ReplyKeyboardMarkup()
                        {
                            Keyboard = new[]
                            {
                                new[] { new KeyboardButton("تنظیمات اعلان‌ها") },
                                new[] {new KeyboardButton("انتخاب مسیر"), new KeyboardButton("تماس با پشتیبانی")},
                            },
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        Console.WriteLine(notifRoute.StationRouteId);
                        var times = botManager.GetRouteTimesByStId(notifRoute.StationRouteId);
                        var routeName = botManager.GetStationName(notifRoute.StationRouteId);
                        var items = new Dictionary<string, string>();
                        foreach (var routeTimese in times)
                        {
                            items.Add(routeTimese.Count + " نفر ساعت " + routeTimese.Time.ToString("HH:mm"),
                                routeTimese.Time.ToString("HH:mm"));
                        }
                        var inlineKeyboardMarkup = InlineKeyboardMarkupMaker(items);
                        var timemsg = string.Format(
                            messages.ResourceManager.GetString("NotifString"), routeName);
                        Console.WriteLine(timemsg);
                        var chatIds = botManager.GetNotifiChatIds(notifRoute.StationRouteId);
                        Console.WriteLine(chatIds.Count);
                        foreach (var userchatId in chatIds)
                        {
                            try
                            {
                                bot.SendTextMessageAsync(userchatId, timemsg, ParseMode.Default, false, false, 0,
                                    inlineKeyboardMarkup).Wait();
                                bot.SendTextMessageAsync(userchatId, "👇👇👇👇تنظیمات اعلان ها برای مسیر",
                                        ParseMode.Default, false, false, 0, setRouteMenu).Wait();
                            }
                            catch (Exception ie)
                            {
                                Console.WriteLine(userchatId + " " + ie.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.Message + " - " + e.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                return Ok();
            }
        }
    }
}