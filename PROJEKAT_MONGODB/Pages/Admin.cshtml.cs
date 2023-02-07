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
        private readonly IMongoCollection<Kruzer> kr;//h
        private readonly IMongoCollection<Ponuda> p;//a
        private readonly IMongoCollection<Korisnik> ko;//k
        private readonly IMongoCollection<Rezervacija> r;//r
        private readonly IMongoCollection<Kabina> ka;//s
        [BindProperty]
        public List<Kruzer> kruzeri { get; set; }
        [BindProperty]
        public List<Korisnik> korisnici { get; set; }
        [BindProperty]
        public List<Rezervacija> rezervacije { get; set; }
        [BindProperty]
        public List<Kruzer> kruzeriMenadzera { get; set; }//hoteliMedandzera
        [BindProperty]
        public List<Ponuda> ponudeRezervacija { get; set; }//aranzmani ponuda
        [BindProperty]
        public List<Kruzer> kruzeriRezervacija { get; set; }//hotelirezervacija
        [BindProperty]
        public List<Korisnik> menadzeriRezervacija { get; set; }
        [BindProperty]
        public List<Korisnik> menadzeri { get; set; }
        [BindProperty]
        public List<Ponuda> ponude { get; set; }//aranzmani
        [BindProperty]
        public List<Kruzer> kruzeriPonuda { get; set; }//hoteliaranzmana
        public string Message { get; set; }

        public AdminModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            kr = database.GetCollection<Kruzer>("kruzeri");
            ka = database.GetCollection<Kabina>("kabine");
            ko = database.GetCollection<Korisnik>("korisnici");
            p = database.GetCollection<Ponuda>("ponude");
            r = database.GetCollection<Rezervacija>("rezervacije");
            kruzeriMenadzera = new List<Kruzer>();
            menadzeriRezervacija = new List<Korisnik>();
            ponudeRezervacija = new List<Ponuda>();
            kruzeriRezervacija = new List<Kruzer>();
            menadzeri = new List<Korisnik>();
            ponude = new List<Ponuda>();
            kruzeriPonuda = new List<Kruzer>();
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


            //var test = values.Select(v => BsonSerializer.Deserialize<Property>(v));
            //kruzeri = kr.Find(Builders<Kruzer>.Filter.Empty).ToList();
            kruzeri = kr.Find(x => true).ToList();
            rezervacije = r.Find(x => true).ToList();//kad se promeni status pukne na aktivno resi ovo sa if uslov neki
            korisnici = ko.Find(x => x.Tip == 0).ToList();
            ponude = p.Find(x => true).ToList();
            foreach (Rezervacija rez in rezervacije)
            {
                ponudeRezervacija.Add(p.Find(x => x.Id.Equals(new ObjectId(rez.Ponuda.Id.ToString()))).FirstOrDefault());
                kruzeriRezervacija.Add(kr.Find(x => x.Id.Equals( new ObjectId(rez.Kruzer.Id.ToString()))).FirstOrDefault());
            }
            foreach (Kruzer kruzer in kruzeriRezervacija)
            {
                menadzeriRezervacija.Add(ko.Find(x => x.Kruzer.Id.Equals(kruzer.Id)).FirstOrDefault());
            }
            foreach (Kruzer kruzer in kruzeri)
            {
                //menadzeri.Add(k.Find(x=>x.Hotel.Id.Equals(hot.Id)).FirstOrDefault());
                //string s = k.AsQueryable<Korisnik>().Select(x=>x.Hotel.Id.AsString).FirstOrDefault();
                Korisnik m = ko.AsQueryable<Korisnik>().Where(x => x.Kruzer.Id == kruzer.Id).FirstOrDefault();
                menadzeri.Add(m);

            }
            //foreach (Korisnik kor in menadzeri)
            //{
            //    kruzeriMenadzera.Add(kr.Find(x => x.Id.Equals(new ObjectId(kor.Kruzer.Id.ToString()))).FirstOrDefault());
            //}
            //foreach (Korisnik kor in menadzeri)
            //{
            //    var kruzer = kr.Find(x => x != null && x.Id.Equals(new ObjectId(kor.Kruzer.Id.ToString()))).FirstOrDefault();
            //    if (kruzer != null)
            //    {
            //        kruzeriMenadzera.Add(kruzer);
            //    }
            //}
            //PROMENI U PRIKAZU DA SE IME KORISNIKA VADI IZ REZERVACIJE, A NE IZ KORISNIKA
        }

        public IActionResult OnPostObrisiRez(string id)
        {
            List<Kabina> kabine = ka.Find(x => true).ToList();
            var pull = Builders<Kabina>.Update.PullFilter(x => x.Rezervacije, Builders<MongoDBRef>.Filter.Where(q => q.Id.Equals(new ObjectId(id))));
            foreach (Kabina kabina in kabine)
            {
                var filter = Builders<Kabina>.Filter.Eq("Id", kabina.Id);
                ka.UpdateOne(filter, pull);
            }
            r.DeleteOne<Rezervacija>(x => x.Id.Equals(new ObjectId(id)));
            return RedirectToPage();
        }

        public IActionResult OnPostStatusAktivno(string id)
        {
            var update = Builders<Rezervacija>.Update.Set("Status", "Aktivno");
            var filter = Builders<Rezervacija>.Filter.Eq("Id", new ObjectId(id));
            r.UpdateOne(filter, update);
            return RedirectToPage();
        }

        public IActionResult OnPostObrisiHotel(string id)
        {
            Kruzer kruzer = kr.Find(x => x.Id.Equals(new ObjectId(id))).FirstOrDefault();
            //brisanje menadzera hotela
            var filter1 = Builders<Korisnik>.Filter.Eq("Kruzer.Id", new ObjectId(id));
            ko.DeleteOne(filter1);
            //brisanje soba hotela
            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                var filter2 = Builders<Kabina>.Filter.Eq("Id", kabinaRef.Id);
                ka.DeleteOne(filter2);
            }
            //brisanje aranzmana vezanih za hotel
            foreach (MongoDBRef ponudaRef in kruzer.Ponude.ToList())
            {
                var filter3 = Builders<Ponuda>.Filter.Eq("Id", ponudaRef.Id);
                p.DeleteOne(filter3);
            }
            //brisanje rezervacija vezanih za taj hotel
            var filter = Builders<Rezervacija>.Filter.Eq("Kruzer.Id", new ObjectId(id));
            r.DeleteMany(filter);
            //brisanje hotela
            kr.DeleteOne(x => x.Id.Equals(new ObjectId(id)));

            return RedirectToPage();
        }
    }
}
