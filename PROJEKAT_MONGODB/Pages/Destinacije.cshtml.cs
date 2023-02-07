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
    public class DestinacijeModel : PageModel
    {
        public List<Drzava> listaDrzava { get; set; }
        public string Message { get; set; }
        public void OnGet()
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var db = client.GetDatabase("SEVENSEAS");
            var collectionKruzeri = db.GetCollection<Kruzer>("kruzeri");
            var collectionGradovi = db.GetCollection<Grad>("gradovi");
            var collectionKorisnici = db.GetCollection<Korisnik>("korisnici");

            String email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                Korisnik k = collectionKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 0)
                    Message = "Menadzer";
                else Message = "Admin";
            }

            //List<string> sveDrzave = collection.AsQueryable<Hotel>().OrderBy(x=>x.Drzava).Select(x=>x.Drzava).Distinct().ToList();
            //foreach(Drzava d in )
            //List<string> sveDrzave = collectionKruzeri.AsQueryable<Kruzer>().SelectMany(x => x.Drzave).Distinct().ToList();
            List<string> sveDrzave = collectionKruzeri.AsQueryable<Kruzer>().SelectMany(x => x.Drzave).Select(x => x.Naziv).Distinct().ToList();
            listaDrzava = new List<Drzava>();

            foreach (string drzava in sveDrzave)
            {
                Drzava d = new Drzava();
                d.Naziv = drzava;
                d.Gradovi = new List<Grad>();
                //d.gradovi = collection.AsQueryable<Hotel>().Where(x=>x.Drzava == drzava).OrderBy(x=>x.Grad).Select(x=>x.Grad).Distinct().ToList();
                //List<string> gradoviDrzave = collectionKruzeri.AsQueryable<Kruzer>().Where(x => x.Drzave == drzava).Select(x => x.Grad).Distinct().ToList();
                List<string> gradoviDrzave = (List<string>)collectionKruzeri.AsQueryable<Kruzer>().Where(x => x.Drzave.Any(y => y.Naziv == drzava)).SelectMany(x => x.Gradovi).Select(z=>z.Naziv).Distinct().ToList();
                foreach (string grad in gradoviDrzave)
                {
                    Grad g = collectionGradovi.AsQueryable<Grad>().Where(x => x.Naziv == grad).FirstOrDefault();
                    d.Gradovi.Add(g);
                }
                listaDrzava.Add(d);
            }

        }
    }
}
