using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PROJEKAT_MONGODB.Model
{
    public class Kruzer
    {
        public ObjectId Id { get; set; }
        public string Naziv { get; set; }
        public string Kapetan { get; set; }
        public string BrojTelefona { get; set; }
        public int BrojZvezdica { get; set; }//https://kvi.travel/travel-tips/cruise-ship-ratings/ od 1 do 6 se krecu ratinzi
        public string Opis { get; set; }
        public List<string> Slike { get; set; } = new List<string>();
        public List<MongoDBRef> Krstarenja { get; set; } = new List<MongoDBRef>();
        public List<MongoDBRef> Kabine { get; set; } = new List<MongoDBRef>();
    }
}
