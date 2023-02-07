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
        private readonly ILogger<IndexModel> _logger;

        //public IndexModel(ILogger<IndexModel> logger)
        //{
        //    _logger = logger;
        //}

        public List<Kruzer> topRated { get; set; }
        public string Message { get; set; }


        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Drzava> _dbDrzave;


        public IndexModel(IDatabaseSettings settings)
        {
            //var client = new MongoClient(settings.ConnectionString);
            //var database = client.GetDatabase(settings.DatabaseName);
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbDrzave = database.GetCollection<Drzava>("drzave");
        }

        public void OnGet()
        {
            topRated = _dbKruzeri.AsQueryable<Kruzer>().OrderByDescending(Hotel => Hotel.BrojZvezdica).Take(4).ToList();



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
            HttpContext.Session.Remove("email");
            Message = null;
            return RedirectToPage("/Index");
            //<a asp-page="/Index" asp-page-handler="Logout">Logout</a>
        }
    }
}
