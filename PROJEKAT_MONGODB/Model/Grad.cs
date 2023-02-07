using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PROJEKAT_MONGODB.Model
{
    public class Grad
    {
        public ObjectId Id { get; set; }
        public string Naziv { get; set; }
        public string Slika { get; set; }
        public string Opis { get; set; }
        
        public Drzava Drzava { get; set; }//ovde sam zbunjen sad stio nekad MongoDBref a nekad drzava, verovatno oko baze nesto
    }
}
