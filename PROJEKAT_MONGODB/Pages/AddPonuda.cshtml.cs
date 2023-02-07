using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PROJEKAT_MONGODB.Model;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Bson;

namespace PROJEKAT_MONGODB.Pages
{
    public class AddPonudaModel : PageModel
    {
        [BindProperty]
        public string kruzerID { get; set; }
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public DateTime pocetak { get; set; }
        [BindProperty]
        public DateTime kraj { get; set; }
        [BindProperty]
        public int cena { get; set; }
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Ponuda> _dbPonude;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        public string Message { get; set; }
        public AddPonudaModel(IDatabaseSettings settings)
        {
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbPonude = database.GetCollection<Ponuda>("ponude");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
        }

        public async Task<IActionResult> OnGet(string kruzerId)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToPage("/Login");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 1)
                { return RedirectToPage("/Login"); }
                else { Message = "Menadzer"; }
            }

            ObjectId objId = new ObjectId(kruzerId);
            kruzer = await _dbKruzeri.Find(kruzer => kruzer.Id == objId).FirstOrDefaultAsync();
            kruzerID = kruzerId;
            if (kruzer == null) return RedirectToPage("/Index");
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {

            if (pocetak.CompareTo(kraj) > 0 || cena < 1) return Page();
            Ponuda novaPonuda = new Ponuda();
            novaPonuda.Cena = cena;
            novaPonuda.Pocetak = new DateTime(pocetak.Ticks, DateTimeKind.Utc);
            novaPonuda.Kraj = new DateTime(kraj.Ticks, DateTimeKind.Utc);
            novaPonuda.Kruzer = new MongoDBRef("kruzeri", new ObjectId(kruzerID));
            await _dbPonude.InsertOneAsync(novaPonuda);

            var update = Builders<Kruzer>.Update.Push(Hotel => Hotel.Ponude, new MongoDBRef("aranzmani", novaPonuda.Id));
            await _dbKruzeri.UpdateOneAsync(Hotel => Hotel.Id == new ObjectId(kruzerID), update);
            return RedirectToPage("/KruzerSingle", new { id = kruzerID });
        }
    }
}
