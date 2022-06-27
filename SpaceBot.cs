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
using SpaceInformationBot.Client;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using SpaceInformationBot.Model;
using SpaceInformationBot.Clients;
using System.ComponentModel.DataAnnotations;


namespace SpaceInformationBot
{
    public class SpaceBot
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
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hi, my name is NasaBot\nI can help you to know new interesting information");

                await SendMainMenuAsync(botClient, message);

                return;
            }
            else
            if (message.Text == "🛰️ISS")
            {
                await SendISS(botClient, message);
                return;
            }
            else
            if (message.Text == "🌌APOD")
            {  
                await SendAPOD(botClient, message);
                return;
            }
            else
            if (message.Text == "🌠APODbyDate")
            {
                LastText = "📆Enter a date:";

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter a date:",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                    );

                return;
            }
            else
            if (LastText == "📆Enter a date:")
            {
                if (message.Text == null)
                {
                    return;
                }
                string date = message.Text;

                Regex pattern = new Regex("[0-9]{4}-[0-9]{2}-[0-9]{2}");
                

                Match match = pattern.Match(date);
                if (match.Success)
                {
                    if (FirstText == "🪐MarsRoverPhotos")
                    {
                        Date = date;
                        await SendChooseCamera(botClient, message);
                    }
                    else
                        await SendAPOD(botClient, message, date);
                    
                }
                FirstText = message.Text;
                LastText = message.Text;
                return;
            }
            else
            if (message.Text == "🪐MarsRoverPhotos")
            {
                FirstText = message.Text;
                LastText = "📆Enter a date:";

                //Message message1 = await botClient.SendTextMessageAsync(
                //    message.Chat.Id,
                //    text: "Loading...",
                //    replyMarkup: new ReplyKeyboardRemove()
                //    );

                //await botClient.DeleteMessageAsync(
                //    message1.Chat.Id,
                //    message1.MessageId
                //    );

                

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "📆Enter a date:",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                    );
                //await SendChooseCamera(botClient, message);

                return;
            }
            else
            if (message.Text == "🚀LocationOfISS")
            {
                await SendLocationOfISS(botClient, message);
                return; 
            }
            else
            if (message.Text == "🧑🏻‍🚀NoPiS")
            {
                await SendNumberOfPeople(botClient, message);
            }
            else
            if (message.Text == "/help")
            {
                await SendHelpMessage(botClient, message);

            }
            else
            if (message.Text == "/myfavourites")
            {
                await SendListOfFavouritesAPODorPhoto(botClient, message);
            }
        }

        private async Task SendListOfFavouritesAPODorPhoto(ITelegramBotClient botClient, Message message)
        {
            Message message1 = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text: "Loading...",
                replyMarkup: new ReplyKeyboardRemove()
                );

            await botClient.DeleteMessageAsync(
                message1.Chat.Id,
                message1.MessageId
                );

            InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🌌My APOD list", $"APODlist"),
                            InlineKeyboardButton.WithCallbackData( "🪐My Mars photo list ", $"MarsPhotoList")
                        },
                    });

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "You have two Favourite lists",
                replyMarkup: inlineKeyboard
                );

        }

        private async Task SendHelpMessage(ITelegramBotClient botClient, Message message)
        {
            string text = "some info";
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text
                );

            return;
        }

        private async Task SendNumberOfPeople(ITelegramBotClient botClient, Message message)
        {
            var peopleInSpace = new OpenNotifyClient().GetPeopleInSpaceAsync().Result;

            if (peopleInSpace == null)
            {
                return;
            }

            Message message1 = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Loading...",
                ParseMode.Markdown,
                replyMarkup: new ReplyKeyboardRemove()
                );

            await botClient.DeleteMessageAsync(message1.Chat.Id, message1.MessageId);


            string text = "Number of people in Space: " + peopleInSpace.number;
            for (int i = 0; i < peopleInSpace.number; i++)
            {
                
                text += $"\n\n{i+1}. {peopleInSpace.people[i].name} - [ℹ️](https://google.com/search?q={peopleInSpace.people[i].name})";
                
            }
            text += "\n\n_Click ℹ️ for more info_";

            

            InlineKeyboardMarkup keyboard = new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close")
                            //InlineKeyboardButton.WithCallbackData( "Update", $"UpdateLocation")

                        },
                    });

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                ParseMode.Markdown,
                replyMarkup: keyboard,
                disableWebPagePreview: true
                );

            return;
        }
        private async Task SendLocationOfISS(ITelegramBotClient botClient, Message message)
        {
            var location = new OpenNotifyClient().GetLocationAsync().Result;

            if (location == null)
            {
                return;
            }

            string text = "Now you can see where the ISS is";

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: new ReplyKeyboardRemove()
                );

            InlineKeyboardMarkup keyboard = new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "Update", $"UpdateLocation")

                        },
                    });

            await botClient.SendVenueAsync(
                message.Chat.Id,
                location.iss_position.latitude,
                location.iss_position.longitude,
                title: "Location of ISS",
                address:"Latitude: " + location.iss_position.latitude + " | " +

                "Longitude: " + location.iss_position.longitude,
                replyMarkup: keyboard
                );

            return;
        }

        private InlineKeyboardMarkup InlineCameraKeyboard()
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
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "🔍Info", $"InfoCamera")
                        }
                    });
        }
        private InlineKeyboardMarkup inlineCloseAndFav(bool c)
        {
            if(!c)
                return new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "Like it", $"Favourite")

                        },
                    });
            else
                return  new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "❤️Liked", $"Favourite")

                        },
                    });
        }
        private InlineKeyboardMarkup inlineUnderMarsPhoto(bool value)
        {
            InlineKeyboardMarkup inlineKeyboard;
            if (!value)
            {
                inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️", $"Left"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{photos.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"Right")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackCamera"),
                            InlineKeyboardButton.WithCallbackData("📝Details", $"Details")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData("Like it", $"FavouritePhoto")
                        }
                    });
            }
            else
                inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️", $"Left"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{photos.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"Right")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackCamera"),
                            InlineKeyboardButton.WithCallbackData("📝Details", $"Details")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData("❤️Liked", $"FavouritePhoto")
                        }
                    });
           
            return inlineKeyboard;
        }
        private InlineKeyboardMarkup inlineUnderFavouriteAPOD(bool value)
        {
            InlineKeyboardMarkup inlineKeyboard;
            if (value)
            {
                inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️", $"PrevAPOD"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{apodlist.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"NextAPOD")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackToFavourites"),

                            InlineKeyboardButton.WithCallbackData("🗑️Remove", $"RemoveFromFavourite")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🚪Close", $"Close")
                        }
                    });
                return inlineKeyboard;
            }
            inlineKeyboard = new(
                    new[]
                    {
                        // first row
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️", $"PrevAPOD"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{apodlist.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"NextAPOD")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackToFavourites"),

                            InlineKeyboardButton.WithCallbackData("✅Removed", $"RemoveFromFavourite")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🚪Close", $"Close")
                        }
                    });

            return inlineKeyboard;
        }


        public async Task<Message> SendMainMenuAsync(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
              {
                    new KeyboardButton [] { "🛰️ISS" },
                    new KeyboardButton [] { "🌌APOD", "🌠APODbyDate"},
                    new KeyboardButton [] { "🪐MarsRoverPhotos" }

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

        private async Task SendChooseCamera(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "📷Choose the camera:",
                replyMarkup: InlineCameraKeyboard()
                );
        }
        private async Task SendAPOD(ITelegramBotClient botClient, Message message, string? date = null)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            APOD apod;
            if ( date == null)
            {
               apod = new SpaceClient().GetAPODAsync().Result;

            }
            else
            {
                apod = new SpaceClient().GetAPODAsync(date).Result;

            }
            apod_to_db = apod;

            string text;

            if (apod.media_type == "video")
                text = "*" + apod.title + $"* [🎞️]({apod.url})" + "\n\n" + apod.explanation + "\n" + "_" + apod.date + "_\n\n _P.S. Click_ 🎞️";
            else
                text = "*" + apod.title + "*" + "\n\n" + apod.explanation + "\n" + "_" + apod.date + "_";

            Message message1 = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text: "Loading...",
                replyMarkup: new ReplyKeyboardRemove()
                );
            
            await botClient.DeleteMessageAsync(
                message1.Chat.Id,
                message1.MessageId
                );

            var NullorNot = new ApodDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apod.url).Result;

            bool c = true;
            if (NullorNot == null)
            {
                c = false;
            }
            try
            {
                

                await botClient.SendPhotoAsync(
                    message.Chat.Id,
                    apod.url,
                    caption: text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineCloseAndFav(c),

                    cancellationToken: cancellationToken
                    );
            }
            catch (Exception)
            {
                await botClient.SendPhotoAsync(
                    message.Chat.Id,
                    apod.url
                    );

                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineCloseAndFav(c),
                    disableWebPagePreview: true
                    );
            }

            return;
        }
        private async Task SendISS(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
              {
                    new KeyboardButton [] { "🚀LocationOfISS", "🧑🏻‍🚀NoPiS" }
                })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Press one of the buttons:\n" +
                "_LocationOfISS_ - find out the location of the international Space Station\n" +
                "_NSP_ - find out the number of people in Space",
                parseMode: ParseMode.Markdown,
                replyMarkup: replyKeyboardMarkup
                );
            return;
        }


        // callbackdata
        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "FavouritePhoto":

                    await AddFavouriteMarsPhoto(botClient, callbackQuery);
                    break;
                case "RemoveFromFavourite":
                    apod_to_db = apodlist[NumberOfPhoto];
                    await AddOrDeleteToMyFavouriteAPOD(botClient, callbackQuery, false);

                    break;
                case "NextAPOD":
                    await SendNextAPOD(botClient, callbackQuery.Message!);

                    break;
                case "PrevAPOD":
                    await SendPrevAPOD(botClient, callbackQuery.Message!);
                    break;
                case "BackToFavourites":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await SendListOfFavouritesAPODorPhoto(botClient, callbackQuery.Message!);
                    break;
                case "Favourite":
                    await AddOrDeleteToMyFavouriteAPOD(botClient, callbackQuery, true);
                    break;
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
                    //await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
                    await NextPhoto(botClient, callbackQuery.Message!);
                    break;
                case "Left":
                    //await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);
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
                case "UpdateLocation":
                    await UpdateLocation(botClient, callbackQuery);
                    break;
                case "APODlist":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await SendFavouriteAPOD(botClient, callbackQuery);
                    break;
                case "MarsPhotoList":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await SendFavouriteMarsPhotos(botClient, callbackQuery);
                    break;
            }

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id
                );
            
            return;
        }

        MarsPhotoDB marsPhoto = new();
        Photos photo = new();
        private async Task AddFavouriteMarsPhoto(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var client = new MarsPhotoDBClient();
            var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, photo.img_src);

            if (result == null)
            {
                var post = await new MarsPhotoDBClient().PostDataToDynamoDB(photo, (int)callbackQuery.Message!.Chat.Id);

                await botClient.AnswerCallbackQueryAsync(
                    callbackQuery.Id,
                    "Added successfully to 'My Favourites'"
                    );

                await botClient.EditMessageReplyMarkupAsync(
                    callbackQuery.Message!.Chat.Id,
                    callbackQuery.Message!.MessageId,
                    replyMarkup: inlineUnderMarsPhoto(true)
                    );
            }
            else
            {
                await client.DeleteDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id, result.url);

                await botClient.AnswerCallbackQueryAsync(
                    callbackQuery.Id,
                    "Removed from 'My Favoutites'"
                    );

                await botClient.EditMessageReplyMarkupAsync(
                    callbackQuery.Message!.Chat.Id,
                    callbackQuery.Message!.MessageId,
                    replyMarkup: inlineUnderMarsPhoto(false)
                    );
            }
            return;
        }

        private async Task SendNextAPOD(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto++;
            if (NumberOfPhoto == apodlist.Count)
            {
                NumberOfPhoto = 0;
            }
            string text;

            if (apodlist[NumberOfPhoto].media_type == "video")
                text = "*" + apodlist[NumberOfPhoto].title + $"* [🎞️]({apodlist[NumberOfPhoto].url})" + "\n\n" + apodlist[NumberOfPhoto].explanation + "\n" + "_" + apodlist[NumberOfPhoto].date + "_\n\n _P.S. Click_ 🎞️";
            else
                text = "*" + apodlist[NumberOfPhoto].title + "*" + "\n\n" + apodlist[NumberOfPhoto].explanation + "\n" + "_" + apodlist[NumberOfPhoto].date + "_";

            var result = new ApodDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apodlist[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))

             );
            await botClient.EditMessageCaptionAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                caption: text,
                replyMarkup: inlineUnderFavouriteAPOD(value),
                parseMode: ParseMode.Markdown
                ) ;
            //await botClient.SendPhotoAsync(
            //    message.Chat.Id,
            //    photos[NumberOfPhoto].img_src,
            //    );
            return;
        }
        private async Task SendPrevAPOD(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto--;
            if (NumberOfPhoto == -1)
            {
                NumberOfPhoto = apodlist.Count-1;
            }

            string text;

            if (apodlist[NumberOfPhoto].media_type == "video")
                text = "*" + apodlist[NumberOfPhoto].title + $"* [🎞️]({apodlist[NumberOfPhoto].url})" + "\n\n" + apodlist[NumberOfPhoto].explanation + "\n" + "_" + apodlist[NumberOfPhoto].date + "_\n\n _P.S. Click_ 🎞️";
            else
                text = "*" + apodlist[NumberOfPhoto].title + "*" + "\n\n" + apodlist[NumberOfPhoto].explanation + "\n" + "_" + apodlist[NumberOfPhoto].date + "_";


            var result = new ApodDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apodlist[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))

             );
            await botClient.EditMessageCaptionAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                caption: text,
                replyMarkup: inlineUnderFavouriteAPOD(value),
                parseMode: ParseMode.Markdown
                );
            //await botClient.SendPhotoAsync(
            //    message.Chat.Id,
            //    photos[NumberOfPhoto].img_src,
            //    );
            return;
        }

        List<MarsPhotoDB> marsPhotosList = new();
        private async Task SendFavouriteMarsPhotos(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var favouriteslist = new MarsPhotoDBClient().GetAllUserDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id).Result;

            if (favouriteslist == null || favouriteslist.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackToFavourites")
                    });
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "empty",
                    replyMarkup: inlineKeyboard
                    );
                return;
            }

            marsPhotosList = favouriteslist;

            string text = ""

            return;
        }
        List<APOD> apodlist = new();
        private async Task SendFavouriteAPOD(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var favouriteslist = new ApodDBClient().GetAllUserDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id).Result;

            if (favouriteslist == null || favouriteslist.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackToFavourites")
                    });
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "empty",
                    replyMarkup: inlineKeyboard
                    );
                return;
            }
            apodlist = favouriteslist;

            string text;

            if (favouriteslist[NumberOfPhoto].media_type == "video")
                text = "*" + favouriteslist[NumberOfPhoto].title + $"* [🎞️]({favouriteslist[NumberOfPhoto].url})" + "\n\n" + favouriteslist[NumberOfPhoto].explanation + "\n" + "_" + favouriteslist[NumberOfPhoto].date + "_\n\n _P.S. Click_ 🎞️";
            else
                text = "*" + favouriteslist[NumberOfPhoto].title + "*" + "\n\n" + favouriteslist[NumberOfPhoto].explanation + "\n" + "_" + favouriteslist[NumberOfPhoto].date + "_";

            var NullorNot = new ApodDBClient().GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, favouriteslist[NumberOfPhoto].url).Result;

            bool c = true;
            if (NullorNot == null)
            {
                c = false;
            }
            try
            {
                await botClient.SendPhotoAsync(
                    callbackQuery.Message!.Chat.Id,
                    favouriteslist[NumberOfPhoto].url,
                    caption: text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineUnderFavouriteAPOD(true)
                    );
            }
            catch (Exception)
            {
                await botClient.SendPhotoAsync(
                    callbackQuery.Message!.Chat.Id,
                    favouriteslist[NumberOfPhoto].url
                    );

                await botClient.SendTextMessageAsync(
                    callbackQuery.Message!.Chat.Id,
                    text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineUnderFavouriteAPOD(true),
                    disableWebPagePreview: true
                    );
            }

            return;
        }

        APOD apod_to_db;
        private async Task AddOrDeleteToMyFavouriteAPOD(ITelegramBotClient botClient, CallbackQuery callbackQuery, bool value)
        {
            if (value)
            {
                var client = new ApodDBClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, apod_to_db.url);

                if (result == null)
                {
                    var post = await new ApodDBClient().PostDataToDynamoDB(apod_to_db, (int)callbackQuery.Message!.Chat.Id);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Added successfully to 'My Favourites'"
                        );

                    await botClient.EditMessageReplyMarkupAsync(
                        callbackQuery.Message!.Chat.Id,
                        callbackQuery.Message!.MessageId,
                        replyMarkup: inlineCloseAndFav(true)
                        );
                }
                else
                {
                    await client.DeleteDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id, result.url);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Removed from 'My Favoutites'"
                        );

                    await botClient.EditMessageReplyMarkupAsync(
                        callbackQuery.Message!.Chat.Id,
                        callbackQuery.Message!.MessageId,
                        replyMarkup: inlineCloseAndFav(false)
                        );
                }
            }
            else
            {
                var client = new ApodDBClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, apod_to_db.url);

                if (result == null)
                {
                    var post = await new ApodDBClient().PostDataToDynamoDB(apod_to_db, (int)callbackQuery.Message!.Chat.Id);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Added successfully to 'My Favourites'"
                        );

                    await botClient.EditMessageReplyMarkupAsync(
                        callbackQuery.Message!.Chat.Id,
                        callbackQuery.Message!.MessageId,
                        replyMarkup: inlineUnderFavouriteAPOD(true)
                        );
                }
                else
                {
                    await client.DeleteDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id, result.url);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Removed from 'My Favoutites'"
                        );

                    await botClient.EditMessageReplyMarkupAsync(
                        callbackQuery.Message!.Chat.Id,
                        callbackQuery.Message!.MessageId,
                        replyMarkup: inlineUnderFavouriteAPOD(false)
                        );
                }
            }
            return;
        }

        private async Task UpdateLocation(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var location = new OpenNotifyClient().GetLocationAsync().Result;

            if (location == null)
            {
                return;
            }
            InlineKeyboardMarkup keyboard = new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "🚪Close", $"Close"),
                            InlineKeyboardButton.WithCallbackData( "Update", $"UpdateLocation")

                        },
                    });
            await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: $"Loading..."
                        );

            await botClient.DeleteMessageAsync(
                callbackQuery.Message!.Chat.Id,
                callbackQuery.Message!.MessageId
                );

            await botClient.SendVenueAsync(
                callbackQuery.Message!.Chat.Id,
                location.iss_position.latitude,
                location.iss_position.longitude,
                title: "Location of ISS",
                address: "Latitude: " + location.iss_position.latitude + " | " +  
                "Longitude: " + location.iss_position.longitude,
                replyMarkup: keyboard
                );

            return;
        }

        string? camera { get; set; }
        private async Task InfoCamera(ITelegramBotClient botClient, Message message)
        {
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackCamera"),
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
        List<Photos> photos = new();
        

        private async Task SendCameraPhoto(string date, string camera, ITelegramBotClient botClient, Message message)
        {
            photos = new SpaceClient().GetMarsPhotosAsync(date, camera).Result.photos;

            if (photos.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackCamera"),
                        }
                    });
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Unfortunately no photos",
                    replyMarkup: inlineKeyboard
                    );
                return;
            }

            photo = photos[NumberOfPhoto];
            var result = new MarsPhotoDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.SendPhotoAsync(
            message.Chat.Id,
            photos[NumberOfPhoto].img_src,
            replyMarkup: inlineUnderMarsPhoto(value)
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
            photo = photos[NumberOfPhoto];

            var result = new MarsPhotoDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(photos[NumberOfPhoto].img_src)),
                replyMarkup: inlineUnderMarsPhoto(value)

             );

            return;
            
        }
        private async Task PrevPhoto(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto--;
            if (NumberOfPhoto == -1)
            {
                NumberOfPhoto = photos.Count-1;
            }
            photo = photos[NumberOfPhoto];

            var result = new MarsPhotoDBClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(photos[NumberOfPhoto].img_src)),
                replyMarkup: inlineUnderMarsPhoto(value)
             );

            return;
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
