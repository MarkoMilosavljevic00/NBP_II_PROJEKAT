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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PROJEKAT_MONGODB.Pages
{
    public class RezervacijaModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Rezervacija> _dbRezervacije;
        private readonly IMongoCollection<Kabina> _dbKabine;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        [BindProperty]
        public List<Kabina> dostupneKabine { get; set; } = new List<Kabina>();
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public string kruzerId { get; set; }
        [BindProperty]
        public Krstarenje krstarenje { get; set; }
        [BindProperty]
        public string krstarenjeId { get; set; }
        [BindProperty]
        public string ime { get; set; }
        [BindProperty]
        public string prezime { get; set; }
        [BindProperty]
        public string brojPasosa { get; set; }
        [BindProperty]
        public string email { get; set; }
        public SelectList dostupneKabineSelect { get; set; }
        [BindProperty]
        public string idKabine { get; set; }
        public string Message { get; set; }
        public RezervacijaModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
            _dbRezervacije = database.GetCollection<Rezervacija>("rezervacije");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
        }
        public async Task<IActionResult> OnGet(string krstarenjeId, string kruzerId)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            kruzer = await _dbKruzeri.Find(k => k.Id == new ObjectId(kruzerId)).FirstOrDefaultAsync();
            krstarenje = await _dbKrstarenja.Find(k => k.Id == new ObjectId(krstarenjeId)).FirstOrDefaultAsync();
            if (kruzer == null || krstarenje == null)
                return RedirectToPage("/Index");

            List<Kabina> rezervisaneKabine = await _dbKabine.Find(x => x.Kruzer.Id.AsObjectId.Equals(kruzer.Id) && x.Rezervacije!=null && x.Rezervacije.Count > 0).ToListAsync();
            List<Rezervacija> rezervacije = new List<Rezervacija>();
            foreach (var kabina in rezervisaneKabine)
            {
                Rezervacija r = _dbRezervacije.Find(x =>
                    x.Kabina.Id.AsObjectId.Equals(kabina.Id) &&
                    x.Krstarenje.Id.AsObjectId.Equals(krstarenje.Id) &&
                    x.Kruzer.Id.AsObjectId.Equals(kruzer.Id))
                    .FirstOrDefault();
                if(r!=null)
                    rezervacije.Add(r);
            }
            List<Kabina> sveKabine = await _dbKabine.Find(kabina => kabina.Kruzer.Id == kruzer.Id).ToListAsync();
            if(sveKabine.Count == 0)
            {
                return Page();
            }
            if(rezervacije.Count == 0)
            {
                dostupneKabine = sveKabine;
                return Page();
            }
            foreach(Kabina kabina in sveKabine)
            {
                if (!rezervacije.Any(r => kabina.Rezervacije.Contains(new MongoDBRef("rezervacije", r.Id))))
                {
                    dostupneKabine.Add(kabina);
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (String.IsNullOrEmpty(ime) || String.IsNullOrEmpty(prezime) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(brojPasosa) || String.IsNullOrEmpty(idKabine))
                return Page();

            //kruzer = await _dbKruzeri.Find(k => k.Id == new ObjectId(kruzerId)).FirstOrDefaultAsync();
            //krstarenje = await _dbKrstarenja.Find(k => k.Id == new ObjectId(krstarenjeId)).FirstOrDefaultAsync();
            //List<Krstarenje> zabranjenePonude = _dbKrstarenja.Find(a => a.Kruzer.Id.Equals(kruzer.Id) &&
            //                      (
            //                      (a.Pocetak.CompareTo(krstarenje.Pocetak) >= 0 && a.Pocetak.CompareTo(krstarenje.Kraj) <= 0) ||
            //                      (a.Kraj.CompareTo(krstarenje.Pocetak) >= 0 && a.Kraj.CompareTo(krstarenje.Kraj) <= 0) ||
            //                      (a.Pocetak.CompareTo(krstarenje.Pocetak) < 0 && a.Kraj.CompareTo(krstarenje.Kraj) > 0)
            //                      )).ToList();

            //List<Rezervacija> zabranjeneRezervacije = new List<Rezervacija>();
            //foreach (Krstarenje p in zabranjenePonude)
            //{
            //    zabranjeneRezervacije.AddRange(_dbRezervacije.Find(rez => rez.Ponuda.Id == p.Id).ToList());
            //}
            //Kabina kabineZaRezervisanje = null;
            //List<Kabina> sveKabine = _dbKabine.Find(Kabina => Kabina.Kruzer.Id == kruzer.Id && Kabina.BrojMesta == this.brojMesta).ToList();
            //foreach (Kabina kabina in sveKabine)
            //{
            //    bool nadjeno = true;
            //    foreach (Rezervacija rezervacija in zabranjeneRezervacije)
            //    {
            //        if (kabina.Rezervacije.Contains(new MongoDBRef("rezervacije", rezervacija.Id)))
            //        {
            //            nadjeno = false;
            //            break;
            //        }
            //    }
            //    if (nadjeno)
            //    {
            //        kabineZaRezervisanje = kabina;
            //        break;
            //    }

            //}


            //if (kabineZaRezervisanje == null)
            //{
            //    return RedirectToPage("/Error");
            //}
            kruzer = await _dbKruzeri.Find(k => k.Id == new ObjectId(kruzerId)).FirstOrDefaultAsync();
            krstarenje = await _dbKrstarenja.Find(k => k.Id == new ObjectId(krstarenjeId)).FirstOrDefaultAsync();
            Kabina kabina = await _dbKabine.Find(x => x.Id.Equals(new ObjectId(idKabine))).FirstOrDefaultAsync();

            Rezervacija novaRezervacija = new Rezervacija();
            novaRezervacija.Ime = ime;
            novaRezervacija.Prezime = prezime;
            novaRezervacija.BrojPasosa = brojPasosa;
            novaRezervacija.Email = email;
            novaRezervacija.DatumKreiranja = DateTime.Now;
            novaRezervacija.Pocetak = krstarenje.Pocetak;
            novaRezervacija.Kraj = krstarenje.Kraj;
            novaRezervacija.Kabina = new MongoDBRef("kabine", kabina.Id);
            novaRezervacija.Krstarenje = new MongoDBRef("krstarenja", krstarenje.Id);
            novaRezervacija.Kruzer = new MongoDBRef("kruzeri", kruzer.Id);
            _dbRezervacije.InsertOne(novaRezervacija);

            var update = Builders<Kabina>.Update.Push(kabina => kabina.Rezervacije, new MongoDBRef("rezervacije", novaRezervacija.Id));
            await _dbKabine.UpdateOneAsync(k => k.Id == kabina.Id, update);
            
            var update1 = Builders<Krstarenje>.Update.Push(krstarenje => krstarenje.Rezervacije, new MongoDBRef("rezervacije", novaRezervacija.Id));
            await _dbKrstarenja.UpdateOneAsync(k => k.Id == krstarenje.Id, update1);


            return RedirectToPage("/Index");
        }

    }
}
