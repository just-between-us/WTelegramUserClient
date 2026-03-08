using TL;

namespace SimpleTelegramClient.Services
{
    public class UserCacheService
    {
        private readonly Dictionary<long, User> _cache = new();
        
        public void AddUser(User user)
        {
            _cache[user.ID] = user;
        }
        
        public bool TryGetUser(long userId, out User? user)
        {
            return _cache.TryGetValue(userId, out user);
        }
        
        public void Clear()
        {
            _cache.Clear();
        }
        
        public int Count => _cache.Count;
        
        public string GetUserDisplayName(User? user)
        {
            if (user == null) return "Неизвестный";
            
            var fullName = $"{user.first_name} {user.last_name}".Trim();
            if (!string.IsNullOrEmpty(fullName))
                return fullName;
            
            if (!string.IsNullOrEmpty(user.username))
                return $"@{user.username}";
            
            return $"User {user.ID}";
        }
    }
}