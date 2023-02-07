using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJEKAT_MONGODB.Model;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;

namespace PROJEKAT_MONGODB.Pages
{
    public class ONamaModel : PageModel
    {
        public string Message { get; set; }
        public void OnGet()
        {

            var client = new MongoClient("mongodb://localhost/?safe=true");
            var db = client.GetDatabase("SEVENSEAS");
            var collection = db.GetCollection<Korisnik>("korisnici");

            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = collection.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }
        }
    }
}
