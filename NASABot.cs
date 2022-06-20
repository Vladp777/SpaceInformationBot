using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using NASAInformationBot.Client;

namespace NASAInformationBot
{
    public class NASABot
    {
        TelegramBotClient client = new TelegramBotClient("5569588978:AAGeC3oGohGlp3qgrqfCgrmVqqb2wesQ8fo");

        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

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
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть команду /keyboard");
                return;
            }else
            if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton [] {"/help"},
                    new KeyboardButton [] {"/start"}

                })
                {
                    ResizeKeyboard = true

                };

                Message sendMessage = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "choose",
                    replyMarkup: replyKeyboardMarkup
                    //cancellationToken: cancellationToken
                    );
                //await botClient.SendTextMessageAsync(message.Chat.Id, "choose");
                return;
            }
            else
            if (message.Text == "/inline")
            {
                InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
                            InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
                        },
                        // second row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
                            InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
                        },
                    });

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "A message with an inline keyboard markup",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
            else
                if (message.Text == "/image")
            {
                await botClient.SendPhotoAsync(message.Chat.Id, $"https://apod.nasa.gov/apod/image/e_lens.gif");
                return;
            }
            else
                if (message.Text == "/apod")
            {
                await SendAPOD(botClient, message);
                return;
            }
        }

        private Task SendAPOD(ITelegramBotClient botClient, Message message)
        {
            var apod = new APODClient().GetAPODAsync();
            string text = "*" + apod.Result.title + "*" + "\n\n" + apod.Result.explanation + "\n" + "_" + apod.Result.date + "_";

            botClient.SendPhotoAsync(
                message.Chat.Id,
                apod.Result.url,
                caption: text,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
                );

            return Task.CompletedTask;
        }
    }
}
