using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BotBaoKhach.Dtos
{
    [BsonIgnoreExtraElements]
    public class SettingBaoKhachDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string BotName { get; set; }
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public string ReadSheetCredentialUrl { get; set; }
        public string ReadSheetId { get; set; }
        public string ReadSheetRange { get; set; }
        public string WriteSheetCredentialUrl { get; set; }
        public string WriteSheetId { get; set; }
        public int Status { get; set; }
        public bool IsDividedByZone { get; set; }
        public bool IsCheckTeam { get; set; }
        public bool IsAmountVisible { get; set; }
        public List<string> Sites { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
