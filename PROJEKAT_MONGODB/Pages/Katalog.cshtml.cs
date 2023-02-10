using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PROJEKAT_MONGODB.Model;
using MongoDB.Driver;
namespace PROJEKAT_MONGODB.Pages
{
    public class KatalogModel : PageModel
    {
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Luka> _dbLuke;
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        [BindProperty]
        public List<string> listaDestinacija { get; set; }
        [BindProperty]
        public List<KrstarenjeInfo> listaKrstarenjaInfo { get; set; } = new List<KrstarenjeInfo>();
        public string Message { get; set; }

        public KatalogModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var db = client.GetDatabase("SEVENSEAS");
            _dbKrstarenja = db.GetCollection<Krstarenje>("krstarenja");
            _dbKorisnici = db.GetCollection<Korisnik>("korisnici");
            _dbLuke = db.GetCollection<Luka>("luke");
            _dbKruzeri = db.GetCollection<Kruzer>("kruzeri");
        }
        public void OnGet()
        {
            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }
            listaDestinacija = _dbKrstarenja.AsQueryable<Krstarenje>().Select(krstarenje => krstarenje.Destinacija).Distinct().ToList();
            List<Krstarenje> listaKrstarenja = _dbKrstarenja.AsQueryable<Krstarenje>().ToList();
            foreach (var krstarenje in listaKrstarenja)
            {
                string lukaStr = "";
                foreach (MongoDBRef lref in krstarenje.Luke.ToList())
                {
                    Luka luka = _dbLuke.Find(x => x.Id.Equals(lref.Id.AsObjectId)).FirstOrDefault();
                    lukaStr += luka.Naziv + " -" + luka.Mesto;
                    if (krstarenje.Luke.Last() != lref)
                        lukaStr += ", ";
                }

                Kruzer kruzer = _dbKruzeri.Find(x => x.Id.Equals(krstarenje.Kruzer.Id.AsObjectId)).FirstOrDefault();
                listaKrstarenjaInfo.Add(new KrstarenjeInfo
                {
                    Destinacija = krstarenje.Destinacija,
                    GlavnaSlika = krstarenje.Slike[0],
                    NazivKruzera = kruzer.Naziv,
                    RejtingKruzera = kruzer.BrojZvezdica,
                    Luke = lukaStr,
                    Opis = krstarenje.Opis,
                    KrstarenjeId = krstarenje.Id,
                    KruzerId = kruzer.Id
                });
            }
        }
    }
}
