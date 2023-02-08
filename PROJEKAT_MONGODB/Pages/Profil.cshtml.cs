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
        public Korisnik menadzer { get; set; }
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public List<Kabina> kabine { get; set; }
        [BindProperty]
        public List<Krstarenje> krstarenja { get; set; }
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

            menadzer = _dbKorisnici.Find(x => x.Tip == 0 && x.Email.Equals(email)).FirstOrDefault();
            String id = menadzer.Kruzer.Id.ToString();
            //if(id==null)
            //{
            //    return RedirectToPage("/Index");
            //}
            kruzer = _dbKruzeri.AsQueryable<Kruzer>().Where(x => x.Id == menadzer.Kruzer.Id).FirstOrDefault();
            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(_dbKabine.Find(x => x.Id.Equals(new ObjectId(kabinaRef.Id.ToString()))).FirstOrDefault());
            }
            List<Luka> pom = new List<Luka>();
            foreach (MongoDBRef krstarenjaRef in kruzer.Krstarenja.ToList())
            {
                Krstarenje pomKrs = _dbKrstarenja.Find(x => x.Id.Equals(new ObjectId(krstarenjaRef.Id.ToString()))).FirstOrDefault();
                krstarenja.Add(pomKrs);
                foreach(MongoDBRef lukaRef in pomKrs.Luke.ToList())
                {
                    pom.Add(_dbLuke.Find(x => x.Id.Equals(new ObjectId(lukaRef.Id.ToString()))).FirstOrDefault());
                }
                luke.Add(pom);
            }
        }

        public ActionResult OnGetKabina(string oznaka)
        {
            String email = HttpContext.Session.GetString("Email");

            menadzer = _dbKorisnici.Find(x => x.Tip == 0 && x.Email.Equals(email)).FirstOrDefault();
            kruzer = _dbKruzeri.Find(x => x.Id.Equals(new ObjectId(menadzer.Kruzer.Id.ToString()))).FirstOrDefault();

            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(_dbKabine.Find(x => x.Id.Equals(new ObjectId(kabinaRef.Id.ToString()))).FirstOrDefault());
            }
            Kabina soba = kabine.Where(x => x.BrojKabine.Equals(oznaka)).FirstOrDefault();
            List<Rezervacija> rez = new List<Rezervacija>();
            List<Krstarenje> ar = new List<Krstarenje>();

            foreach (MongoDBRef rezRef in soba.Rezervacije.ToList())
            {
                rez.Add(_dbRezervacije.Find(x => x.Id.Equals(new ObjectId(rezRef.Id.ToString()))).FirstOrDefault());
            }
            foreach (Rezervacija Rez in rez)
            {
                ar.Add(_dbKrstarenja.Find(x => x.Id.Equals(new ObjectId(Rez.Ponuda.Id.ToString()))).FirstOrDefault());
            }

            List<string> datum = new List<string>();
            List<string> status = new List<string>();
            List<string> pocetak = new List<string>();
            List<string> kraj = new List<string>();
            for (int i = 0; i < rez.Count; i++)
            {
                datum.Add(rez.ElementAt(i).DatumKreiranja.ToString("dd.MM.yyyy."));
                status.Add(rez.ElementAt(i).Status);
                pocetak.Add(ar.ElementAt(i).Pocetak.ToString("dd.MM.yyyy."));
                kraj.Add(ar.ElementAt(i).Kraj.ToString("dd.MM.yyyy."));
            }
            var result = new { Datum = datum, Status = status, Pocetak = pocetak, Kraj = kraj };
            return new JsonResult(result);
        }
    }
}
