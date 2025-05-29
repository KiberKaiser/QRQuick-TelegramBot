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
    private static readonly TelegramBotClient Bot = new TelegramBotClient(
        Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new InvalidOperationException("Bot token is not set.")
    );
    private static readonly Dictionary<long, UserSettings> UserSettingsDict = new();
    private static readonly Translator Translator = new Translator();
    class UserSettings
    {
        public string Template { get; set; }
        public string Design { get; set; }
        public string QRData { get; set; }
        public string QRColor { get; set; }
        public string BGColor { get; set; }
        public byte[] ImageData { get; set; }
        public string Language { get; set; } = "ua";
    }

    static async Task Main()
    {
        Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        Console.WriteLine("Бот запущений...");
        await Task.Delay(-1);
    }
    private static async Task SendMainMenu(long chatId, UserSettings userSettings)
    {
        var menuKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Translator.Translate("🎨 Дизайн QR-коду", userSettings.Language), "set_design"),
                InlineKeyboardButton.WithCallbackData(Translator.Translate("🛠 Шаблон QR-коду", userSettings.Language), "set_template")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Translator.Translate("✅ Згенерувати QR-код", userSettings.Language), "generate_qr"),
                InlineKeyboardButton.WithCallbackData(Translator.Translate("📷 Декодувати QR-код", userSettings.Language), "scan_qr"),
            }
        });
    }
    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery != null)
        {
            
            var callback = update.CallbackQuery;
            var chatId = callback.Message.Chat.Id;

            if (!UserSettingsDict.ContainsKey(chatId))
                UserSettingsDict[chatId] = new UserSettings();

            var userSettings = UserSettingsDict[chatId];
            
        
            switch (callback.Data)
            {
                case "lang_en":
                    userSettings.Language = "en"; 
                    await botClient.SendTextMessageAsync(chatId,
                        "👋 Welcome to the QRCodeQuick! \n\n" +
                        "With this bot you can: \n" +
                        "• Generate custom QR codes from templates (URL, text, Wi-Fi, contacts, etc.)\n" +
                        "• Decode QR codes to get their content \n" +
                        "• Customize the design of the QR code (background and QR code colors, inserting an image)\n\n" +
                        "To get started with the Telegram bot, enter the /menu command, which will open the menu with QR code settings" );
                    break;
                case "lang_ua":
                    userSettings.Language = "ua";
                    await botClient.SendTextMessageAsync(chatId,
                        "👋 Ласкаво просимо до телеграм-бота QRСodeQuick!\n\n" +
                        "За допомогою цього бота ви можете:\n" +
                        "• Генерувати кастомні QR-коди за шаблонами (URL, текст, Wi-Fi, контакти тощо.)\n" +
                        "• Декодувати QR-коди для отримання їх змісту \n" +
                        "• Налаштовувати дизайн QR-коду (кольори фону та QR-коду, вставка зоображення)\n\n" +
                        "Щоб почати роботу з телеграм-ботом, введіть команду /menu, яка відкриє меню з налаштуванням QR-коду" );
                    break;

                case "set_template":
                    var templateKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🔗 URL", "template_url"),
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("📝 Текст", userSettings.Language),"template_text")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("📶 Wi-Fi", "template_wifi"),
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("👤 Контакт", userSettings.Language), "template_contact")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("📧 Email", "template_email"),
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("📞 Телефон", userSettings.Language), "template_phone")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("🗺 Геолокація", userSettings.Language), "template_geo")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("▶️ Назад до меню", userSettings.Language), "back_to_menu")
                        }
                    });
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("Оберіть шаблон QR-коду:", userSettings.Language),replyMarkup: templateKeyboard);
                    break;
            
                case "set_design":
                    var designKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("🖼 Додати зображення", userSettings.Language), "add_image"),
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("🎨 Зміна кольору QR-коду", userSettings.Language), "change_qr_color")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("🌈 Зміна кольору фону", userSettings.Language), "change_bg_color"),
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("🔄 Скинути всі параметри", userSettings.Language), "reset_design")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(Translator.Translate("▶️ Назад до меню", userSettings.Language), "back_to_menu")
                        }
                    });
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🎨 Оберіть опцію дизайну QR-коду:",userSettings.Language), replyMarkup: designKeyboard);
                    break; 

                case "back_to_menu":
                    await SendMainMenu(chatId, userSettings);
                    break;   

                case "generate_qr":
                    if (string.IsNullOrWhiteSpace(userSettings.Template) || string.IsNullOrWhiteSpace(userSettings.QRData))
                    {
                        await botClient.SendTextMessageAsync(chatId, Translator.Translate("❗ Спочатку оберіть шаблон та введіть дані для QR-коду.", userSettings.Language));
                        break;
                    }
                    try
                    {
                        var qrData = QRCodeGenerator.CreateQrCode(userSettings.QRData, QRCodeGenerator.ECCLevel.M);
                        using var qrCode = new PngByteQrCode(qrData);
                        var qrBytes = qrCode.GetGraphic(10);

                        using var ms = new MemoryStream(qrBytes);
                        var originalBitmap = SKBitmap.Decode(ms);

                        var colorChanger = new ChangeQRCodeColor();
                        var foregroundColor = string.IsNullOrWhiteSpace(userSettings.QRColor)
                            ? SKColors.Black
                            : SKColor.Parse(userSettings.QRColor);
                        var backgroundColor = string.IsNullOrWhiteSpace(userSettings.BGColor)
                            ? SKColors.White
                            : SKColor.Parse(userSettings.BGColor);

                        var modifiedBitmap = colorChanger.ChangeColors(originalBitmap, foregroundColor, backgroundColor);

                        using var imageStream = new MemoryStream();
                        using (var skStream = new SKManagedWStream(imageStream))
                        {
                            modifiedBitmap.Encode(skStream, SKEncodedImageFormat.Png, 100);
                        }

                        if (userSettings.ImageData != null)
                        {
                            qrBytes = InsertImageQRCode.AddLogoToQrCode(imageStream.ToArray(), userSettings.ImageData, 20, 20);
                        }
                        else
                        {
                            qrBytes = imageStream.ToArray();
                        }

                        using var resultStream = new MemoryStream(qrBytes);
                        await botClient.SendPhotoAsync(chatId, new InputOnlineFile(resultStream, "qr_with_logo.png"), Translator.Translate("✅ Ваш QR-код готовий!", userSettings.Language));
                        await SendMainMenu(chatId, userSettings);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendTextMessageAsync(chatId, Translator.Translate($"❗ Помилка генерації QR-коду: {ex.Message}", userSettings.Language));
                        await SendMainMenu(chatId, userSettings);
                    }
                    break;

                case "template_url":
                    UserSettingsDict[chatId].Template = "url";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🔗 Введіть посилання для QR-коду:", userSettings.Language));
                    break;
                case "template_text":
                    UserSettingsDict[chatId].Template = "text";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📝 Введіть текст для QR-коду:", userSettings.Language));
                    break;
                case "template_wifi":
                    UserSettingsDict[chatId].Template = "wifi";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📶 Введіть Wi-Fi дані (SSID;Пароль;Тип захисту):", userSettings.Language));
                    break;
                case "template_contact":
                    UserSettingsDict[chatId].Template = "contact";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("👤 Введіть контакт у форматі: 'Ім'я;Телефон;Email':", userSettings.Language));
                    break;
                case "template_email":
                    UserSettingsDict[chatId].Template = "email";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📧 Введіть email у форматі: 'Email;Тема;Повідомлення':", userSettings.Language));
                    break;
                case "template_phone":
                    UserSettingsDict[chatId].Template = "phone";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📞 Введіть номер телефону:", userSettings.Language));
                    break;
                case "template_geo":
                    UserSettingsDict[chatId].Template = "geo";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📍 Введіть координати у форматі: 'Широта,Довгота' (наприклад, 48.8588443,2.2943506).", userSettings.Language));
                    break;
                case "add_image":
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🖼 Надішліть зображення, яке буде інтегроване у QR-код.", userSettings.Language));
                    UserSettingsDict[chatId].Design = "logo";
                    break;
                case "change_qr_color":
                    UserSettingsDict[chatId].Design = "qr_color";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🎨 Введіть колір QR-коду у форматі HEX (наприклад, #000000):", userSettings.Language));
                    break;
                case "change_bg_color":
                    UserSettingsDict[chatId].Design = "bg_color";
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🎨 Введіть колір фону у форматі HEX (наприклад, #FFFFFF):", userSettings.Language));
                    break;
                case "reset_design":
                    UserSettingsDict[chatId].QRColor = null;
                    UserSettingsDict[chatId].BGColor = null;
                    UserSettingsDict[chatId].ImageData = null;
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("🔄 Усі параметри дизайну скинуто!", userSettings.Language));
                    await SendMainMenu(chatId, userSettings);
                    break;
                case "scan_qr":
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("📷 Надішліть фото QR-коду для розпізнавання.", userSettings.Language));
                    UserSettingsDict[chatId].Template = "scan";
                    break;
                default: 
                    await botClient.SendTextMessageAsync(chatId, Translator.Translate("Невідома команда", userSettings.Language));
                    await SendMainMenu(chatId, userSettings);
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
            var languageKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("🇬🇧 English", "lang_en"),
                    InlineKeyboardButton.WithCallbackData("🇺🇦 Українська", "lang_ua"),
                }
            });
            await botClient.SendTextMessageAsync(chatIdMessage, "Сhoose your language / Оберіть мову: ", replyMarkup: languageKeyboard);
        }
        
        if (message.Text == "/menu")
        {
            await SendMainMenu(chatIdMessage, currentUserSettings);
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
                await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("❗ Не вдалося зчитати зображення.", currentUserSettings.Language));
                await SendMainMenu(chatIdMessage, currentUserSettings);
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
                var translatedMessage = Translator.Translate("🔍 Вміст QR-коду:", currentUserSettings.Language) + $"\n```\n{result.Text}\n```";
                await botClient.SendTextMessageAsync(chatIdMessage, translatedMessage, parseMode: ParseMode.Markdown);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("❌ QR-код не розпізнано.", currentUserSettings.Language));
                
            }
            currentUserSettings.Template = string.Empty;
            await SendMainMenu(chatIdMessage, currentUserSettings);
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

                await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Зображення успішно додано.", currentUserSettings.Language));
                currentUserSettings.Design = string.Empty;  
                await SendMainMenu(chatIdMessage, currentUserSettings);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate($"❗ Помилка додавання зображення: {ex.Message}", currentUserSettings.Language));
                currentUserSettings.Design = string.Empty;
                await SendMainMenu(chatIdMessage, currentUserSettings);
            }
        }
    
        if (!string.IsNullOrWhiteSpace(currentUserSettings.Design))
        {
            switch(currentUserSettings.Design)
            {
                case "qr_color":
                    if (!message.Text.StartsWith("#") || message.Text.Length != 7)
                    {
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("❗ Неправильний формат кольору. Використовуйте формат HEX(наприклад: #FFFFFF).", currentUserSettings.Language));
                    }
                    else
                    {
                        currentUserSettings.QRColor = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Колір QR-коду успішно змінено!", currentUserSettings.Language));
                    }
                    break;

                case "bg_color":
                    if (!message.Text.StartsWith("#") || message.Text.Length != 7)
                    {
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("❗ Неправильний формат кольору. Використовуйте формат HEX (наприклад: #000000).", currentUserSettings.Language));
                    }
                    else
                    {
                        currentUserSettings.BGColor = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Колір фону QR-коду успішно змінено.", currentUserSettings.Language));
                    }
                    break;
                default:
                await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("❗ Невідома команда для дизайну.", currentUserSettings.Language));
                break;
            }
            currentUserSettings.Design = string.Empty;
            await SendMainMenu(chatIdMessage, currentUserSettings);
        }
        else if (!string.IsNullOrWhiteSpace(currentUserSettings.Template))
        {
            try
            {
                switch (currentUserSettings.Template)
                {
                    case "url":
                        if (!Uri.IsWellFormedUriString(message.Text.Trim(), UriKind.Absolute))
                            throw new Exception(Translator.Translate("❗ Неправильний формат посилання.", currentUserSettings.Language));
                        currentUserSettings.QRData = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Посилання для QR-коду збережено.", currentUserSettings.Language));
                        break;
                    case "text":
                        var urlRegex = @"(http(s)?://|www\.)\S+"; 
                        if (System.Text.RegularExpressions.Regex.IsMatch(message.Text.Trim(), urlRegex, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            throw new Exception(Translator.Translate("❗ Текст не може містити посилання або URL-адреси.", currentUserSettings.Language));
                        currentUserSettings.QRData = message.Text.Trim();
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Текст для QR-коду збережено.", currentUserSettings.Language));
                        break;
                    case "wifi":
                        var wifiParts = message.Text.Split(';');
                        if (wifiParts.Length != 3)
                            throw new Exception(Translator.Translate("❗ Формат: SSID;PASSWORD;WPA/WEP/nopass", currentUserSettings.Language));
                        currentUserSettings.QRData = $"WIFI:T:{wifiParts[2].ToUpper()};S:{wifiParts[0]};P:{wifiParts[1]};;";
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Wi-Fi дані збережено.", currentUserSettings.Language));
                        break;
                    case "contact":
                        var contactParts = message.Text.Split(';');
                        if (contactParts.Length != 3)
                            throw new Exception(Translator.Translate("❗ Формат: Ім'я;Телефон;Email", currentUserSettings.Language));
                        currentUserSettings.QRData = $"MECARD:N:{contactParts[0]};TEL:{contactParts[1]};EMAIL:{contactParts[2]};;";
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Контакт збережено.", currentUserSettings.Language));
                        break;
                    case "email":
                        var emailParts = message.Text.Split(';');
                        if (emailParts.Length != 3)
                            throw new Exception(Translator.Translate("❗ Формат: Email;Тема;Повідомлення", currentUserSettings.Language));
                        currentUserSettings.QRData = $"mailto:{emailParts[0]}?subject={Uri.EscapeDataString(emailParts[1])}&body={Uri.EscapeDataString(emailParts[2])}";
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Дані електронної пошти збережені.", currentUserSettings.Language));
                        break;
                    case "phone":
                        var phoneRegex = @"^\+?\d{10,15}$";
                        if (!System.Text.RegularExpressions.Regex.IsMatch(message.Text.Trim(), phoneRegex))
                            throw new Exception(Translator.Translate("❗ Невірний формат номера телефону. Використовуйте міжнародний формат, наприклад: +380474747474.", currentUserSettings.Language));
                        currentUserSettings.QRData = $"tel:{message.Text.Trim()}";
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Дані номеру телефона збережено.", currentUserSettings.Language));
                        break;
                    case "geo":
                        var geoParts = message.Text.Split(',');
                        if (geoParts.Length != 2 || !double.TryParse(geoParts[0], out _) || !double.TryParse(geoParts[1], out _))
                            throw new Exception(Translator.Translate("❗ Формат: Широта,Довгота (наприклад: 48.8588443,2.2943506)", currentUserSettings.Language));
                        currentUserSettings.QRData = $"geo:{geoParts[0]},{geoParts[1]}";
                        await botClient.SendTextMessageAsync(chatIdMessage, Translator.Translate("✅ Дані геолокації збережено.", currentUserSettings.Language));
                        break;
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatIdMessage, ex.Message);
                await SendMainMenu(chatIdMessage, currentUserSettings);
            }
        }
    }
    
    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Error: {exception.Message}");
        return Task.CompletedTask;
    }
}