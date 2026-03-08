using System.Text;
using System.Text.Json;
using SimpleTelegramClient.Models;

namespace SimpleTelegramClient.Services
{
     public class MessageStorageService
    {
        private const string MessagesFile = "messages.json";
        private List<MessageRecord> _messages = new();
        private readonly object _lock = new();
        
        public MessageStorageService()
        {
            Console.WriteLine($"📂 Инициализация хранилища. Файл: {MessagesFile}");
            LoadMessages();
            Console.WriteLine($"📂 Загружено {_messages.Count} из файла");
        }
        
        public void AddMessage(MessageRecord message)
        {
            //Console.WriteLine($"📼 Попытка сохранить сообщение {message.Id} от {message.ContactName}");
            lock (_lock)
            {
                var isDuplicate = _messages.Any(m =>
                    m.Id == message.Id &&
                    m.PeerUserId == message.PeerUserId &&
                    m.ContactName == message.ContactName);
                
                if (!isDuplicate && message.Text != "")
                {
                    _messages.Add(message);
                    SaveMessages();
                    //Console.WriteLine($"✅ Сохранено {message.ContactName}: {StringHelper.Truncate(message.Text, 100)}"); 
                }
                else
                {
                    Console.WriteLine($"⚠️ Пропуск дубликата сообщения {message.Id}");
                }
            }
        }
        
        public List<MessageRecord> GetMessages(string? contactName = null)
        {
            lock (_lock)
            {
                var query = _messages.AsEnumerable();
                
                if (!string.IsNullOrEmpty(contactName))
                {
                    query = query.Where(m => 
                        m.ContactName.Contains(contactName, StringComparison.OrdinalIgnoreCase));
                }
                return query.OrderByDescending(m => m.Time).ToList();
            }
        }
        
        public List<string> GetContacts()
        {
            lock (_lock)
            {
                return _messages
                    .Select(m => m.ContactName)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
        }
        
        public void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
                SaveMessages();
            }
        }
        
        private void LoadMessages()
        {
            if (!File.Exists(MessagesFile))
            {
                SaveMessages();
            }
            try
            {
                var json = File.ReadAllText(MessagesFile);
                var messages = JsonSerializer.Deserialize<List<MessageRecord>>(json);
                if (messages != null)
                {
                    _messages = messages;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Ошибка загрузки сообщений: {ex.Message}");
            }
        }
        
        
        private void SaveMessages()
        {
            try
            {
                List<MessageRecord> messagesCopy;
                lock (_lock) { messagesCopy = _messages.ToList(); }
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(messagesCopy, options);
                File.WriteAllText(MessagesFile, json);
                Console.WriteLine($"📂 Сообщения записаны и сохранены: по пути {MessagesFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка сохранения сообщений: {ex.Message}");
            }
        }
        public string GetExamples( int totalLimit = 200)
        {
            lock (_lock)
            {
                try
                {
                    var sorted = _messages
                        .OrderByDescending(m => m.Time)
                        .Take(totalLimit)
                        .OrderBy(m => m.Time)
                        .ToList();

                    var sb = new StringBuilder();
                    foreach (var m in sorted.Where(m => m.ContactName != "Telegram" && m.ContactName != "Фек"))
                    {
                        if (m.ContactName == "Bodya" || m.Text.StartsWith("!"))
                        {
                            continue;
                        }
                        sb.AppendLine($"{m.ContactName}: {m.Text}");
                    }

                    Console.WriteLine($"📦 Глобальный контекст собран: {sorted.Count} сообщений");

                    return sb.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка загрузки глобального контекста: {ex.Message}");
                    return string.Empty;
                }
            }
        }
    }
}