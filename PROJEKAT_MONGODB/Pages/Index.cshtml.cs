using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using PROJEKAT_MONGODB.Model;

namespace PROJEKAT_MONGODB.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        public List<Krstarenje> krstarenjaZaPrikaz { get; set; }
        public string Message { get; set; }

        public IndexModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
        }

        public void OnGet()
        {
            krstarenjaZaPrikaz = _dbKrstarenja.AsQueryable<Krstarenje>().Take(6).ToList();



            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Remove("Email");
            Message = null;
            return RedirectToPage("/Index");
        }
    }
}
