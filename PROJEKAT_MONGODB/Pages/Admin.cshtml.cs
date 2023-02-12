using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PROJEKAT_MONGODB.Model;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace PROJEKAT_MONGODB.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Rezervacija> _dbRezervacije;
        private readonly IMongoCollection<Kabina> _dbKabine;
        [BindProperty]
        public List<Kruzer> kruzeri { get; set; }
        [BindProperty]
        public List<Korisnik> korisnici { get; set; }
        [BindProperty]
        public List<Rezervacija> rezervacije { get; set; }
        [BindProperty]
        public List<Krstarenje> rezervisanaKrstarenja { get; set; }
        [BindProperty]
        public List<Kruzer> kruzeriRezervacija { get; set; }
        [BindProperty]
        public List<Korisnik> menadzeriRezervacija { get; set; }
        [BindProperty]
        public List<Korisnik> menadzeri { get; set; }
        [BindProperty]
        public List<Krstarenje> krstarenja { get; set; }

        public string Message { get; set; }

        public AdminModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
            _dbRezervacije = database.GetCollection<Rezervacija>("rezervacije");
            menadzeriRezervacija = new List<Korisnik>();
            rezervisanaKrstarenja = new List<Krstarenje>();
            kruzeriRezervacija = new List<Kruzer>();
            menadzeri = new List<Korisnik>();
            krstarenja = new List<Krstarenje>();
        }

        public void OnGet()
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                {
                    Message = "Menadzer";
                    RedirectToPage("/Index");
                    return;
                }
                else Message = "Admin";
            }
            else
            {
                RedirectToPage("/Index");
                return;
            }

            kruzeri = _dbKruzeri.Find(x => true).ToList();
            rezervacije = _dbRezervacije.Find(x => true).ToList();
            korisnici = _dbKorisnici.Find(x => x.Tip == 0).ToList();
            krstarenja = _dbKrstarenja.Find(x => true).ToList();
            foreach (Rezervacija rez in rezervacije)
            {
                Krstarenje k = _dbKrstarenja.Find(x => x.Id.Equals(rez.Krstarenje.Id.AsObjectId)).FirstOrDefault();
                if(k!=null)
                    rezervisanaKrstarenja.Add(k);
                kruzeriRezervacija.Add(_dbKruzeri.Find(x => x.Id.Equals(new ObjectId(rez.Kruzer.Id.ToString()))).FirstOrDefault());
            }
            foreach (Kruzer kruzer in kruzeriRezervacija)
            {
                menadzeriRezervacija.Add(_dbKorisnici.Find(x => x.Kruzer.Id.Equals(kruzer.Id)).FirstOrDefault());
            }
            foreach (Kruzer kruzer in kruzeri)
            {
                Korisnik m = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Kruzer.Id == kruzer.Id).FirstOrDefault();
                menadzeri.Add(m);

            }
        }

        public IActionResult OnPostObrisiRez(string rezervacijaid)
        {
            List<Kabina> kabine = _dbKabine.Find(x => true).ToList();
            var pull = Builders<Kabina>.Update.PullFilter(x => x.Rezervacije, Builders<MongoDBRef>.Filter.Where(q => q.Id.Equals(new ObjectId(rezervacijaid))));
            foreach (Kabina kabina in kabine)
            {
                var filter = Builders<Kabina>.Filter.Eq("Id", kabina.Id);
                _dbKabine.UpdateOne(filter, pull);
            }
            List<Krstarenje> krstarenja = _dbKrstarenja.Find(x => true).ToList();

            var pull1 = Builders<Krstarenje>.Update.PullFilter(x => x.Rezervacije, Builders<MongoDBRef>.Filter.Where(q => q.Id.Equals(new ObjectId(rezervacijaid))));
            foreach (Krstarenje krstarenje in krstarenja)
            {
                var filter1 = Builders<Krstarenje>.Filter.Eq("Id", krstarenje.Id);
                _dbKrstarenja.UpdateOne(filter1, pull1);
            }
            _dbRezervacije.DeleteOne<Rezervacija>(x => x.Id.Equals(new ObjectId(rezervacijaid)));
            return RedirectToPage();
        }

        public IActionResult OnPostStatusAktivno(string rezervacijaid)
        {
            var update = Builders<Rezervacija>.Update.Set("Status", 1);
            var filter = Builders<Rezervacija>.Filter.Eq("Id", new ObjectId(rezervacijaid));
            _dbRezervacije.UpdateOne(filter, update);
            return RedirectToPage();
        }

        public IActionResult OnPostObrisiKruzer(string id)
        {
            Kruzer kruzer = _dbKruzeri.Find(x => x.Id.Equals(new ObjectId(id))).FirstOrDefault();
            var filter1 = Builders<Korisnik>.Filter.Eq("Kruzer.Id", new ObjectId(id));
            _dbKorisnici.DeleteOne(filter1);
            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                var filter2 = Builders<Kabina>.Filter.Eq("Id", kabinaRef.Id);
                _dbKabine.DeleteOne(filter2);
            }
            foreach (MongoDBRef KrstarenjeRef in kruzer.Krstarenja.ToList())
            {
                var filter3 = Builders<Krstarenje>.Filter.Eq("Id", KrstarenjeRef.Id);
                _dbKrstarenja.DeleteOne(filter3);
            }
            var filter = Builders<Rezervacija>.Filter.Eq("Kruzer.Id", new ObjectId(id));
            _dbRezervacije.DeleteMany(filter);
            _dbKruzeri.DeleteOne(x => x.Id.Equals(new ObjectId(id)));

            return RedirectToPage();
        }
    }
}
