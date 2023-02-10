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
    public class KrstarenjePrikazModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Kabina> _dbKabine;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Luka> _dbLuke;
        public Kruzer kruzer { get; set; }
        public Krstarenje krstarenje { get; set; }
        public List<Kabina> kabine { get; set; }
        public List<Luka> luke { get; set; }
        public string Message { get; set; }

        public KrstarenjePrikazModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
            _dbLuke = database.GetCollection<Luka>("luke");
            kabine = new List<Kabina>();
            luke = new List<Luka>();
        }

        public void OnGet(string krstarenjeId, string kruzerId)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik korisnik = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (korisnik.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }
            ObjectId kruzerIdObj = new ObjectId(kruzerId);
            ObjectId krstarenjeIdObj = new ObjectId(krstarenjeId);
            kruzer = _dbKruzeri.Find(x => x.Id.Equals(kruzerIdObj)).FirstOrDefault();
            krstarenje = _dbKrstarenja.Find(x => x.Id.Equals(krstarenjeIdObj)).FirstOrDefault();
            //foreach (MongoDBRef pon in kruzer.Krstarenja.ToList())
            //{
            //    krstarenja.Add(_dbKrstarenja.Find(x => x.Id.Equals(new ObjectId(pon.Id.ToString()))).FirstOrDefault());
            //}
            foreach (MongoDBRef kabinaRef in kruzer.Kabine.ToList())
            {
                kabine.Add(_dbKabine.Find(x => x.Id.Equals(kabinaRef.Id.AsObjectId)).FirstOrDefault());

            }            
            foreach (MongoDBRef lukaRef in krstarenje.Luke.ToList())
            {
                luke.Add(_dbLuke.Find(x => x.Id.Equals(lukaRef.Id.AsObjectId)).FirstOrDefault());

            }
        }
    }
}
