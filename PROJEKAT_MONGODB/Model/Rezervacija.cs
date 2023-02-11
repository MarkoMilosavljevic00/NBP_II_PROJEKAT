using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PROJEKAT_MONGODB.Model
{
    public enum StatusRezervacije
    {
        Cekanje,
        Prihvaceno,
        Odbijeno
    }
    public class Rezervacija
    {
        public ObjectId Id { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public string Status { get; set; } = "Na cekanju!";
        public MongoDBRef Krstarenje { get; set; }
        public MongoDBRef Soba { get; set; }
        public string ImeKorisnika { get; set; }
        public string PrezimeKorisnika { get; set; }
        public string BrojPasosaKorisnika { get; set; }
        public string BrojTelefonaKorisnika { get; set; }
    }
}
