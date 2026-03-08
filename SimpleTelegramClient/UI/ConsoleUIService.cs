using SimpleTelegramClient.Services;
using TelegramClient.Utils;

namespace SimpleTelegramClient.UI
{
    public class ConsoleUiService
    {
        private readonly MessageStorageService _storage;
        
        public ConsoleUiService(MessageStorageService storage)
        {
            _storage = storage;
        }
        
        public void ShowWelcome()
        {
            Console.Clear();
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine("      Telegram Personal Monitor");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine();
        }
        
        public Task ShowRecentMessagesAsync()
        {
            Console.WriteLine("\n📨 Последние сообщения:");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            
            var messages = _storage.GetMessages().Take(20);

            var messageRecords = messages.ToList();
            if (!messageRecords.Any())
            {
                Console.WriteLine("Сообщений нет");
                return Task.CompletedTask;
            }
            
            foreach (var msg in messageRecords)
            {
                var direction = msg.IsOutgoing ? "Я →" : "Мне ←";
                Console.WriteLine($" [{msg.Time:HH:mm:ss}] {direction} {msg.ContactName}:");
                Console.WriteLine($" {StringHelper.Truncate(msg.Text, 100)}");
                Console.WriteLine();
            }

            return Task.CompletedTask;
        }
        public Task ShowExamples()
        {
            var examples = _storage.GetExamples();
            if (!string.IsNullOrEmpty(examples))
            {
                Console.WriteLine("\n📊 Примеры для запроса генерируются как:");
                Console.WriteLine("════════════════════════════════════════════════════════════");
                Console.WriteLine(examples);
            }
            else
            {
                Console.WriteLine("Примеров для запроса нет");
            }
            return Task.CompletedTask;
        }
        public Task SearchByContactAsync()
        {
            Console.Write("\n🔍 Введите имя контакта для поиска: ");
            var name = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("❌ Имя не может быть пустым");
                return Task.CompletedTask;
            }
            
            var found = _storage.GetMessages(name);
            
            Console.WriteLine($"\nНайдено {found.Count} сообщений:");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            
            if (found.Count == 0)
            {
                Console.WriteLine("Сообщений не найдено");
                return Task.CompletedTask;
            }
            
            foreach (var msg in found.Take(15))
            {
                var direction = msg.IsOutgoing ? "→" : "←";
                Console.WriteLine($"[{msg.Time:HH:mm:ss}] {direction} {msg.ContactName}: {StringHelper.Truncate(msg.Text, 80)}");
            }

            return Task.CompletedTask;
        }
        
        public Task ShowContactsAsync()
        {
            var contacts = _storage.GetContacts();
            
            Console.WriteLine($"\n👥 Контакты ({contacts.Count}):");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            
            if (contacts.Count == 0)
            {
                Console.WriteLine("Контактов нет");
                return Task.CompletedTask;
            }
            
            foreach (var contact in contacts)
            {
                var contactMessages = _storage.GetMessages(contact);
                var incoming = contactMessages.Count(m => !m.IsOutgoing);
                var outgoing = contactMessages.Count(m => m.IsOutgoing);
                
                Console.WriteLine($"• {contact}");
                Console.WriteLine($"  Всего: {contactMessages.Count}, Входящих: {incoming}, Исходящих: {outgoing}");
            }

            return Task.CompletedTask;
        }
        
        public void ShowStatistics()
        {
            var messages = _storage.GetMessages();
            
            Console.WriteLine("\n📊 Статистика:");
            Console.WriteLine("════════════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine($"Всего сообщений: {messages.Count}");
            Console.WriteLine($"Входящих: {messages.Count(m => !m.IsOutgoing)}");
            Console.WriteLine($"Исходящих: {messages.Count(m => m.IsOutgoing)}");
            Console.WriteLine($"Контактов: {messages.Select(m => m.ContactName).Distinct().Count()}");
            
            if (messages.Count > 0)
            {
                var firstMessage = messages.OrderBy(m => m.Time).FirstOrDefault();
                var lastMessage = messages.OrderByDescending(m => m.Time).FirstOrDefault();
                
                Console.WriteLine($"Первое сообщение: {firstMessage?.Time:g}");
                Console.WriteLine($"Последнее сообщение: {lastMessage?.Time:g}");
            }
        }
        
        public Task<bool> AskClearHistoryAsync()
        {
            Console.Write("\n⚠️  Вы уверены, что хотите удалить всю историю сообщений? (y/n): ");
            var confirm = Console.ReadLine()?.ToLower();
            
            if (confirm == "y")
            {
                _storage.Clear();
                Console.WriteLine("✅ История сообщений удалена");
                return Task.FromResult(true);
            }
            
            Console.WriteLine("❌ Удаление отменено");
            return Task.FromResult(false);
        }
    }
}