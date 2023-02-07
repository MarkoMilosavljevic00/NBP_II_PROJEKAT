using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Bson;
using PROJEKAT_MONGODB.Model;

namespace PROJEKAT_MONGODB.Pages
{
    public class KruzeriModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> kr;
        private readonly IMongoCollection<Korisnik> ko;
        public List<Kruzer> kruzeri { get; set; }
        public string Message { get; set; }

        public KruzeriModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            kr = database.GetCollection<Kruzer>("kruzeri");
            ko = database.GetCollection<Korisnik>("korisnici");
        }

        public void OnGet(string grad, string drzava)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = ko.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            kruzeri = kr.Find(x => x.Gradovi.Equals(grad) && x.Drzave.Equals(drzava)).ToList();
        }
    }
}
