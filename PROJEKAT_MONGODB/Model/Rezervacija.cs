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
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string BrojPasosa { get; set; }
        public string Email { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public DateTime Pocetak { get; set; }
        public DateTime Kraj { get; set; }
        public StatusRezervacije Status { get; set; } = StatusRezervacije.Cekanje;
        public MongoDBRef Krstarenje { get; set; }
        public MongoDBRef Kruzer { get; set; }
        public MongoDBRef Kabina { get; set; }
    }
}
