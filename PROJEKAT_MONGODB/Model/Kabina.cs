using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
namespace PROJEKAT_MONGODB.Model
{
    public class Kabina
    {
        public ObjectId Id { get; set; }
        public string BrojKabine { get; set; }
        public List<MongoDBRef> Rezervacije { get; set; } = new List<MongoDBRef>();
        public int BrojMesta { get; set; }
        public MongoDBRef Kruzer { get; set; }
    }
}
