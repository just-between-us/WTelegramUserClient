using System.Text.Json;
using SimpleTelegramClient.Models;

namespace SimpleTelegramClient.Services
{
    public class ConfigService
    {
        private const string ConfigFile = "config.json";
        
        public async Task<AppConfig> LoadOrCreateConfigAsync()
        {
            if (!File.Exists(ConfigFile)) return await CreateNewConfigAsync();
            try
            {
                var json = await File.ReadAllTextAsync(ConfigFile);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                    
                if (config is { ApiId: not null, ApiHash: not null })
                {
                    Console.WriteLine("✅ Найдена сохраненная конфигурация");
                    Console.Write("Использовать её? (y/n): ");
                        
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка чтения конфигурации: {ex.Message}");
            }
            return await CreateNewConfigAsync();
        }
        
        private async Task<AppConfig> CreateNewConfigAsync()
        {
            Console.WriteLine("\n🔧 Настройка подключения");
            Console.WriteLine("══════════════════════════");
            Console.WriteLine("Для получения API данных:");
            Console.WriteLine("1. Перейдите на https://my.telegram.org");
            Console.WriteLine("2. Войдите в свой аккаунт");
            Console.WriteLine("3. Создайте приложение в 'API Development Tools'");
            Console.WriteLine("4. Скопируйте api_id и api_hash\n");
            
            var config = new AppConfig();
            
            Console.Write("Введите API ID: ");
            config.ApiId = Console.ReadLine()?.Trim();
            
            Console.Write("Введите API Hash: ");
            config.ApiHash = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(config.ApiId) || string.IsNullOrEmpty(config.ApiHash))
            {
                throw new ArgumentException("API ID и API Hash обязательны для работы");
            }
            
            await SaveConfigAsync(config);
            return config;
        }

        private async Task SaveConfigAsync(AppConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(config, options);
                await File.WriteAllTextAsync(ConfigFile, json);
                Console.WriteLine("✅ Конфигурация сохранена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка сохранения конфигурации: {ex.Message}");
            }
        }
    }
}