using MongoDB.Bson.Serialization.Attributes;

namespace ExpenseSharingApplication
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; }
        public string FullName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }


    }
}