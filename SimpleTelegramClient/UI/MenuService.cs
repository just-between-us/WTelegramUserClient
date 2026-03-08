using SimpleTelegramClient.Services;

namespace SimpleTelegramClient.UI
{
    public class MenuService
    {
        private readonly ConsoleUiService _ui;
        private readonly TelegramService _telegramService;
        private bool _isRunning = true;
        
        public MenuService(ConsoleUiService ui, TelegramService telegramService)
        {
            _ui = ui;
            _telegramService = telegramService;
        }
        
        public async Task RunMainMenuAsync()
        {
            Console.WriteLine("\n👂 Начинаю мониторинг новых сообщений...");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            
            while (_isRunning)
            {
                ShowMenu();
                await ProcessChoiceAsync();
            }
        }
        
        private void ShowMenu()
        {
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("Главное меню:");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("1. 📨 Показать последние сообщения");
            Console.WriteLine("2. 🔍 Поиск по контакту");
            Console.WriteLine("3. 👥 Показать все контакты");
            Console.WriteLine("4. 📊 Статистика");
            Console.WriteLine("5. 🗑️ Очистить историю");
            Console.WriteLine("6. 📗 Показать пример для запроса");
            Console.WriteLine("7. 🛑 Прекратить мониторинг ");
            Console.WriteLine("8. 👂 Запустить мониторинг");
            Console.WriteLine("9. 🚪 Выход");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            Console.Write("Выберите: ");
        }
        
        private async Task ProcessChoiceAsync()
        {
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await _ui.ShowRecentMessagesAsync();
                    break;
                case "2":
                    await _ui.SearchByContactAsync();
                    break;
                case "3":
                    await _ui.ShowContactsAsync();
                    break;
                case "4":
                    _ui.ShowStatistics();
                    break;
                case "5":
                    await _ui.AskClearHistoryAsync();
                    break;
                case "6":
                    await _ui.ShowExamples();
                    break;
                case "7":
                    _telegramService.StopMonitoring();
                    break;
                case "8":
                    _telegramService.StartMonitoring();
                    break;
                case "9":
                    _isRunning = false;
                    Console.WriteLine("\n👋 Выход...");
                    return;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    break;
            }
            
            if (_isRunning)
            {
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }
}