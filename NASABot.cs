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
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
              {
                    new KeyboardButton [] {"APOD", "APODbyDate"},
                    new KeyboardButton [] {"RoverPhoto"}

                })
                {
                    ResizeKeyboard = true

                };
                
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hi, my name is NasaBot\nI can help you to know new interesting information");

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose, what you want to know:",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken
                    );

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
                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Enter a date:",
                    cancellationToken: cancellationToken
                    );


                Regex pattern = new Regex("[2012-2022]{1}-[1-12]{1}-[1-31]{1}");
                while (true)
                {
                    
                    Match match = pattern.Match(message.Text);
                    if (match.Success)
                    {
                        await SendAPODbyDate(message.Text, botClient, message);
                        break;
                    }
                }
                
                return;
            }
            
            
            
        }

        private Task SendAPOD(ITelegramBotClient botClient, Message message)
        {
            var apod = new NasaClient().GetAPODAsync();
            string text = "*" + apod.Result.title + "*" + "\n\n" + apod.Result.explanation + "\n" + "_" + apod.Result.date + "_";

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Close", $"11"),
                            InlineKeyboardButton.WithCallbackData( "Favourite", $"Close")

                        },
                    });

            botClient.SendPhotoAsync(
                message.Chat.Id,
                apod.Result.hdurl,
                caption: text,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
            return Task.CompletedTask; 
        }
        private Task SendAPODbyDate(string date, ITelegramBotClient botClient, Message message)
        {
            var apod = new NasaClient().GetAPODAsync(date);
            string text = "*" + apod.Result.title + "*" + "\n\n" + apod.Result.explanation + "\n" + "_" + apod.Result.date + "_";

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData( "Close", $"11"),
                            InlineKeyboardButton.WithCallbackData( "Favourite", $"Close")

                        },
                    });

            botClient.SendPhotoAsync(
                message.Chat.Id,
                apod.Result.hdurl,
                caption: text,
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
            
            return Task.CompletedTask;
        }

        private async Task SendMainMenuAsync(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
              {
                    new KeyboardButton [] {"APOD", "APODbyDate"},
                    new KeyboardButton [] {"RoverPhoto"}

                })
            {
                ResizeKeyboard = true

            };

            Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose, what you want to know:",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken
                    );
            return;
        }
    }
}
