﻿using Telegram.Bot;
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
        private static readonly Dictionary<long, UserSettings> UserSettingsDict = new();

    class UserSettings
    {
        public string Template { get; set; }
        public string Design { get; set; }
        public string QRData { get; set; }
        public byte[] ImageData { get; set; } 
    }

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

            if (!UserSettingsDict.ContainsKey(chatId))
                UserSettingsDict[chatId] = new UserSettings();

            switch (callback.Data)
            {
                case "set_template":
                    var templateKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🔗 URL", "template_url"),
                            InlineKeyboardButton.WithCallbackData("📝 Текст", "template_text")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("📶 Wi-Fi", "template_wifi"),
                            InlineKeyboardButton.WithCallbackData("👤 Контакт", "template_contact")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("📧 Email", "template_email"),
                            InlineKeyboardButton.WithCallbackData("📞 Телефон", "template_phone")
                        }
                    });
                    await botClient.SendTextMessageAsync(chatId, "Оберіть шаблон QR-коду:", replyMarkup: templateKeyboard);
                    break;

                case "set_design":
                    await botClient.SendTextMessageAsync(chatId, "🎨 Ви можете додати зображення до QR-коду. Надішліть зображення у відповідь на це повідомлення.");
                    UserSettingsDict[chatId].Design = "logo";
                    break;
                
                case "generate_qr":
                    var userSettings = UserSettingsDict[chatId];
                    if (string.IsNullOrWhiteSpace(userSettings.Template) || string.IsNullOrWhiteSpace(userSettings.QRData))
                    {
                        await botClient.SendTextMessageAsync(chatId, "❗ Спочатку оберіть шаблон та введіть дані для QR-коду.");
                        break;
                    }
                    try
                    {
                        var qrData = QRCodeGenerator.CreateQrCode(userSettings.QRData, QRCodeGenerator.ECCLevel.M);
                        using var qrCode = new PngByteQrCode(qrData);
                        var qrBytes = qrCode.GetGraphic(10);
                        
                        if (userSettings.Design == "logo" && userSettings.ImageData != null)
                        {
                            qrBytes = InsertImageQRCode.AddLogoToQrCode(qrBytes, userSettings.ImageData, 20, 20); 
                        }
                        
                        using var stream = new MemoryStream(qrBytes);
                        await botClient.SendPhotoAsync(chatId, new InputOnlineFile(stream, "qr_with_logo.png"), "✅ Ваш QR-код готовий!");
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"❗ Помилка генерації QR-коду: {ex.Message}");
                    }
                    break;
                case "template_url":
                    UserSettingsDict[chatId].Template = "url";
                    await botClient.SendTextMessageAsync(chatId, "🔗 Введіть посилання для QR-коду:");
                    break;

                case "template_text":
                    UserSettingsDict[chatId].Template = "text";
                    await botClient.SendTextMessageAsync(chatId, "📝 Введіть текст для QR-коду:");
                    break;

                case "template_wifi":
                    UserSettingsDict[chatId].Template = "wifi";
                    await botClient.SendTextMessageAsync(chatId, "📶 Введіть Wi-Fi дані (SSID;Пароль;Тип захисту):");
                    break;

                case "template_contact":
                    UserSettingsDict[chatId].Template = "contact";
                    await botClient.SendTextMessageAsync(chatId, "👤 Введіть контакт у форматі: 'Ім'я;Телефон;Email':");
                    break;

                case "template_email":
                    UserSettingsDict[chatId].Template = "email";
                    await botClient.SendTextMessageAsync(chatId, "📧 Введіть email у форматі: 'Email;Тема;Повідомлення':");
                    break;

                case "template_phone":
                    UserSettingsDict[chatId].Template = "phone";
                    await botClient.SendTextMessageAsync(chatId, "📞 Введіть номер телефону:");
                    break;
                case "add_image":
                    await botClient.SendTextMessageAsync(chatId, "🖼 Надішліть зображення, яке буде інтегроване у QR-код.");
                    UserSettingsDict[chatId].Design = "image";
                    break;
                case "scan_qr":
                    await botClient.SendTextMessageAsync(chatId, "📷 Надішліть фото QR-коду для розпізнавання.");
                    UserSettingsDict[chatId].Template = "scan";
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId, "Невідома команда.");
                    break;
            }

            await botClient.AnswerCallbackQueryAsync(callback.Id);
            return;
        }

        if (update.Message == null) return;

        var message = update.Message;
        var chatIdMessage = message.Chat.Id;

        if (!UserSettingsDict.ContainsKey(chatIdMessage))
            UserSettingsDict[chatIdMessage] = new UserSettings();

        var currentUserSettings = UserSettingsDict[chatIdMessage];

        if (message.Text == "/start")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🎨 Дизайн QR-Коду", "set_design"),
                    InlineKeyboardButton.WithCallbackData("🛠️ Шаблон QR-Коду", "set_template")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Генерувати QR-код", "generate_qr"),
                    InlineKeyboardButton.WithCallbackData("📷 Зчитати вміст QR-кода", "scan_qr"),
                }
            });

            await botClient.SendTextMessageAsync(chatIdMessage, "👋 Привіт! Оберіть дію:", replyMarkup: keyboard);
            return;
        }
        if (message.Photo != null && currentUserSettings.Template == "scan")
        {
            var photo = message.Photo.Last();
            var file = await botClient.GetFileAsync(photo.FileId, cancellationToken);

            using var ms = new MemoryStream();
            await botClient.DownloadFileAsync(file.FilePath, ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            using var skStream = new SKManagedStream(ms);
            using var bitmap = SKBitmap.Decode(skStream);

            if (bitmap == null)
            {
                await botClient.SendTextMessageAsync(chatIdMessage, "❗ Не вдалося зчитати зображення.");
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
                await botClient.SendTextMessageAsync(chatIdMessage, $"🔍 Вміст QR-коду:\n```\n{result.Text}\n```", parseMode: ParseMode.Markdown);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatIdMessage, "❌ QR-код не розпізнано.");
            }
        }
        if (message.Photo != null && currentUserSettings.Design == "logo")
        {
            try
            {
                var photo = message.Photo.Last(); 
                var file = await botClient.GetFileAsync(photo.FileId, cancellationToken);

                using var logoStream = new MemoryStream();
                await botClient.DownloadFileAsync(file.FilePath, logoStream, cancellationToken);
                currentUserSettings.ImageData = logoStream.ToArray(); 

                await botClient.SendTextMessageAsync(chatIdMessage, "✅Зображення успішно додано.");
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatIdMessage, $"❗ Помилка додавання зображення: {ex.Message}");
            }
        }
       
        if (!string.IsNullOrWhiteSpace(currentUserSettings.Template))
        {
            try
            {
                switch (currentUserSettings.Template)
                {
                    case "url":
                        if (!Uri.IsWellFormedUriString(message.Text.Trim(), UriKind.Absolute))
                            throw new Exception("❗ Неправильний формат посилання.");
                        currentUserSettings.QRData = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Дані для QR-коду збережено.");
                        break;
                    case "text":
                        currentUserSettings.QRData = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Текст для QR-коду збережено.");
                        break;
                    case "wifi":
                        var wifiParts = message.Text.Split(';');
                        if (wifiParts.Length != 3)
                            throw new Exception("❗ Формат: SSID;PASSWORD;WPA/WEP/nopass");
                        currentUserSettings.QRData = $"WIFI:T:{wifiParts[2].ToUpper()};S:{wifiParts[0]};P:{wifiParts[1]};;";
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Wi-Fi дані збережено.");
                        break;
                    case "contact":
                        var contactParts = message.Text.Split(';');
                        if (contactParts.Length != 3)
                            throw new Exception("❗ Формат: Ім'я;Телефон;Email");
                        currentUserSettings.QRData = $"MECARD:N:{contactParts[0]};TEL:{contactParts[1]};EMAIL:{contactParts[2]};;";
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Контакт збережено.");
                        break;
                    case "email":
                        var emailParts = message.Text.Split(';');
                        if (emailParts.Length != 3)
                            throw new Exception("❗ Формат: Email;Тема;Повідомлення");
                        currentUserSettings.QRData = $"mailto:{emailParts[0]}?subject={Uri.EscapeDataString(emailParts[1])}&body={Uri.EscapeDataString(emailParts[2])}";
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Дані email збережено.");
                        break;
                    case "phone":
                        currentUserSettings.QRData = $"tel:{message.Text.Trim()}";
                        await botClient.SendTextMessageAsync(chatIdMessage, "✅ Телефон збережено.");
                        break;
                    case "scan":
                        await botClient.SendTextMessageAsync(chatIdMessage, "📷 Надішліть фото для сканування.");
                        break;
                    default:
                        await botClient.SendTextMessageAsync(chatIdMessage, "❗ Невідомий шаблон.");
                        break;
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatIdMessage, ex.Message);
            }
        }
        
    }
    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Error: {exception.Message}");
        return Task.CompletedTask;
    }
}