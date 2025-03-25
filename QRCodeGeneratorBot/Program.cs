using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

class Program
{
    private static readonly TelegramBotClient Bot = new TelegramBotClient("NUMBER_TOKEN");

    static async Task Main()
    {
        Bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandleErrorAsync
        );
        Console.WriteLine("Бот запущений...");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text == null) return;

        var message = update.Message;

        if (message.Text == "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привіт! Надішли мені текст, щоб я згенерував QR-код.",
                cancellationToken: cancellationToken
            );
            return; 
        }

        var qrCodeData = QRCodeGenerator.CreateQrCode(message.Text, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQrCode(qrCodeData);

        var qrCodeImageBytes = qrCode.GetGraphic(10, new byte[] { 0, 0, 0, 255 }, new byte[] { 255, 255, 255, 255 });

        using var stream = new MemoryStream(qrCodeImageBytes);
        await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            photo: new InputOnlineFile(stream, "qrcode.png"),
            caption: "Ось ваш QR-код!",
            cancellationToken: cancellationToken
        );
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}