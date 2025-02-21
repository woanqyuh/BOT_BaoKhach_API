using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BotBaoKhach.Dtos
{
    [BsonIgnoreExtraElements]
    public class ReadListDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId SettingId { get; set; }
        public string Data { get; set; }
        public DateTime LastModified{ get; set; } = DateTime.UtcNow;

    }
}
