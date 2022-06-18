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

namespace NASAInformationBot
{
    public class NASAInformationBot
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
            }
            else
                if (message.Text == "/image")
            {
                await botClient.SendPhotoAsync(message.Chat.Id, $"https://apod.nasa.gov/apod/image/e_lens.gif");
                return;
            }
        }
    }
}
