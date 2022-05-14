using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingApplication
{
    public class Expense
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string GroupId { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

        public string CreatedBy { get; set; }
        public string Payer { get; set; }

        public decimal Amount { get; set; }
        public string[] Involved { get; set; }

        public string [] Confirmed { get; set; }
    }
}
