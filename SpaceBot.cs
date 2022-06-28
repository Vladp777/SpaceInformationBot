using SpaceInformationBot.Clients;
using SpaceInformationBot.Constant;
using SpaceInformationBot.Model;

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpaceInformationBot
{
    public class SpaceBot
    {
        TelegramBotClient client = new TelegramBotClient(Constants.TelegramToken);
        
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
            Thread.Sleep(int.MaxValue);
            //Console.ReadKey();
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
                UpdateType.Message => HandlerMessageAsync(botClient, update.Message!),
                UpdateType.EditedMessage => HandlerMessageAsync(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                //_ => UnknownUpdateHandlerAsync(botClient, update)
            };
            await handler;
        }
        string? LastText { get; set; }
        string? FirstText { get; set; }
        string Date { get; set; }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Hi, my name is *SpaceInfoInfoBot*✋\n\n" +
                    "Here you can see interesting photos of Mars🪐,\n" +
                    "fascinating information about Space🛸 \n" +
                    "and some information about the ISS🛰",
                    parseMode: ParseMode.Markdown
                    );

                await SendHelpMessage(botClient, message);

                await SendMainMenuAsync(botClient, message);
            }
            else
            if (message.Text == "/menu")
            {
                await SendMainMenuAsync(botClient, message);
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
            else
            if (message.Text == "/clearapod")
            {
                await ClearFavouriteAPODlist(botClient, message);
            }
            else
            if (message.Text == "/clearmars")
            {
                await ClearFavouriteMarsPhotosList(botClient, message);
            }
            else
            if (message.Text == "🛰️ISS")
            {
                await SendISS(botClient, message);
            }
            else
            if (message.Text == "🌌APOD")
            {  
                await SendAPOD(botClient, message);
            }
            else
            if (message.Text == "🌠APODbyDate")
            {
                LastText = "Enter the date in the appropriate format(YYYY-MM-DD) 📆";

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter the date in the appropriate format(YYYY-MM-DD) 📆",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                    );
            }
            else
            if (LastText == "Enter the date in the appropriate format(YYYY-MM-DD) 📆")
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
                        try
                        {
                            Date = date;
                            await SendChooseCamera(botClient, message);
                        }
                        catch (Exception)
                        {

                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Error, enter the date between 2012-08-06 and today"
                                );
                        }
                        
                    }
                    else
                        try
                        {
                            await SendAPOD(botClient, message, date);
                        }
                        catch (Exception)
                        {
                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Error, something went wrong, try again"
                                );
                        }

                    FirstText = message.Text;
                    LastText = message.Text;
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "The date entered incorrectly, try again"
                    );
                }
                
            }
            else
            if (message.Text == "🪐MarsRoverPhotos")
            {
                FirstText = message.Text;
                LastText = "Enter the date in the appropriate format(YYYY-MM-DD) 📆";

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter the date in the appropriate format(YYYY-MM-DD) and between 2012-08-06 and today 📆",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                    );

            }
            else
            if (message.Text == "🚀LocationOfISS")
            {
                await SendLocationOfISS(botClient, message);
            }
            else
            if (message.Text == "🧑🏻‍🚀NoPiS")
            {
                await SendNumberOfPeople(botClient, message);
            }
            
            return;
        }

        private async Task ClearFavouriteMarsPhotosList(ITelegramBotClient botClient, Message message)
        {

            string text = "Are you really want to clear the list😳";
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: inlineClearOrNot(false)
                );
            return;
        }

        private async Task ClearFavouriteAPODlist(ITelegramBotClient botClient, Message message)
        {
            string text = "Are you really want to clear the list😳";
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: inlineClearOrNot(true)
                );
            return;
        }

        private async Task SendListOfFavouritesAPODorPhoto(ITelegramBotClient botClient, Message message)
        {
            if (!norm)
            {
                await botClient.DeleteMessageAsync(
                apodmessage.Chat.Id,
                apodmessage.MessageId
                );
            }
            norm = true;
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
            string text = "*You can control me by sending these commands:*" +
                "\n\n/menu - call the main menu" +
                "\n/myfavourites - lists of objects you like will be displayed" +
                "\n/clearapod - all your 'Favourite APODs' list items will be deleted" +
                "\n/clearmars - all your 'Favourite Mars photos' list items will be deleted" +
                "\n/help - instructions for using the bot" +
                "\n\n*Button functions:*" +
                "\n*🛰️ISS* - information about the location of the ISS and the number of people in Space" +
                "\n*🌌APOD* - information about Astronomy Picture of the Day " +
                "\n*🌠APODbyDate*  - information about Astronomy Picture of a certain Day" +
                "\n*🪐MarsRoverPhotos* - incredible photos of Mars, just enter the date and" +
                " will see landscapes of this beautiful planet taken on this day";


            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                parseMode: ParseMode.Markdown
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

        private InlineKeyboardMarkup inlineClearOrNot(bool c)
        {
            if (c)
                return new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "NO", $"No")

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Yes", $"clearApod")

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Of course not", $"No")


                        }
                        
                    });
            else
                return new(
                    new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "NO", $"No")

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Yes", $"clearMars")

                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Of course not", $"No")


                        }

                    });
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
        private InlineKeyboardMarkup inlineUnderFavouriteMarsPhoto(bool value)
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
                            InlineKeyboardButton.WithCallbackData("⬅️", $"PrevFavouritePhoto"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{marsPhotosList.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"NextFavouritePhoto")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackToFavourites"),

                            InlineKeyboardButton.WithCallbackData("🗑️Remove", $"RemoveFavouritePhoto")
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
                            InlineKeyboardButton.WithCallbackData("⬅️", $"PrevFavouritePhoto"),

                            InlineKeyboardButton.WithCallbackData($"{NumberOfPhoto + 1}/{marsPhotosList.Count}", "count"),

                            InlineKeyboardButton.WithCallbackData("➡️", $"NextFavouritePhoto")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("↩️Back", $"BackToFavourites"),

                            InlineKeyboardButton.WithCallbackData("✅Removed", $"RemoveFavouritePhoto")
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
            if (!norm)
            {
                await botClient.DeleteMessageAsync(
                apodmessage.Chat.Id,
                apodmessage.MessageId
                );
            }
            norm = true;

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
                    "Choose, what you want to see:",
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
               apod = new APODClient().GetAPODAsync().Result;

            }
            else
            {
                apod = new APODClient().GetAPODAsync(date).Result;

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

            var NullorNot = new APODClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apod.url).Result;

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
                norm = false;

                apodmessage = await botClient.SendPhotoAsync(
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
                case "clearMars":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await DeleteAllMarsItems(botClient, callbackQuery.Message!);
                    break;
                case "clearApod":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await DeleteAllApodItems(botClient, callbackQuery.Message!);
                    break;
                case "No":
                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    await botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId);

                    break;
                case "RemoveFavouritePhoto":
                    await AddOrDeleteFavouriteMarsPhoto(botClient, callbackQuery, false);

                    break;
                case "NextFavouritePhoto":
                    await SendNextFavouritePhoto(botClient, callbackQuery.Message!);
                    break;
                case "PrevFavouritePhoto":
                    await SendPrevFavouritePhoto(botClient, callbackQuery.Message!);
                    break;
                case "FavouritePhoto":

                    await AddOrDeleteFavouriteMarsPhoto(botClient, callbackQuery, true);
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
                    NumberOfPhoto = 0;
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


        private async Task DeleteAllMarsItems(ITelegramBotClient botClient, Message message)
        {
            var client = new MarsRoverPhotosClient();
            var chek = client.GetAllUserDataFromDynamoDB((int)message.Chat.Id).Result;
            if (chek == null || chek.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Your list is already empty"
                    );
            }
            else
            {
                await client.DeleteAllUserDataFromDynamoDB((int)message.Chat.Id);

                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Your list cleared successfully✅"
                    );
            }
            return;
        }
        private async Task DeleteAllApodItems(ITelegramBotClient botClient, Message message)
        {
            var client = new APODClient();
            
            var c = client.DeleteAllUserDataFromDynamoDB((int)message.Chat.Id).Result;
            if (c)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                "Your list cleared successfully✅"
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                "Your list is already empty"
                );
            }
                
            
            return;
        }

        private async Task SendPrevFavouritePhoto(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto--;
            if (NumberOfPhoto == -1)
            {
                NumberOfPhoto = marsPhotosList.Count - 1;
            }
            string text = "*Rover of Mars name*: " + marsPhotosList[NumberOfPhoto].roverName +
                "\n*Name of camera*: " + marsPhotosList[NumberOfPhoto].cameraName +
                "\n*Date*: " + marsPhotosList[NumberOfPhoto].earth_date;

            var result = new MarsRoverPhotosClient().GetInfoAboutUserFavourites((int)message.Chat.Id, marsPhotosList[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(marsPhotosList[NumberOfPhoto].url)),
                replyMarkup: inlineUnderFavouriteMarsPhoto(value)

             );
            await botClient.EditMessageCaptionAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                caption: text,
                replyMarkup: inlineUnderFavouriteMarsPhoto(value),
                parseMode: ParseMode.Markdown
                );

            return;
        }
        private async Task SendNextFavouritePhoto(ITelegramBotClient botClient, Message message)
        {
            NumberOfPhoto++;
            if (NumberOfPhoto == marsPhotosList.Count)
            {
                NumberOfPhoto = 0;
            }
            string text = "*Rover of Mars name*: " + marsPhotosList[NumberOfPhoto].roverName +
                "\n*Name of camera*: " + marsPhotosList[NumberOfPhoto].cameraName +
                "\n*Date*: " + marsPhotosList[NumberOfPhoto].earth_date;

            var result = new MarsRoverPhotosClient().GetInfoAboutUserFavourites((int)message.Chat.Id, marsPhotosList[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }

            await botClient.EditMessageMediaAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                new InputMediaPhoto(new InputMedia(marsPhotosList[NumberOfPhoto].url)),
                replyMarkup: inlineUnderFavouriteMarsPhoto(value)

             );
            await botClient.EditMessageCaptionAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                caption: text,
                replyMarkup: inlineUnderFavouriteMarsPhoto(value),
                parseMode: ParseMode.Markdown
                );

            return;
        }


        Photos photo;
        private async Task AddOrDeleteFavouriteMarsPhoto(ITelegramBotClient botClient, CallbackQuery callbackQuery, bool value)
        {
            

            if (value)
            {
                var client = new MarsRoverPhotosClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, photos[NumberOfPhoto].img_src);
                if (result == null)
                {
                    var post = await new MarsRoverPhotosClient().PostDataToDynamoDB(photos[NumberOfPhoto], (int)callbackQuery.Message!.Chat.Id);

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
            }
            else
            {
                var client = new MarsRoverPhotosClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, marsPhotosList[NumberOfPhoto].url);

                if (result == null)
                {
                    var post = await new MarsRoverPhotosClient().PostDataToDynamoDB(marsPhotosList[NumberOfPhoto], (int)callbackQuery.Message!.Chat.Id);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQuery.Id,
                        "Added successfully to 'My Favourites'"
                        );

                    await botClient.EditMessageReplyMarkupAsync(
                        callbackQuery.Message!.Chat.Id,
                        callbackQuery.Message!.MessageId,
                        replyMarkup: inlineUnderFavouriteMarsPhoto(true)
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
                        replyMarkup: inlineUnderFavouriteMarsPhoto(false)
                        );
                }
            }
                
            return;
        }
        private async Task AddOrDeleteToMyFavouriteAPOD(ITelegramBotClient botClient, CallbackQuery callbackQuery, bool value)
        {
            if (value)
            {
                var client = new APODClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, apod_to_db.url);

                if (result == null)
                {
                    var post = await new APODClient().PostDataToDynamoDB(apod_to_db, (int)callbackQuery.Message!.Chat.Id);

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
                var client = new APODClient();
                var result = await client.GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, apod_to_db.url);

                if (result == null)
                {
                    var post = await new APODClient().PostDataToDynamoDB(apod_to_db, (int)callbackQuery.Message!.Chat.Id);

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

        bool norm = true;
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

            var result = new APODClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apodlist[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }
            if (norm)
            {
                apodmessage = await botClient.EditMessageMediaAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))
                    );

                try
                {
                    await botClient.EditMessageCaptionAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: text,
                    replyMarkup: inlineUnderFavouriteAPOD(value),
                    parseMode: ParseMode.Markdown
                    );
                }
                catch (Exception)
                {
                    norm = false;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text,
                        replyMarkup: inlineUnderFavouriteAPOD(value),
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                        );
                }
                
            }
            else
            {
                await botClient.DeleteMessageAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId);

                apodmessage =  await botClient.EditMessageMediaAsync(
                    chatId: apodmessage.Chat.Id,
                    messageId: apodmessage.MessageId,
                    new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))
                    );
                try
                {
                    norm = true;
                    await botClient.EditMessageCaptionAsync(
                    chatId: apodmessage.Chat.Id,
                    messageId: apodmessage.MessageId,
                    caption: text,
                    replyMarkup: inlineUnderFavouriteAPOD(value),
                    parseMode: ParseMode.Markdown
                    );
                }
                catch (Exception)
                {
                    norm = false;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text,
                        replyMarkup: inlineUnderFavouriteAPOD(value),
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                        );
                }
            }
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


            var result = new APODClient().GetInfoAboutUserFavourites((int)message.Chat.Id, apodlist[NumberOfPhoto].url).Result;

            bool value = true;
            if (result == null)
            {
                value = false;
            }

            if (norm)
            {
                apodmessage = await botClient.EditMessageMediaAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))
                    );

                try
                {
                    await botClient.EditMessageCaptionAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: text,
                    replyMarkup: inlineUnderFavouriteAPOD(value),
                    parseMode: ParseMode.Markdown
                    );
                }
                catch (Exception)
                {
                    norm = false;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text,
                        replyMarkup: inlineUnderFavouriteAPOD(value),
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview:true
                        );
                }

            }
            else
            {
                await botClient.DeleteMessageAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId);

                apodmessage = await botClient.EditMessageMediaAsync(
                    chatId: apodmessage.Chat.Id,
                    messageId: apodmessage.MessageId,
                    new InputMediaPhoto(new InputMedia(apodlist[NumberOfPhoto].url))
                    );
                try
                {
                    norm = true;
                    await botClient.EditMessageCaptionAsync(
                    chatId: apodmessage.Chat.Id,
                    messageId: apodmessage.MessageId,
                    caption: text,
                    replyMarkup: inlineUnderFavouriteAPOD(value),
                    parseMode: ParseMode.Markdown
                    );
                }
                catch (Exception)
                {
                    norm = false;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text,
                        replyMarkup: inlineUnderFavouriteAPOD(value),
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                        );
                }
            }
            return;
        }

        List<MarsPhotoDB> marsPhotosList = new();
        private async Task SendFavouriteMarsPhotos(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var favouriteslist = new MarsRoverPhotosClient().GetAllUserDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id).Result;

            if (favouriteslist == null || favouriteslist.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackToFavourites"),
                        InlineKeyboardButton.WithCallbackData( "Close", $"Close")

                    });
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Your list is empty",
                    replyMarkup: inlineKeyboard
                    );
                return;
            }

            marsPhotosList = favouriteslist;

            string text = "*Rover of Mars name*: " + favouriteslist[NumberOfPhoto].roverName +
                "\n*Name of camera*: " + favouriteslist[NumberOfPhoto].cameraName +
                "\n*Date*: " + favouriteslist[NumberOfPhoto].earth_date;

            await botClient.SendPhotoAsync(
                    callbackQuery.Message!.Chat.Id,
                    favouriteslist[NumberOfPhoto].url,
                    caption: text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineUnderFavouriteMarsPhoto(true)
                    );
            return;
        }
        List<APOD> apodlist = new();

        Message apodmessage = new();
        private async Task SendFavouriteAPOD(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var favouriteslist = new APODClient().GetAllUserDataFromDynamoDB((int)callbackQuery.Message!.Chat.Id).Result;

            if (favouriteslist == null || favouriteslist.Count == 0)
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData( "↩️Back", $"BackToFavourites"),
                        InlineKeyboardButton.WithCallbackData( "Close", $"Close")

                    });
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    "Your list is empty",
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

            var NullorNot = new APODClient().GetInfoAboutUserFavourites((int)callbackQuery.Message!.Chat.Id, favouriteslist[NumberOfPhoto].url).Result;

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
                norm = false;

                apodmessage = await botClient.SendPhotoAsync(
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

            //FileStream fsSource = new FileStream(@"cameraExample.png", FileMode.Open, FileAccess.Read);
            //InputOnlineFile file = new InputOnlineFile(fsSource);

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


            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                infotext,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard
                );
        }

        int NumberOfPhoto = 0;
        List<Photos> photos = new();
        

        private async Task SendCameraPhoto(string date, string camera, ITelegramBotClient botClient, Message message)
        {
            photos = new MarsRoverPhotosClient().GetMarsPhotosAsync(date, camera).Result.photos;

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
            var result = new MarsRoverPhotosClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
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

            var result = new MarsRoverPhotosClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
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

            var result = new MarsRoverPhotosClient().GetInfoAboutUserFavourites((int)message.Chat.Id, photos[NumberOfPhoto].img_src).Result;
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

            //FileStream fsSource = new FileStream(@"Curiosity.jpg", FileMode.Open, FileAccess.Read);
            //InputOnlineFile file = new InputOnlineFile(fsSource);

            await botClient.SendPhotoAsync(
                message.Chat.Id,
                "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dc/PIA16239_High-Resolution_Self-Portrait_by_Curiosity_Rover_Arm_Camera.jpg/431px-PIA16239_High-Resolution_Self-Portrait_by_Curiosity_Rover_Arm_Camera.jpg",
                caption: content,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard
                );
            return;
        }
    }
}
