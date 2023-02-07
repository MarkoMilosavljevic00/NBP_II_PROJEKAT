using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using PROJEKAT_MONGODB.Model;
using MongoDB.Bson;

namespace PROJEKAT_MONGODB.Pages
{
    public class KruzerSingleModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> kr;
        private readonly IMongoCollection<Ponuda> p;
        private readonly IMongoCollection<Kabina> ka;
        private readonly IMongoCollection<Korisnik> ko;
        public Kruzer kruzer { get; set; }
        public List<Ponuda> ponude { get; set; }
        public List<Kabina> kabine { get; set; }
        public string Message { get; set; }

        public KruzerSingleModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            kr = database.GetCollection<Kruzer>("kruzeri");
            ka = database.GetCollection<Kabina>("kabine");
            ko = database.GetCollection<Korisnik>("korisnici");
            p = database.GetCollection<Ponuda>("ponude");
            kabine = new List<Kabina>();
            ponude = new List<Ponuda>();
        }

        public void OnGet(string id)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = ko.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }
            ObjectId idHotela = new ObjectId(id);
            kruzer = kr.Find(x => x.Id.Equals(idHotela)).FirstOrDefault();
            foreach (MongoDBRef pon in kruzer.Ponude.ToList())
            {
                ponude.Add(p.Find(x => x.Id.Equals(new ObjectId(pon.Id.ToString()))).FirstOrDefault());
            }
            foreach (MongoDBRef kabina in kruzer.Kabine.ToList())
            {
                kabine.Add(ka.Find(x => x.Id.Equals(new ObjectId(kabina.Id.ToString()))).FirstOrDefault());//nzm ovo kako da resimo sve mu jebeem

            }
        }
        //public static ObjectId ToObjectId(this ObjectId id)
        //{
        //    return new ObjectId(id);
        //}
    }
}
