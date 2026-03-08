using SimpleTelegramClient.Services;
using SimpleTelegramClient.UI;
using TL;
using WTelegram;


Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "Telegram Personal Monitor";

try
{
    /*File.Delete("session.dat");
    File.Delete("WTelegram.session");*///если происходит ошибка, то при перезапуске можно попробовать удалить файлы сессии
    var configService = new ConfigService();
    var storageService = new MessageStorageService();
    var userCache = new UserCacheService();
    var me = new User();

    var config = await configService.LoadOrCreateConfigAsync();

    string? ConfigCallback(string what)
    {
        switch (what)
        {
            case "api_id": return config.ApiId;
            case "api_hash": return config.ApiHash;
            case "server_address": return "2>149.154.167.50:443";
            case "phone_number":
                Console.Write("\n📱 Введите номер телефона (например, +79998887777): ");
                return Console.ReadLine()?.Trim();
            case "verification_code":
                Console.Write("🔢 Введите код из Telegram: ");
                return Console.ReadLine()?.Trim();
            case "password":
                Console.Write("🔐 Введите пароль 2FA: ");
                return Console.ReadLine()?.Trim();
            default: return null;
        }
    }

    var client = new Client(ConfigCallback);

    Console.WriteLine("\n🔗 Подключение к Telegram...");
    try
    {
        var user = await client.LoginUserIfNeeded();
        Console.WriteLine($"✅ Подключен как: {user.first_name} {user.last_name}");

        userCache.AddUser(user);
        me = user;//Вот тут задаётся мой ID, если че-то не работает то передайте свой ID насильно
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");

        if (ex.Message.Contains("UPDATE_APP_TO_LOGIN"))
        {
            Console.WriteLine("\n⚠️ Требуется обновление клиента Telegram.");
            Console.WriteLine("Попробуйте зайти в официальное приложение Telegram и обновиться.");
        }

        return;
    }

    var telegramService = new TelegramService(client, storageService, userCache);

    var consoleUi = new ConsoleUiService(storageService);
    consoleUi.ShowWelcome();

    await telegramService.SetMeId(me);

    await telegramService.LoadRecentPersonalMessagesAsync();

    telegramService.StartMonitoring();

    telegramService.OnNewMessageReceived += (message) => { };

    var menuService = new MenuService(consoleUi, telegramService);
    await menuService.RunMainMenuAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Критическая ошибка: {ex.Message} \n {ex.StackTrace}");
    if (ex.Message ==
        "Попытка установить соединение была безуспешной, т.к. от другого компьютера за требуемое время не получен нужный отклик, или было разорвано уже установленное соединение из-за неверного отклика уже подключенного компьютера.")
    {
        Console.WriteLine(
            "Перезапустите устройство и проверьте настройки сетевого подключения;\nвозможно ошибка со стороны сервера: ping 149.154.167.50: -t");
    }

    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey();
}