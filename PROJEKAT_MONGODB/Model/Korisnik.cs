using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class Korisnik
    {
        //slobodno izmenjajte model sta mislite da treba da se oduzme ili doda
        public ObjectId Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string Sifra { get; set; }
        public string BrojTelefona { get; set; }
        public string Grad { get; set; }
        public string Adresa { get; set; }
        public int Tip { get; set; } 
        public MongoDBRef Kruzer { get; set; }
        
    }
}
