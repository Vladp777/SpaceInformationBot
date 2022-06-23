using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using NASAInformationBot.Client;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;

using System.IO;
using Telegram.Bot.Types.InputFiles;
using NASAInformationBot.Model;
//using MySql.Data.MySqlClient.Memcached;

namespace NASAInformationBot
{
    public class NASABot
    {
        TelegramBotClient client = new TelegramBotClient("5569588978:AAGeC3oGohGlp3qgrqfCgrmVqqb2wesQ8fo");
        
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { 
            AllowedUpdates = { },
            ThrowPendingUpdates = true,
        };

        public async Task Start()
        {
            client.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await client.GetMeAsync();
            
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
            
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMasage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n{apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}", _ => exception.ToString()
            };
            Console.WriteLine(ErrorMasage);

            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => HandlerMessageAsync(botClient, update.Message!),
                UpdateType.EditedMessage => HandlerMessageAsync(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                //_ => UnknownUpdateHandlerAsync(botClient, update)
            };
            await handler;
            //if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            //{
            //    await HandlerMessageAsync(botClient, update.Message);
            //}
        }
        string? LastText { get; set; }
        string? FirstText { get; set; }
        string? Date { get; set; }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            //if (message.Text == "/inline")
            //{
            //    InlineKeyboardMarkup inlineKeyboard = new(
            //        new[]
            //        {
            //            // first row
            //            new []
            //            {
            //                InlineKeyboardButton.WithCallbackData( @"\u27A1", $"Close"),
            //                InlineKeyboardButton.WithCallbackData( "➡️", $"Favourite")

            //            },
            //        });

            //    await botClient.SendTextMessageAsync(
            //        message.Chat.Id,
            //        "some text",
            //        replyMarkup: inlineKeyboard
            //        );
            //    return;
            //}

            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hi, my name is NasaBot\nI can help you to know new interesting information");

                await SendMainMenuAsync(botClient, message);

                return;
            }
            else
            if (message.Text == "APOD")
            {  
                await SendAPOD(botClient, message);
                return;
            }
            else
            if (message.Text == "APODbyDate")
            {
                LastText = "Enter a date:";

                Message message1 = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Loading...",
                    replyMarkup: new ReplyKeyboardRemove()
                    );

                await botClient.DeleteMessageAsync(
                    message1.Chat.Id,
                    message1.MessageId
                    );

    

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter a date:",
                    cancellationToken: cancellationToken
                    );

                return;
            }
            else
            if (LastText == "Enter a date:")
            {
                if (message.Text == null)
                {
                    return;
                }
                string date = message.Text;

                Regex pattern = new Regex("20(1|2){1}[0-9]{1}-[0-9]{2}-[0-9]{2}");
                

                Match match = pattern.Match(date);
                if (match.Success)
                {
                    if (FirstText == "MarsRoverPhotos")
                    {
                        Date = date;
                        await SendChooseCamera(botClient, message);
                    }
                    else
                        await SendAPODbyDate(date, botClient, message);
                    
                }
                FirstText = message.Text;
                LastText = message.Text;
                return;
            }
            else
            if (message.Text == "MarsRoverPhotos")
            {
                FirstText = message.Text;
                LastText = "Enter a date:";

                Message message1 = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Loading...",
                    replyMarkup: new ReplyKeyboardRemove()
                    );

                await botClient.DeleteMessageAsync(
                    message1.Chat.Id,
                    message1.MessageId
                    );

                

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter a date:",
                    cancellationToken: cancellationToken
                    );
                //await SendChooseCamera(botClient, message);

                return;
            }

        }
        private InlineKeyboardMarkup InlineCamera()
        {
            return new(
                    new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "FHAZ", $"FHAZ"),
                            InlineKeyboardButton.WithCallbackData( "RHAZ", $"RHAZ"),
                            InlineKeyboardButton.WithCallbackData( "MAST", $"MAST")


                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "MAHLI", $"MAHLI"),
                            InlineKeyboardButton.WithCallbackData( "MARDI", $"MARDI"),
                            InlineKeyboardButton.WithCallbackData( "CHEMCAM", $"CHEMCAM")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "NAVCAM", $"NAVCAM"),
                            InlineKeyboardButton.WithCallbackData( "PANCAM", $"PANCAM"),
                            InlineKeyboardButton.WithCallbackData( "MINITES", $"MINITES")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "Info", $"InfoCamera")
                        }
                    });
        }
        private InlineKeyboardMarkup inlineCloseAndFav()
        {
            return new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "Favourite", $"Favourite")

                        },
                    });
        }
        private InlineKeyboardMarkup inlinePhoto()
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "⬅️", $"Left"),

                            InlineKeyboardButton.WithCallbackData( $"{NumberOfPhoto+1}/{photos.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData( "➡️", $"Right")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Back", $"BackCamera"),
                            InlineKeyboardButton.WithCallbackData( "Details", $"Details")

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Close", $"Close")

                        }
                    });
            return inlineKeyboard;
        }
        private async Task SendChooseCamera(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Choose the camera:",
                replyMarkup: InlineCamera()
                );
        }
        private async Task SendAPOD(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            var apod = new NasaClient().GetAPODAsync();
            string text = "*" + apod.Result.title + "*" + "\n\n" + apod.Result.explanation + "\n" + "_" + apod.Result.date + "_";

            Message message1 = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text: "Loading...",
                replyMarkup: new ReplyKeyboardRemove()
                );

            await botClient.DeleteMessageAsync(
                message1.Chat.Id,
                message1.MessageId
                );

            

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                apod.Result.url,
                caption: text,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineCloseAndFav(),
                cancellationToken: cancellationToken
                );
            return;
        }
        private async Task SendAPODbyDate(string date, ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            var apod = new NasaClient().GetAPODAsync(date);
            string text = "*" + apod.Result.title + "*" + "\n\n" + apod.Result.explanation + "\n" + "_" + apod.Result.date + "_";

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                apod.Result.url,
                caption: text,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineCloseAndFav(),
                cancellationToken: cancellationToken
                );
            return;
            
        }

        public async Task<Message> SendMainMenuAsync(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
              {
                    new KeyboardButton [] {"APOD", "APODbyDate"},
                    new KeyboardButton [] {"MarsRoverPhotos"}

                })
            {
                ResizeKeyboard = true

            };

            return await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose, what you want to know:",
                    replyMarkup: replyKeyboardMarkup

                    );
            
        }

        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id
                //text: $"Received {callbackQuery.Data}"
                );

            //if (callbackQuery.Data == "Close")
            //{  
            //    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

            //    await SendMainMenuAsync(botClient, callbackQuery.Message!);
            //}

            switch (callbackQuery.Data)
            {
                case "Close":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    NumberOfPhoto = 0;
                    await SendMainMenuAsync(botClient, callbackQuery.Message!);
                    break;
                case "InfoCamera":
                    await InfoCamera(botClient,callbackQuery.Message!);
                    break;
                case "BackCamera":
                    NumberOfPhoto = 0;
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    await SendChooseCamera(botClient, callbackQuery.Message!);
                    break;
                case "Right":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    await NextPhoto(botClient, callbackQuery.Message!);
                    break;
                case "Left":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    await PrevPhoto(botClient, callbackQuery.Message!);
                    break;

                case "FHAZ":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "RHAZ":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "MAST":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "CHEMCAM":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "MAHLI":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "MARDI":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "NAVCAM":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "PANCAM":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;

                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "MINITES":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    camera = callbackQuery.Data;
                    await SendCameraPhoto(Date, callbackQuery.Data, botClient, callbackQuery.Message!);
                    break;
                case "Details":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await SendDetails(botClient, callbackQuery.Message!);
                    break;
                case "BackPhoto":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await SendCameraPhoto(Date, camera, botClient, callbackQuery.Message!);
                    break;
            }

            return;
        }
        string camera { get; set; }
        private async Task InfoCamera(ITelegramBotClient botClient, Message message)
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Back", $"BackCamera"),
                        },
                    });

            FileStream fsSource = new FileStream(@"D:\GitRep\TelegramBot\cameraExample.png", FileMode.Open, FileAccess.Read);
            InputOnlineFile file = new InputOnlineFile(fsSource);

            string infotext = " You can choose the camera and see it's photos\n" +
                "FHAZ -_Front Hazard Avoidance Camera_\n" +
                "RHAZ - _Rear Hazard Avoidance Camera_\n" +
                "MAST - _Mast Camera_\n" +
                "CHEMCAM - _Chemistry and Camera Complex_\n" +
                "MAHLI - _Mars Hand Lens Imager_\n" +
                "MARDI - _Mars Descent Imager_\n" +
                "NAVCAM - _Navigation Camera_\n" +
                "PANCAM - _Panoramic Camera_\n" +
                "MINITES - _Miniature Thermal Emission Spectrometer (Mini-TES)_";


            await botClient.SendPhotoAsync(
                message.Chat.Id,
                file,
                parseMode: ParseMode.Markdown,
                caption: infotext,
                replyMarkup: inlineKeyboard
                );
        }

        int NumberOfPhoto = 0;
        List<Photos> photos = new List<Photos>();
        Message photomes;
        private async Task SendCameraPhoto(string date, string camera, ITelegramBotClient botClient, Message message)
        {
            photos = new NasaClient().GetMarsPhotosAsync(date, camera).Result.photos;


            if (photos.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Back", $"BackCamera"),

                        }
                    });
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Unfortunately no photos",
                    replyMarkup: inlineKeyboard
                    );
                return;
            }


            await botClient.SendPhotoAsync(
            message.Chat.Id,
            photos[NumberOfPhoto].img_src,
            replyMarkup: inlinePhoto()
            );
            

            return;
            //NumberOfPhoto++;
            //await botClient.EditMessageMediaAsync(

            //    message.Chat.Id,
            //    message.MessageId,
            //    );
        }
        private async Task NextPhoto(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto++;
            if (NumberOfPhoto == photos.Count)
            {
                NumberOfPhoto = 0;
            }

            //var link = new InputOnlineFile("https://mars.nasa.gov/msl-raw-images/proj/msl/redops/ods/surface/sol/02398/opgs/edr/fcam/FRB_610378727EDR_F0751398FHAZ00341M_.JPG");

            //await botClient.EditMessageMediaAsync(
            //    chatId: message.Chat.Id,
            //    messageId: message.MessageId,
            //    InputMediaBase: //"https://mars.nasa.gov/msl-raw-images/proj/msl/redops/ods/surface/sol/02398/opgs/edr/fcam/FRB_610378727EDR_F0751398FHAZ00341M_.JPG",
            //    //replyMarkup: inlinePhoto()

            //    //new InputMediaBase( "https://mars.nasa.gov/msl-raw-images/proj/msl/redops/ods/surface/sol/02398/opgs/edr/fcam/FRB_610378727EDR_F0751398FHAZ00341M_.JPG")
            //    //new InputOnlineFile( "https://mars.nasa.gov/msl-raw-images/proj/msl/redops/ods/surface/sol/02398/opgs/edr/fcam/FRB_610378727EDR_F0751398FHAZ00341M_.JPG") //photos[NumberOfPhoto].img_src
            //    );

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                photos[NumberOfPhoto].img_src,
                replyMarkup: inlinePhoto()
                );
            return;
            //await botClient.edit
        }
        private async Task PrevPhoto(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto--;
            if (NumberOfPhoto == -1)
            {
                NumberOfPhoto = photos.Count-1;
            }

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                photos[NumberOfPhoto].img_src,
                replyMarkup: inlinePhoto()
                );
            return;
            //await botClient.edit
        }

        private async Task SendDetails(ITelegramBotClient botClient, Message message)
        {
            string roverName = photos[0].rover.name;

            string landingDate = photos[0].rover.landing_date;

            string launchDate = photos[0].rover.launch_date;

            string status = photos[0].rover.status;

            string cameraName = photos[NumberOfPhoto].camera.full_name;

            string content = "*Curiosity* is a car-sized Mars rover designed to explore the Gale crater on Mars as part of NASA's Mars Science Laboratory mission.\n" +
                "*Launch date*: _" + launchDate + "_\n" +
                "*Landing date*: _" + landingDate + "_\n" +
                "*Status*: _" + status + "_\n";

            InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {

                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Back", $"BackPhoto"),

                        }
                    });

            FileStream fsSource = new FileStream(@"D:\GitRep\TelegramBot\Curiosity.jpg", FileMode.Open, FileAccess.Read);
            InputOnlineFile file = new InputOnlineFile(fsSource);

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                file,
                caption: content,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard
                );
            return;
        }
    }
}
