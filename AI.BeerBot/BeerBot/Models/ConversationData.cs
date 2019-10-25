namespace BeerBot.Models
{
    public class ConversationData
    {
        public string ChannelId { get; set; }

        public string Timestamp { get; set; }

        public bool PromptedUserForName { get; set; }
    }
}
