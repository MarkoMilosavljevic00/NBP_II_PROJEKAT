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
    public class BookingFormModel : PageModel
    {
        public string Message { get; set; }
        [BindProperty]
        public Kruzer Kruzer { get; set; }
        [BindProperty]
        public Ponuda ponuda { get; set; }

        [BindProperty]
        public string KruzerId { get; set; }
        [BindProperty]
        public string ponudaId { get; set; }
        [BindProperty]
        public string ime { get; set; }
        [BindProperty]
        public string Prezime { get; set; }
        [BindProperty]
        public string brojPasosa { get; set; }
        [BindProperty]
        public string brTelefona { get; set; }
        public SelectList kapacitet { get; set; }
        [BindProperty]
        public int brojMesta { get; set; }
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Ponuda> _dbPonude;
        private readonly IMongoCollection<Rezervacija> _dbRezervacije;
        private readonly IMongoCollection<Kabina> _dbKabine;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        public BookingFormModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbPonude = database.GetCollection<Ponuda>("ponude");
            _dbRezervacije = database.GetCollection<Rezervacija>("rezervacije");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
        }
        public async Task<IActionResult> OnGet(string id, string kruzer)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            Kruzer = await _dbKruzeri.Find(k => k.Id == new ObjectId(kruzer)).FirstOrDefaultAsync();
            ponuda = await _dbPonude.Find(p => p.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (kruzer == null || ponuda == null)
                return RedirectToPage("/Index");
            KruzerId = kruzer;
            ponudaId = id;
            // List<Aranzman> zav=_dbAranzmani.Find(ar=>true).ToList();
            List<Ponuda> zabranjenePonude= _dbPonude.Find(p => p.Kruzer.Id == Kruzer.Id &&
                                   (
                                   (p.Pocetak.CompareTo(ponuda.Pocetak) >= 0 && p.Pocetak.CompareTo(ponuda.Kraj) <= 0) ||
                                   (p.Kraj.CompareTo(ponuda.Pocetak) >= 0 && p.Kraj.CompareTo(ponuda.Kraj) <= 0) ||
                                   (p.Pocetak.CompareTo(ponuda.Pocetak) < 0 && p.Kraj.CompareTo(ponuda.Kraj) > 0)
                                   )).ToList();
            List<Rezervacija> zabranjeneRezervacije = new List<Rezervacija>();
            foreach (Ponuda p in zabranjenePonude)
            {
                zabranjeneRezervacije.AddRange(_dbRezervacije.Find(rez => rez.Ponuda.Id == p.Id).ToList());
            }

            List<Kabina> dozvoljeneKabine = new List<Kabina>();
            List<Kabina> sveKabine = _dbKabine.Find(kabina => kabina.Kruzer.Id == Kruzer.Id).ToList();

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
                kapacitet = new SelectList(_dbKabine.Distinct(k => k.BrojMesta, kab => kab.Kruzer.Id == Kruzer.Id).ToList());


            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (String.IsNullOrEmpty(ime) || String.IsNullOrEmpty(Prezime) || String.IsNullOrEmpty(brTelefona) || String.IsNullOrEmpty(brojPasosa) || brojMesta == 0)
                return Page();

            Kruzer = await _dbKruzeri.Find(h => h.Id == new ObjectId(KruzerId)).FirstOrDefaultAsync();
            ponuda = await _dbPonude.Find(a => a.Id == new ObjectId(ponudaId)).FirstOrDefaultAsync();
            List<Ponuda> zabranjenePonude = _dbPonude.Find(a => a.Kruzer.Id.Equals(Kruzer.Id) &&
                                  (
                                  (a.Pocetak.CompareTo(ponuda.Pocetak) >= 0 && a.Pocetak.CompareTo(ponuda.Kraj) <= 0) ||
                                  (a.Kraj.CompareTo(ponuda.Pocetak) >= 0 && a.Kraj.CompareTo(ponuda.Kraj) <= 0) ||
                                  (a.Pocetak.CompareTo(ponuda.Pocetak) < 0 && a.Kraj.CompareTo(ponuda.Kraj) > 0)
                                  )).ToList();

            List<Rezervacija> zabranjeneRezervacije = new List<Rezervacija>();
            foreach (Ponuda p in zabranjenePonude)
            {
                zabranjeneRezervacije.AddRange(_dbRezervacije.Find(rez => rez.Ponuda.Id == p.Id).ToList());
            }
            Kabina kabineZaRezervisanje = null;
            List<Kabina> sveKabine = _dbKabine.Find(Kabina => Kabina.Kruzer.Id == Kruzer.Id && Kabina.BrojMesta == this.brojMesta).ToList();
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
            novaRezervacija.PrezimeKorisnika = Prezime;
            novaRezervacija.BrojPasosaKorisnika = brojPasosa;
            novaRezervacija.BrojTelefonaKorisnika = brTelefona;
            novaRezervacija.DatumKreiranja = DateTime.Now;
            novaRezervacija.Ponuda = new MongoDBRef("ponude", ponuda.Id);
            novaRezervacija.Kruzer = new MongoDBRef("kruzeri", Kruzer.Id);
            _dbRezervacije.InsertOne(novaRezervacija);

            var update = Builders<Kabina>.Update.Push(kabina => kabina.Rezervacije, new MongoDBRef("rezervacije", novaRezervacija.Id));
            await _dbKabine.UpdateOneAsync(kabina => kabina.Id == kabineZaRezervisanje.Id, update);


            return RedirectToPage("/Index");
        }

    }
}
