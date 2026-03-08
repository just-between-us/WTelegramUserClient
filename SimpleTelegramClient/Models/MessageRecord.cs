namespace SimpleTelegramClient.Models
{
    public class MessageRecord
    {
        public int Id { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public bool IsOutgoing { get; set; }
        public long PeerUserId { get; set; }
    }
}
