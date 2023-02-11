using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class Krstarenje
    {
        public ObjectId Id { get; set; }
        public string Destinacija { get; set; }
        public DateTime Pocetak { get; set; }
        public DateTime Kraj { get; set; }
        public float Cena { get; set; }
        public List<string> Slike { get; set; } = new List<string>();
        public string Opis { get; set; }
        public List<MongoDBRef> Luke { get; set; } = new List<MongoDBRef>();
        public List<MongoDBRef> Rezervacije { get; set; } = new List<MongoDBRef>();
        public MongoDBRef Kruzer { get; set; }
    }
}
