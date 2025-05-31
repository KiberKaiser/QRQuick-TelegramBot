public class Translator
{
    private readonly Dictionary<string, string> _translations = new()
    {
        {"Ласкаво просимо! Використовуйте команду /menu для списку доступних опцій.", "Welcome! Use /menu to see available options."},
        {"Оберіть шаблон QR-коду:", "Choose a QR code template:"},
        {"🎨 Оберіть опцію дизайну QR-коду:", "🎨 Choose a QR code design option:"},
        {"❗ Спочатку оберіть шаблон та введіть дані для QR-коду.", "❗ First choose a template and enter data for the QR code."},
        {"✅ Ваш QR-код готовий!", "✅ Your QR code is ready!"},
        {"❗ Помилка генерації QR-коду: {ex.Message}", "❗ QR code generation error: {ex.Message}"},
        {"🔗 Введіть посилання для QR-коду:", "🔗 Enter a link for the QR code:"},
        {"📝 Введіть текст для QR-коду:", "📝 Enter text for the QR code:"},
        {"📶 Введіть Wi-Fi дані (SSID;Пароль;Тип захисту):", "📶 Enter Wi-Fi data (SSID;Password;Security Type):"},
        {"👤 Введіть контакт у форматі: 'Ім'я;Телефон;Email':", "👤 Enter contact in format: 'Name;Phone;Email':"},
        {"📧 Введіть email у форматі: 'Email;Тема;Повідомлення':", "📧 Enter email in format: 'Email;Subject;Message':"},
        {"📞 Введіть номер телефону:", "📞 Enter phone number:"},
        {"📍 Введіть координати у форматі: 'Широта,Довгота' (наприклад, 48.8588443,2.2943506).", "📍 Enter coordinates in format: 'Latitude,Longitude' (e.g., 48.8588443,2.2943506)."},
        {"🖼 Надішліть зображення, яке буде інтегроване у QR-код.", "🖼 Send an image to be embedded into the QR code."},
        {"🎨 Введіть колір QR-коду у форматі HEX (наприклад, #000000):", "🎨 Enter QR code color in HEX format (e.g., #000000):"},
        {"🎨 Введіть колір фону у форматі HEX (наприклад, #FFFFFF):", "🎨 Enter background color in HEX format (e.g., #FFFFFF):"},
        {"🔄 Усі параметри дизайну скинуто!", "🔄 All design parameters have been reset!"},
        {"🔄 Шаблон та його зміст скинуто!", "🔄 The template and its content have been reset!"},
        {"📷 Надішліть фото QR-коду для розпізнавання.", "📷 Send a QR code image to decode."},
        {"📋 Оберіть дію:", "📋 Choose an action:"},
        {"📝 Текст", "📋 Text"},
        {"👤 Контакт", "👤 Contact"},
        {"📞 Телефон", "📞 Phone"},
        {"🗺 Геолокація", "🗺 Geolocation"},
        {"🖼 Додати зображення", "🖼 Add image"},
        {"🎨 Зміна кольору QR-коду", "🎨 Change QR code color"},
        {"🌈 Зміна кольору фону", "🌈 Change background color"},
        {"🔄 Скинути всі параметри дизайну", "🔄 Reset all design parameters"},
        {"🔄 Скинути шаблон і зміст", "🔄 Reset template and content"},
        {"▶️ Назад до меню", "▶️ Back to the menu"},
        {"🎨 Дизайн QR-коду", "🎨 Design QR Code"},
        {"🛠 Шаблон QR-коду", "🛠 Template QR Code"},
        {"✅ Згенерувати QR-код", "✅ Generate QR Code"},
        {"📷 Декодувати QR-код", "📷 Decode QR Code"},
        {"❗ Не вдалося зчитати зображення.", "❗ Failed to read the image."},
        {"🔍 Вміст QR-коду:", "🔍 QR code content:"},
        {"❌ QR-код не розпізнано.", "❌ QR code not recognized."},
        {"✅ Зображення успішно додано.", "✅ Image added successfully."},
        {"❗ Помилка додавання зображення: {ex.Message}", "❗ Error adding image: {ex.Message}"},
        {"❗ Неправильний формат кольору. Використовуйте формат HEX(наприклад: #FFFFFF).", "❗ Invalid color format. Use HEX format (e.g., #FFFFFF)."},
        {"❗ Неправильний формат кольору. Використовуйте формат HEX(наприклад: #000000).", "❗ Invalid color format. Use HEX format (e.g., #000000)."},
        {"✅ Колір QR-коду успішно змінено!", "✅ QR code color updated successfully!"},
        {"✅ Колір фону QR-коду успішно змінено.", "✅ Background color updated successfully."},
        {"❗ Невідома команда для дизайну.", "❗ Unknown design command."},
        {"❗ Неправильний формат посилання.", "❗ Invalid URL format."},
        {"✅ Посилання для QR-коду збережено.", "✅ Link for QR code saved."},
        {"❗ Текст не може містити посилання або URL-адреси.", "❗ Text cannot contain links or URLs."},
        {"✅ Текст для QR-коду збережено.", "✅ Text for QR code saved."},
        {"❗ Формат: SSID;PASSWORD;WPA/WEP/nopass", "❗ Format: SSID;PASSWORD;WPA/WEP/nopass"},
        {"✅ Wi-Fi дані збережено.", "✅ Wi-Fi data saved."},
        {"❗ Формат: Ім'я;Телефон;Email", "❗ Format: Name;Phone;Email"},
        {"✅ Дані електронної пошти збережені.", "Email data saved."},
        {"✅ Контакт збережено.", "✅ Contact saved."},
        {"❗ Формат: Email;Тема;Повідомлення", "❗ Format: Email;Subject;Message"},
        {"✅ Email збережено.", "✅ Email saved."},
        {"✅ Телефон збережено.", "✅ Phone number saved."},
        {"❗ Формат: Широта,Довгота", "❗ Format: Latitude,Longitude"},
        {"✅ Геолокацію збережено.", "✅ Geolocation saved."},
        {"❗ Невідома команда", "❗ Unknown command"},
    };

    public string Translate(string text, string languageCode)
    {
        text = text.Trim();

        if (languageCode == "en")
        {
            if (!_translations.TryGetValue(text, out var translation))
            {
                Console.WriteLine($"[Translate] No translation found for: '{text}'");
                return text;
            }

            return translation;
        }
        else
        {
        var entry = _translations.FirstOrDefault(x => x.Value == text);
            if (entry.Key == null)
            {
                Console.WriteLine($"[Translate] No translation found for: '{text}'");
                return text;
            }

            return entry.Key;
        }
    }
}