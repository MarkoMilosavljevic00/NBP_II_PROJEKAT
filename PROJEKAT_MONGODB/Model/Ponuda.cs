using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class Ponuda
    {
        //mozda je ovo trebalo aranzman da se 
        public ObjectId Id { get; set; }
        public DateTime Pocetak { get; set; }
        public DateTime Kraj { get; set; }
        public int Cena { get; set; }
        public MongoDBRef Kruzer { get; set; }//verovatno sa kruzer pristupis ostalim zaebancijama gradovima drzavama i to, ako nesto treba da se menja promenicemo
    }
}
