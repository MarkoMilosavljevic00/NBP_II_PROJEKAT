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
        public string brTelefona { get; set; }
        public SelectList kapacitet { get; set; }
        [BindProperty]
        public int brojMesta { get; set; }
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

            kruzer = await _dbKruzeri.Find(k => k.Id.Equals(new ObjectId(kruzerId))).FirstOrDefaultAsync();
            krstarenje = await _dbKrstarenja.Find(k => k.Id == new ObjectId(krstarenjeId)).FirstOrDefaultAsync();
            if (kruzer == null || krstarenje == null)
                return RedirectToPage("/Index");
            this.kruzerId = kruzerId;
            this.krstarenjeId = krstarenjeId;
            // List<Aranzman> zav=_dbAranzmani.Find(ar=>true).ToList();
            List<Krstarenje> zabranjenePonude= _dbKrstarenja.Find(k => k.Kruzer.Id == kruzer.Id &&
                                   (
                                   (k.Pocetak.CompareTo(krstarenje.Pocetak) >= 0 && k.Pocetak.CompareTo(krstarenje.Kraj) <= 0) ||
                                   (k.Kraj.CompareTo(krstarenje.Pocetak) >= 0 && k.Kraj.CompareTo(krstarenje.Kraj) <= 0) ||
                                   (k.Pocetak.CompareTo(krstarenje.Pocetak) < 0 && k.Kraj.CompareTo(krstarenje.Kraj) > 0)
                                   )).ToList();
            List<Rezervacija> zabranjeneRezervacije = new List<Rezervacija>();
            foreach (Krstarenje p in zabranjenePonude)
            {
                zabranjeneRezervacije.AddRange(_dbRezervacije.Find(rez => rez.Ponuda.Id == p.Id).ToList());
            }

            List<Kabina> dozvoljeneKabine = new List<Kabina>();
            List<Kabina> sveKabine = _dbKabine.Find(kabina => kabina.Kruzer.Id == kruzerId.Id).ToList();

            /*dozvoljeneSobe.AddRange(_dbSobe.Find(soba => soba.hotel.Id==Hotel.Id&&soba.Rezervacije.Contains(rez.Id)).ToList()); */

            foreach (Kabina k in sveKabine)
            {
                bool ok = true;
                foreach (Rezervacija rez in zabranjeneRezervacije)
                {
                    if (k.Rezervacije.Contains(new MongoDBRef("rezervacije", rez.Id)))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok == true)
                    dozvoljeneKabine.Add(k);
            }

            if (zabranjeneRezervacije.Count != 0)
                kapacitet = new SelectList(dozvoljeneKabine.Select(kab => kab.BrojMesta).Distinct());
            else
                kapacitet = new SelectList(_dbKabine.Distinct(k => k.BrojMesta, kab => kab.Kruzer.Id == kruzerId.Id).ToList());


            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (String.IsNullOrEmpty(ime) || String.IsNullOrEmpty(prezime) || String.IsNullOrEmpty(brTelefona) || String.IsNullOrEmpty(brojPasosa) || brojMesta == 0)
                return Page();

            kruzer = await _dbKruzeri.Find(h => h.Id == new ObjectId(kruzerId)).FirstOrDefaultAsync();
            krstarenje = await _dbKrstarenja.Find(a => a.Id == new ObjectId(krstarenjeId)).FirstOrDefaultAsync();
            List<Krstarenje> zabranjenePonude = _dbKrstarenja.Find(a => a.Kruzer.Id.Equals(kruzer.Id) &&
                                  (
                                  (a.Pocetak.CompareTo(krstarenje.Pocetak) >= 0 && a.Pocetak.CompareTo(krstarenje.Kraj) <= 0) ||
                                  (a.Kraj.CompareTo(krstarenje.Pocetak) >= 0 && a.Kraj.CompareTo(krstarenje.Kraj) <= 0) ||
                                  (a.Pocetak.CompareTo(krstarenje.Pocetak) < 0 && a.Kraj.CompareTo(krstarenje.Kraj) > 0)
                                  )).ToList();

            List<Rezervacija> zabranjeneRezervacije = new List<Rezervacija>();
            foreach (Krstarenje p in zabranjenePonude)
            {
                zabranjeneRezervacije.AddRange(_dbRezervacije.Find(rez => rez.Ponuda.Id == p.Id).ToList());
            }
            Kabina kabineZaRezervisanje = null;
            List<Kabina> sveKabine = _dbKabine.Find(Kabina => Kabina.Kruzer.Id == kruzer.Id && Kabina.BrojMesta == this.brojMesta).ToList();
            foreach (Kabina kabina in sveKabine)
            {
                bool nadjeno = true;
                foreach (Rezervacija rezervacija in zabranjeneRezervacije)
                {
                    if (kabina.Rezervacije.Contains(new MongoDBRef("rezervacije", rezervacija.Id)))
                    {
                        nadjeno = false;
                        break;
                    }
                }
                if (nadjeno)
                {
                    kabineZaRezervisanje = kabina;
                    break;
                }

            }


            if (kabineZaRezervisanje == null)
            {
                return RedirectToPage("/Error");
            }

            Rezervacija novaRezervacija = new Rezervacija();
            novaRezervacija.ImeKorisnika = ime;
            novaRezervacija.PrezimeKorisnika = prezime;
            novaRezervacija.BrojPasosaKorisnika = brojPasosa;
            novaRezervacija.BrojTelefonaKorisnika = brTelefona;
            novaRezervacija.DatumKreiranja = DateTime.Now;
            novaRezervacija.Ponuda = new MongoDBRef("ponude", krstarenje.Id);
            novaRezervacija.Kruzer = new MongoDBRef("kruzeri", kruzer.Id);
            _dbRezervacije.InsertOne(novaRezervacija);

            var update = Builders<Kabina>.Update.Push(kabina => kabina.Rezervacije, new MongoDBRef("rezervacije", novaRezervacija.Id));
            await _dbKabine.UpdateOneAsync(kabina => kabina.Id == kabineZaRezervisanje.Id, update);


            return RedirectToPage("/Index");
        }

    }
}
