using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PROJEKAT_MONGODB.Model
{
    public class Rezervacija
    {
        public ObjectId Id { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public string Status { get; set; } = "Na cekanju!";
        public MongoDBRef Ponuda { get; set; }
        public MongoDBRef Kruzer { get; set; }
        public string ImeKorisnika { get; set; }
        public string PrezimeKorisnika { get; set; }
        public string BrojPasosaKorisnika { get; set; }
        public string BrojTelefonaKorisnika { get; set; }
    }
}
