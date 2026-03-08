using SimpleTelegramClient.Models;
using TL;
using WTelegram;
using Message = TL.Message;


namespace SimpleTelegramClient.Services
{
    public class TelegramService(Client client, MessageStorageService storage, UserCacheService userCache)
        : IDisposable
    {
        private long _myUserId;
        private bool _isDisposed;
        
        public event Action<MessageRecord>? OnNewMessageReceived;

        public Task SetMeId(User me)
        {
            _myUserId = me.ID;
            Console.WriteLine($"✅ Мой ID: {_myUserId}; {me.MainUsername} , {me.first_name} , {me.LastSeenAgo}");
            return Task.CompletedTask;
        }
        public async Task LoadRecentPersonalMessagesAsync()
        {
            Console.WriteLine("\n📥 Загрузка личных сообщений...");
            int loadedCount = 0;
            try
            {
                var dialogs = await client.Messages_GetDialogs();
                foreach (var dialog in dialogs.Dialogs)
                {
                    if (dialog.Peer is not PeerUser peerUser) 
                        continue;
                    
                    try
                    {
                        var inputPeer = new InputPeerUser(peerUser.user_id, 0);

                        var history = await client.Messages_GetHistory(inputPeer, limit: 5);
                        
                        foreach (var messageBase in history.Messages)
                        {
                            if (messageBase is not Message message) 
                                continue;
                            
                            if (string.IsNullOrWhiteSpace(message.message))
                                continue;
                            
                            var messageRecord = await ConvertToMessageRecord(message);
                            
                            if (messageRecord == null) 
                                continue;
                            
                            storage.AddMessage(messageRecord);
                            loadedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Ошибка загрузки диалога с {dialog.Peer.ID}: {ex.Message}");
                    }
                }
                Console.WriteLine($"✅ Загружено {loadedCount} сообщений");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки истории: {ex.Message}");
            }
        }
        
        public void StartMonitoring()
        {
            client.OnUpdates += HandleUpdates;
        }
        
        public void StopMonitoring()
        {
            client.OnUpdates -= HandleUpdates;
            Console.WriteLine("\n👂 Перестаю мониторинг новых сообщений...");
        }
        
        private async Task HandleUpdates(UpdatesBase updates)
        {
            try
            {
                foreach (var update in updates.UpdateList)
                {
                        if(update is UpdateNewMessage { message: Message message })
                        {
                            await ProcessNewMessage(message);
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка обработки обновления: {ex.Message}");
            } 
               
        }
        
        private async Task ProcessNewMessage(Message message)
        {
            if (message.peer_id is not PeerUser) return;
            
            var messageRecord = await ConvertToMessageRecord(message);
            if (messageRecord == null) return;
            
            storage.AddMessage(messageRecord);
            OnNewMessageReceived?.Invoke(messageRecord);
            
            var direction = messageRecord.IsOutgoing ? "→" : "←";
            Console.WriteLine($"\n✨ НОВОЕ СООБЩЕНИЕ [{messageRecord.Time:HH:mm:ss}]");
            Console.WriteLine($"{direction} {messageRecord.ContactName}:");
            Console.WriteLine($"  {messageRecord.Text}");
            Console.WriteLine(new string('─', 50));

            if (!messageRecord.IsOutgoing && !string.IsNullOrEmpty(messageRecord.Text) && messageRecord.Text.StartsWith("/"))// Отвечает только если начинается с "/"
            {
                await SendMessageAsync(messageRecord.PeerUserId, messageRecord.Text);
            }
        }
        
        private async Task<MessageRecord?> ConvertToMessageRecord(Message message)
        {
            try
            {
                if(message.peer_id is not PeerUser peerUser)
                    return null;
                
                var isOutgoing = message.flags.HasFlag(Message.Flags.out_);
                var contactName = "Неизвестен";

                var userId = isOutgoing ? peerUser.user_id : message.Peer.ID;
                
                if (userId > 0)
                {
                    try
                    {
                        var inputUser = new InputUser(userId, 0);
                        var users = await client.Users_GetUsers(inputUser);
                        if (users.Length > 0 && users[0] is User fetchedUser)
                        {
                            contactName = await GetContactNameAsync(fetchedUser.id);
                        }
                        else
                        {
                            contactName = $"User {userId}";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Не удалось получить пользователя {userId}: {ex.Message}");
                        contactName = $"User {userId}";
                    }
                }
                if (contactName == "Неизвестен")
                {
                    Console.WriteLine("хзчезачел");
                }
                var msg =  new MessageRecord
                {
                    Id = message.id,
                    ContactName = contactName,
                    Text = message.message ?? string.Empty,
                    Time = message.Date.ToLocalTime(),
                    IsOutgoing = isOutgoing,
                    PeerUserId = peerUser.user_id
                };
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Ошибка конвертации сообщения: {ex.Message}");
                return null;
            }
        }
        
        private async Task<string> GetContactNameAsync(long userId)
        {
            if (userCache.TryGetUser(userId, out var cachedUser))
            {
                return userCache.GetUserDisplayName(cachedUser);
            }
            if (userId == _myUserId)
            {
                return "Я";
            }
            try
            {
                var inputUser = new InputUser(userId, 0);
                var users = await client.Users_GetUsers([inputUser]);
                
                if (users.Length > 0 && users[0] is User user)
                {
                    userCache.AddUser(user);
                    return userCache.GetUserDisplayName(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Не удалось получить пользователя {userId}: {ex.Message}");
            }

            Console.WriteLine($"User: {userId}");
            return $"User {userId}";
        }

        private async Task SendMessageAsync(long peerUserId, string text)
        {
            try
            {
                var peer = new InputPeerUser(peerUserId, 0);
                await client.SendMessageAsync(peer, text);
                Console.WriteLine("✅   Ответил: " + text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке ответа {peerUserId}: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            if (_isDisposed) return;
            
            StopMonitoring();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
        
        ~TelegramService()
        {
            Dispose();
        }
    }
}