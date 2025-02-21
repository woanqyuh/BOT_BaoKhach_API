using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotBaoKhach.Dtos
{
    [BsonIgnoreExtraElements]
    public class PermissionDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}