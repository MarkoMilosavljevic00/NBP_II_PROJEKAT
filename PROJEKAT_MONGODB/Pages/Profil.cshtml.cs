using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using PROJEKAT_MONGODB.Model;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace PROJEKAT_MONGODB.Pages
{
    public class ProfilModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Rezervacija> _dbRezervacije;
        private readonly IMongoCollection<Kabina> _dbKabine;
        private readonly IMongoCollection<Luka> _dbLuke;
        [BindProperty]
        public Korisnik korisnik { get; set; }
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public List<Kabina> kabine { get; set; }
        [BindProperty]
        public List<Krstarenje> krstarenja { get; set; }
        [BindProperty]
        public List<Krstarenje> rezervisanaKrstarenja { get; set; }
        [BindProperty]
        public List<Rezervacija> rezervacije { get; set; }
        [BindProperty]
        public List<List<Luka>> luke { get; set; }
        public string Message { get; set; }

        public ProfilModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
            _dbRezervacije = database.GetCollection<Rezervacija>("rezervacije");
            _dbLuke = database.GetCollection<Luka>("luke");
            kabine = new List<Kabina>();
            krstarenja = new List<Krstarenje>();
            rezervisanaKrstarenja = new List<Krstarenje>();
            rezervacije = new List<Rezervacija>();
            luke = new List<List<Luka>>();
        }

        public void OnGet()
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            korisnik = _dbKorisnici.Find(x => x.Tip == 0 && x.Email.Equals(email)).FirstOrDefault();
            kruzer = _dbKruzeri.AsQueryable<Kruzer>().Where(x => x.Id == korisnik.Kruzer.Id.AsObjectId).FirstOrDefault();

            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(_dbKabine.Find(x => x.Id.Equals(new ObjectId(kabinaRef.Id.ToString()))).FirstOrDefault());
            }
            foreach (MongoDBRef krstarenjaRef in kruzer.Krstarenja.ToList())
            {
                Krstarenje pomKrs = _dbKrstarenja.Find(x => x.Id.Equals(new ObjectId(krstarenjaRef.Id.ToString()))).FirstOrDefault();
                krstarenja.Add(pomKrs);
                if (pomKrs.Luke != null)
                {
                    List<Luka> pom = new List<Luka>();
                    foreach (MongoDBRef lukaRef in pomKrs.Luke.ToList())
                    {
                        pom.Add(_dbLuke.Find(x => x.Id.Equals(new ObjectId(lukaRef.Id.ToString()))).FirstOrDefault());
                    }
                    luke.Add(pom);
                }
            }

            rezervacije = _dbRezervacije.Find(x => x.Kruzer.Id.AsObjectId == kruzer.Id).ToList();
            foreach(var r in rezervacije)
            {
                rezervisanaKrstarenja.Add(_dbKrstarenja.Find(x => x.Id.Equals(new ObjectId(r.Krstarenje.Id.ToString()))).FirstOrDefault());
            }
        }

        public IActionResult OnPostPrihvati(string rezervacijaid)
        {
            var update = Builders<Rezervacija>.Update.Set("Status", 1);
            var filter = Builders<Rezervacija>.Filter.Eq("Id", new ObjectId(rezervacijaid));
            _dbRezervacije.UpdateOne(filter, update);
            return RedirectToPage();
        }

        public IActionResult OnPostOdbij(string rezervacijaid)
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
    }
}
