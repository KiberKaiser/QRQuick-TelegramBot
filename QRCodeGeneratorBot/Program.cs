using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using ZXing;
using ZXing.SkiaSharp;
using SkiaSharp;

class Program
{
    private static readonly TelegramBotClient Bot = new TelegramBotClient("NUMBER_TOKEN");
    private static readonly Dictionary<long, string> UserContext = new();

    static async Task Main()
    {
        Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        Console.WriteLine("Бот запущений...");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery != null)
        {
            var callback = update.CallbackQuery;
            var chatId = callback.Message.Chat.Id;

            switch (callback.Data)
            {
                case "qr_url":
                    await botClient.SendTextMessageAsync(chatId, "🔗 Введіть посилання для QR-коду:");
                    UserContext[chatId] = "url";
                    break;
                case "qr_wifi":
                    await botClient.SendTextMessageAsync(chatId, "📶 Введіть Wi-Fi дані у форматі:\n`SSID;PASSWORD;WPA/WEP/nopass`", parseMode: ParseMode.Markdown);
                    UserContext[chatId] = "wifi";
                    break;
                case "qr_text":
                    await botClient.SendTextMessageAsync(chatId, "📝 Введіть текст для QR-коду:");
                    UserContext[chatId] = "text";
                    break;
                case "qr_contact":
                    await botClient.SendTextMessageAsync(chatId, "👤 Введіть контакт у форматі:\n`Ім'я;Телефон;Email`");
                    UserContext[chatId] = "contact";
                    break;
                case "qr_email":
                    await botClient.SendTextMessageAsync(chatId, "📧 Введіть email у форматі:\n`email@example.com;Тема;Повідомлення`");
                    UserContext[chatId] = "email";
                    break;
                case "qr_phone":
                    await botClient.SendTextMessageAsync(chatId, "📞 Введіть номер телефону (наприклад, +380991234567):");
                    UserContext[chatId] = "phone";
                    break;
                case "qr_scan":
                    await botClient.SendTextMessageAsync(chatId, "📷 Надішліть фото QR-коду у відповідь на це повідомлення.");
                    UserContext[chatId] = "scan";
                    break;
            }
            await botClient.AnswerCallbackQueryAsync(callback.Id);
            return;
        }

        if (update.Message == null) return;

        var message = update.Message;
        var chatIdText = message.Chat.Id;
        
        if (message.Photo != null)
        {
            if (!UserContext.TryGetValue(chatIdText, out var context) || context != "scan")
            {
                return;
            }

            var photo = message.Photo.Last();
            var file = await botClient.GetFileAsync(photo.FileId, cancellationToken);
            using var ms = new MemoryStream();
            await botClient.DownloadFileAsync(file.FilePath, ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            using var skStream = new SKManagedStream(ms);
            using var bitmap = SKBitmap.Decode(skStream);

            if (bitmap == null)
            {
                await botClient.SendTextMessageAsync(chatIdText, "❗ Не вдалося зчитати зображення.");
                return;
            }

            var reader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(bitmap);
            if (result != null)
            {
                await botClient.SendTextMessageAsync(chatIdText, $"🔍 Вміст QR-коду:\n```\n{result.Text}\n```", parseMode: ParseMode.Markdown);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatIdText, "❌ QR-код не розпізнано.");
            }

            UserContext.Remove(chatIdText);
            return;
        }
        
        if (message.Text == "/start")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🌐 URL", "qr_url"),
                    InlineKeyboardButton.WithCallbackData("📶 Wi-Fi", "qr_wifi"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📝 Текст", "qr_text"),
                    InlineKeyboardButton.WithCallbackData("👤 Контакт", "qr_contact"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📧 Email", "qr_email"),
                    InlineKeyboardButton.WithCallbackData("📞 Телефон", "qr_phone")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📷 Сканувати QR", "qr_scan"),
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: chatIdText,
                text: "👋 Привіт! Я бот для генерації QR-кодів.\nОберіть дію нижче:",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );

            return;
        }

        if (message.Text != null && UserContext.TryGetValue(chatIdText, out var contextText))
        {
            if (contextText == "scan")
            {
                await botClient.SendTextMessageAsync(chatIdText, "❗ Надішліть, будь ласка, зображення з QR-кодом.");
                return;
            }

            string qrContent = message.Text.Trim();
            try
            {
                switch (contextText)
                {
                    case "wifi":
                        var wifi = qrContent.Split(';');
                        if (wifi.Length != 3) throw new Exception("❗ Формат невірний. Має бути: SSID;PASSWORD;WPA/WEP/nopass");
                        qrContent = $"WIFI:T:{wifi[2].ToUpper()};S:{wifi[0]};P:{wifi[1]};;";
                        break;
                    case "contact":
                        var contact = qrContent.Split(';');
                        if (contact.Length != 3) throw new Exception("❗ Формат невірний. Ім'я;Телефон;Email");
                        qrContent = $"MECARD:N:{contact[0]};TEL:{contact[1]};EMAIL:{contact[2]};;";
                        break;
                    case "email":
                        var email = qrContent.Split(';');
                        if (email.Length != 3) throw new Exception("❗ Формат: email;тема;повідомлення");
                        qrContent = $"mailto:{email[0]}?subject={Uri.EscapeDataString(email[1])}&body={Uri.EscapeDataString(email[2])}";
                        break;
                    case "phone":
                        qrContent = $"tel:{qrContent}";
                        break;
                }

                var qrData = QRCodeGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.M);
                using var qrCode = new PngByteQrCode(qrData);
                var qrBytes = qrCode.GetGraphic(10);

                using var stream = new MemoryStream(qrBytes);
                await botClient.SendPhotoAsync(chatId: chatIdText, photo: new InputOnlineFile(stream, "qr.png"), caption: "✅ Ваш QR-код готовий!", cancellationToken: cancellationToken);
                UserContext.Remove(chatIdText);
                return;
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatIdText, ex.Message);
                return;
            }
        }
        
        if (message.Text != null)
        {
            var fallbackData = QRCodeGenerator.CreateQrCode(message.Text, QRCodeGenerator.ECCLevel.M);
            using var fallbackCode = new PngByteQrCode(fallbackData);
            var fallbackBytes = fallbackCode.GetGraphic(10);

            using var fallbackStream = new MemoryStream(fallbackBytes);
            await botClient.SendPhotoAsync(chatId: chatIdText, photo: new InputOnlineFile(fallbackStream, "qrcode.png"), caption: "📦 Ось ваш QR-код!", cancellationToken: cancellationToken);
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
