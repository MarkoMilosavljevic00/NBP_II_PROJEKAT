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
        private readonly IMongoCollection<Kruzer> kr;
        private readonly IMongoCollection<Ponuda> p;
        private readonly IMongoCollection<Korisnik> ko;
        private readonly IMongoCollection<Rezervacija> r;
        private readonly IMongoCollection<Kabina> k;
        [BindProperty]
        public Korisnik menadzer { get; set; }
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public List<Kabina> kabine { get; set; }
        [BindProperty]
        public List<Ponuda> ponude { get; set; }
        [BindProperty]
        public List<Rezervacija> rezervacije { get; set; }
        public string Message { get; set; }

        public ProfilModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            kr = database.GetCollection<Kruzer>("kruzeri");
            k = database.GetCollection<Kabina>("kabine");
            ko = database.GetCollection<Korisnik>("korisnici");
            p = database.GetCollection<Ponuda>("ponude");
            r = database.GetCollection<Rezervacija>("rezervacije");
            kabine = new List<Kabina>();
            ponude = new List<Ponuda>();
            rezervacije = new List<Rezervacija>();
        }

        public void OnGet()
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = ko.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            menadzer = ko.Find(x => x.Tip == 0 && x.Email.Equals(email)).FirstOrDefault();
            String id = menadzer.Kruzer.Id.ToString();
            //if(id==null)
            //{
            //    return RedirectToPage("/Index");
            //}
            kruzer = kr.AsQueryable<Kruzer>().Where(x => x.Id == menadzer.Kruzer.Id).FirstOrDefault();
            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(k.Find(x => x.Id.Equals(new ObjectId(kabinaRef.Id.ToString()))).FirstOrDefault());
            }
            foreach (MongoDBRef ponudeRef in kruzer.Ponude.ToList())
            {
                ponude.Add(p.Find(x => x.Id.Equals(new ObjectId(ponudeRef.Id.ToString()))).FirstOrDefault());
            }
        }

        public ActionResult OnGetKabina(string oznaka)
        {
            String email = HttpContext.Session.GetString("Email");

            menadzer = ko.Find(x => x.Tip == 0 && x.Email.Equals(email)).FirstOrDefault();
            kruzer = kr.Find(x => x.Id.Equals(new ObjectId(menadzer.Kruzer.Id.ToString()))).FirstOrDefault();

            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(k.Find(x => x.Id.Equals(new ObjectId(kabinaRef.Id.ToString()))).FirstOrDefault());
            }
            Kabina soba = kabine.Where(x => x.BrojKabine.Equals(oznaka)).FirstOrDefault();
            List<Rezervacija> rez = new List<Rezervacija>();
            List<Ponuda> ar = new List<Ponuda>();

            foreach (MongoDBRef rezRef in soba.Rezervacije.ToList())
            {
                rez.Add(r.Find(x => x.Id.Equals(new ObjectId(rezRef.Id.ToString()))).FirstOrDefault());
            }
            foreach (Rezervacija Rez in rez)
            {
                ar.Add(p.Find(x => x.Id.Equals(new ObjectId(Rez.Ponuda.Id.ToString()))).FirstOrDefault());
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
