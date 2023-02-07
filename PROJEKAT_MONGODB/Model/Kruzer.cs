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
        //public string Adresa { get; set; } ne treba nam ovo 
        //public  List<string> Gradovi { get; set; }

        //public List<string> Drzave { get; set; }
        //public string Grad { get; set; }
        //public string Drzava { get; set; }


        public List<Drzava> Drzave { get; set; } = new List<Drzava>();
        public List<Grad> Gradovi { get; set; } = new List<Grad>();

        public string KontaktTelefon { get; set; }//da se kontaktira administracija tog kruzera za neka dalja pitanja

        public int BrojZvezdica { get; set; }//https://kvi.travel/travel-tips/cruise-ship-ratings/ od 1 do 6 se krecu ratinzi
        public List<MongoDBRef> Ponude { get; set; } = new List<MongoDBRef>();
        public string Opis { get; set; }
        public string GlavnaSlika { get; set; }
  
        public string Slika1 { get; set; }
        public string Slika2 { get; set; }
        public string Slika3 { get; set; }
        //public double longitude { get; set; } //ovo nzm da l nam treba
        //public double latitude { get; set; }
        public List<MongoDBRef> Kabine { get; set; } = new List<MongoDBRef>();
    }
}
