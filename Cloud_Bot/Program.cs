using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;

namespace Cloud_Bot
{
    partial class Program
    {
        private static string Token  { get; set; } = "5172740511:AAGv-ODZvEpI52eC-0Y58d16smkOOYuvRnE";        

        static long chatId = 623803677;
        static string messageText; 
        static int messageId_last;
        static PhotoSize chatPhoto;
        static bool recordingNotes = false;
        static string alarmTime = null;
        static int ActiveTask = 0; 

        static bool hide_ON = false;


        static async Task Main()
        {
            List<string> Set = new();
            List<int> MessagesId = new List<int>();



            var botClient = new TelegramBotClient(Token);
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");

            //IAsyncEnumerator<int> check =  


            //Console.ReadLine();
            Pause();

            cts.Cancel();

            void Pause()
            {
                Thread.Sleep(Timeout.Infinite);
            }

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationtoken)
            {
                if (update.Type != UpdateType.Message)
                    return;

                if (update.Message!.Type != MessageType.Text && update.Message!.Type != MessageType.Photo) //&& ActiveTask == (int)activeTask.def)
                    return;

                ReplyKeyboardMarkup replyKeyboardMarkup_HEY = new(new[]                {

                    new KeyboardButton[] { "👋" },

                })
                {
                    ResizeKeyboard = true
                };

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    //new KeyboardButton[] { "Notes📝", "Requests", "Settings" },
                    new KeyboardButton[] { "📝", "requests", "settings" },
                    new KeyboardButton[] { "⬇" },

                })
                {
                    ResizeKeyboard = true
                };

                ReplyKeyboardMarkup replyKeyboardMarkup_notes = new(new[]
                {
                    new KeyboardButton[] { "#1", "⏰", "#3", "#4", "#5", "#6" },
                    new KeyboardButton[] { "⬇" },

                })
                {
                    ResizeKeyboard = true
                };

                ReplyKeyboardMarkup replyKeyboardMarkup_zaprosi = new(new[]
                {
                    new KeyboardButton[] { "Show buffer contents", "Clear buffer", "💲" },
                    new KeyboardButton[] { "⬇" },

                })
                {
                    ResizeKeyboard = true
                };

                ReplyKeyboardMarkup replyKeyboardMarkup_HEY_HIDE = new(new[]
                {
                    new KeyboardButton[] { "👋" },
                })
                {
                    ResizeKeyboard = true
                };

                ReplyKeyboardMarkup replyKeyboardMarkup_Alarm_Time = new(new[]
                {
                    new KeyboardButton[] { "set ⏰", "❌" },
                    new KeyboardButton[] { "⬇" },
                })
                {
                    ResizeKeyboard = true
                };



                chatId = update.Message.Chat.Id;
                messageText = update.Message.Text;
                messageId_last = update.Message.MessageId;

                MessagesId.Add(update.Message.MessageId);

                switch (update.Message!.Type)
                {
                    case MessageType.Text:
                        Console.WriteLine("Текст : {0}", update.Message.Text);
                        Console.WriteLine("Чат ID: {0}", update.Message.Chat.Id);
                        Console.WriteLine("Месседж ID: {0}", update.Message.MessageId);

                        if (ActiveTask == (int) activeTask.activeAlarm && messageText != "⬇")
                        {                            
                            await ToSendMessage($"Будильник установлен на {messageText}");
                            Task task = Task.Run(() => CheckTime(messageText));
                            alarmTime = messageText;
                            
                            ActiveTask = (int)activeTask.def;
                            await ReplayKeys(replyKeyboardMarkup, cancellationtoken, false);

                        }


                        if (update.Message.Text.StartsWith("/"))
                        {
                            await ExecuteTheCommand(messageText);
                            return;
                        }

                        if (update.Message.Text.StartsWith("Show me"))
                        {
                            await ExecuteTheCommand("Show_me");
                            return;
                        }

                        break;

                    case MessageType.Photo:
                        Console.WriteLine("Получено photo");

                        chatPhoto = update.Message.Photo[0] == null ? update.Message.Photo[0] : null;
                        await ToSendMessage(id: "photo", photo: chatPhoto);
                        break;
                }



                switch (update.Message.Text)
                {
                    case "Ты тут?":
                        await ToSendMessage("Да, сэр");
                        return;

                    case "📝":
                        //recordingNotes = recordingNotes ? false : true;
                        //await ToSendMessage("_");
                        await ReplayKeys(replyKeyboardMarkup_notes, cancellationtoken, true);
                        return;

                    case "requests":
                        await ReplayKeys(replyKeyboardMarkup_zaprosi, cancellationtoken, false);
                        return;

                    case "⬇":
                        hide_ON = true;
                        await ReplayKeys(replyKeyboardMarkup_HEY_HIDE, cancellationtoken, true);
                        return;

                    case "settings":
                        await ToSendMessage("_");
                        return;

                    case "OFF":
                        await ToSendMessage("😴");
                        Console.WriteLine("OFF");
                        System.Diagnostics.Process.Start("cmd", "/c shutdown -s -f -t 00");
                        return;

                    case "Show buffer contents" or "S":
                        await ExecuteTheCommand("/showbuffer");
                        return;

                    case "Clear buffer":
                        await ExecuteTheCommand("/clearbuffer");
                        return;



                    case "👋":
                        hide_ON = false;
                        await ReplayKeys(replyKeyboardMarkup, cancellationtoken, false, "Здравствуйте, сэр 👋");
                        return;

                    case "💲":

                        await ExecuteTheCommand("/usd");
                        return;

                    case "⏰":

                        await ExecuteTheCommand("/setAlarmTime");
                        await ReplayKeys(replyKeyboardMarkup_Alarm_Time, cancellationtoken, false);
                        return;

                    case "set ⏰":

                        await ToSendMessage("Установить на время = ...");
                        ActiveTask = (int) activeTask.activeAlarm;
                        return;

                    case "❌":
                        await ToSendMessage("Будильник выключен");
                        alarmTime = null;
                        await ReplayKeys(replyKeyboardMarkup, cancellationtoken, true);
                        return;
                }



                Set.Add(messageText);



                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                await ReplayKeys(replyKeyboardMarkup, cancellationtoken, false);




            }

            async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                await Task.CompletedTask;
            }

            async Task ReplayKeys(ReplyKeyboardMarkup replyKeyboardMarkup, CancellationToken cancellationToken, bool showHide, string out_text = "_")
            {
                if (hide_ON && !showHide)
                    return;

                if (recordingNotes) return;

                Message sentMassage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: out_text,
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);


                //await ExecuteTheCommand("");
                MessagesId.Add(sentMassage.MessageId);
                //await ExecuteTheCommand("/delete");

            }

            async Task DeleteMess(int id)
            {
                await botClient.DeleteMessageAsync(
                    chatId: chatId,
                    messageId: id,
                    cancellationToken: default);
            }

            async Task ToSendMessage(string line = "пусто", string id = "text", PhotoSize photo = null)
            {
                string mypath = @"C:\\Users\\Dayks\\Desktop\\pot.jpg";
                //await ExecuteTheCommand("/delete");

                switch (id)
                {
                    case "text":
                        Message sentMassage = await botClient.SendTextMessageAsync(
                            chatId: chatId,

                            text: line,
                            cancellationToken: default);
                        MessagesId.Add(sentMassage.MessageId);
                        break;

                    case "photo":
                        Console.WriteLine("Отправка фото");

                        using (var fileStream = new FileStream(mypath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            Message sentMassage2 = await botClient.SendPhotoAsync(
                            chatId: chatId,
                            photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream),
                            cancellationToken: default);
                            MessagesId.Add(sentMassage2.MessageId);

                        };

                        break;
                }

            }

            async Task ExecuteTheCommand(string line)
            {
                switch (line)
                {
                    case "/showbuffer":
                        if (Set.Count == 0)
                        {
                            await ToSendMessage("Буффер пуст");
                            return;
                        }

                        foreach (string s in Set)
                            await ToSendMessage(s);
                        break;

                    case "/clearbuffer":
                        Set.Clear();
                        await ExecuteTheCommand("/showbuffer");
                        break;

                    case "/start":
                        //await ReplayKeys(replyKeyboardMarkup, cancellationtoken, false);
                        break;

                    case "/delete":

                        foreach (int id in MessagesId)
                            await DeleteMess(id);

                        MessagesId.Clear();
                        break;

                    case "/setAlarmTime":

                        if (alarmTime != null)
                        {
                            await ToSendMessage($"Будильник установлен на {alarmTime}");
                        }
                        break;

                    case "/usd":

                        string parsline = "";
                        using (WebClient wc = new WebClient())
                            parsline = wc.DownloadString("https://www.cbr-xml-daily.ru/daily_utf8.xml");


                        Match match = Regex.Match(parsline, "<Name>Доллар США</Name><Value>(.*?)</Value>");

                        await ToSendMessage("Курс доллара - " + match.Groups[1].Value);
                        break;

                    case "Show_me":

                        //string parsline2 = "";
                        using (WebClient wc = new WebClient())
                            //parsline2 = wc.DownloadString("https://yandex.ru/search/?text=что+такое+xml&lr=118936&clid=2270455&win=508&src=suggest_T.html");
                            //parsline2 = wc.DownloadString("https://vellisa.ru/kak-skopirovat-veb-stranitsu-esli-tam-ustanovlena-zashhita-ot-kopirovaniya.html");
                            //parsline2 = wc.DownloadString("https://www.google.ru/search?q=what+is+xml&newwindow=1&sxsrf=ALiCzsaXiaUcD4PQ9qvcqQxV9t4LLjcQNg%3A1660722940915&ei=_J78YvnCN6zLrgS0-rOYCA&ved=0ahUKEwj5sNqHs835AhWspYsKHTT9DIMQ4dUDCA4&uact=5&oq=what+is+xml&gs_lcp=Cgdnd3Mtd2l6EAMyBAgAEEMyCggAEIAEEIcCEBQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDoECCMQJzoLCC4QgAQQxwEQ0QM6BQgAEMsBSgQIQRgASgQIRhgAUABY3PkFYMWEBmgAcAF4AIABbYgB6AGSAQMyLjGYAQCgAQGgAQLAAQE&sclient=gws-wiz.html");
                            //parsline2 = wc. DownloadString("https://www.google.ru/search?q=what+is+xml&newwindow=1&sxsrf=ALiCzsaXiaUcD4PQ9qvcqQxV9t4LLjcQNg%3A1660722940915&ei=_J78YvnCN6zLrgS0-rOYCA&ved=0ahUKEwj5sNqHs835AhWspYsKHTT9DIMQ4dUDCA4&uact=5&oq=what+is+xml&gs_lcp=Cgdnd3Mtd2l6EAMyBAgAEEMyCggAEIAEEIcCEBQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDoECCMQJzoLCC4QgAQQxwEQ0QM6BQgAEMsBSgQIQRgASgQIRhgAUABY3PkFYMWEBmgAcAF4AIABbYgB6AGSAQMyLjGYAQCgAQGgAQLAAQE&sclient=gws-wiz.html");
                            //parsline2 = wc.DownloadString("https://www.google.ru/search?q=what+is+xml&newwindow=1&sxsrf=ALiCzsaXiaUcD4PQ9qvcqQxV9t4LLjcQNg%3A1660722940915&ei=_J78YvnCN6zLrgS0-rOYCA&ved=0ahUKEwj5sNqHs835AhWspYsKHTT9DIMQ4dUDCA4&uact=5&oq=what+is+xml&gs_lcp=Cgdnd3Mtd2l6EAMyBAgAEEMyCggAEIAEEIcCEBQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDIFCAAQgAQyBQgAEIAEMgUIABCABDoECCMQJzoLCC4QgAQQxwEQ0QM6BQgAEMsBSgQIQRgASgQIRhgAUABY3PkFYMWEBmgAcAF4AIABbYgB6AGSAQMyLjGYAQCgAQGgAQLAAQE&sclient=gws-wiz.html");

                            await ToSendMessage();
                        break;

                    default:
                        await ToSendMessage("Неизвестная команда");
                        break;
                }

            }


            async Task CheckTime(string time)
            {
                while (true)
                {
                    string timez = (DateTime.Now.ToString().Remove(0, 11).Remove(5, 3));
                    if (timez == time!)
                    {
                        await ToSendMessage($"⏰ • {timez}");
                    }
                    Console.WriteLine($"timez = {timez}, time = {time}");

                    Thread.Sleep(60000);

                }

            }

        }
        enum activeTask
        {
           def = 0,
           activeAlarm
        }

    }  

        
}

