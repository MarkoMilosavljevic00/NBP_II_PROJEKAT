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
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace PROJEKAT_MONGODB.Pages
{
    public class DodajKrstarenjeModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Krstarenje> _dbKrstarenja;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Luka> _dbLuke;

        [BindProperty]
        public string kruzerID { get; set; }
        [BindProperty]
        public Kruzer kruzer { get; set; }
        [BindProperty]
        public string destinacija { get; set; }
        [BindProperty]
        public DateTime pocetak { get; set; }
        [BindProperty]
        public DateTime kraj { get; set; }
        [BindProperty]
        public float cena { get; set; }
        [BindProperty]
        public string opis { get; set; }
        [BindProperty]
        public List<string> luke { get; set; }
        [BindProperty]
        public string[] slike { get; set; }
        [BindProperty]
        public string imageError { get; set; } = "";
        public string Message { get; set; }
        public DodajKrstarenjeModel(IWebHostEnvironment ev, IDatabaseSettings settings)
        {
            _environment = ev;
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKrstarenja = database.GetCollection<Krstarenje>("krstarenja");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbLuke = database.GetCollection<Luka>("luke");
        }

        public async Task<IActionResult> OnGet(string kruzerId)
        {
            String email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToPage("/Prijava");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 1)
                { return RedirectToPage("/Prijava"); }
                else { Message = "Menadzer"; }
            }

            ObjectId objId = new ObjectId(kruzerId);
            kruzer = await _dbKruzeri.Find(kruzer => kruzer.Id == objId).FirstOrDefaultAsync();
            kruzerID = kruzerId;
            if (kruzer == null) 
                return RedirectToPage("/Index");
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (pocetak.CompareTo(kraj) > 0 || cena < 1) 
                return Page();

            Krstarenje novoKrstarenje = new Krstarenje();
            novoKrstarenje.Destinacija = destinacija;
            novoKrstarenje.Opis = opis;
            novoKrstarenje.Cena = cena;
            novoKrstarenje.Pocetak = new DateTime(pocetak.Ticks, DateTimeKind.Utc);
            novoKrstarenje.Kraj = new DateTime(kraj.Ticks, DateTimeKind.Utc);
            novoKrstarenje.Kruzer = new MongoDBRef("kruzeri", new ObjectId(kruzerID));

            List<Luka> noveLuke = new List<Luka>();
            foreach (string l in luke)
            {
                Luka novaLuka = new Luka();
                string labela = l.Substring(0, l.LastIndexOf(','));
                string mesto;
                mesto = l.Substring(l.LastIndexOf(',') + 1);
                novaLuka.Naziv = labela;
                novaLuka.Mesto = mesto;
                noveLuke.Add(novaLuka);
            }
            //foreach (Luka l in noveLuke)
            //{
            //    novoKrstarenje.Luke.Add(new MongoDBRef("luke", );
            //}
            await _dbLuke.InsertManyAsync(noveLuke);

            if (string.IsNullOrEmpty(slike[0]))
            {
                imageError = "Morate upload-ovati barem 1 sliku!";
                return Page();
            }

            string folderName = System.Guid.NewGuid().ToString();
            string fileName = "";
            try
            {
                for (int i = 0; i < slike.Length; i++)
                {
                    if (!string.IsNullOrEmpty(slike[i]))
                    {
                        fileName = saveBase64AsImage(slike[i], folderName);
                        novoKrstarenje.Slike.Add("images/" + folderName + "/" + fileName);
                    }
                }
            }
            catch (FormatException fe)
            {
                RedirectToPage("/Error?errorCode=" + fe);
            }

            await _dbKrstarenja.InsertOneAsync(novoKrstarenje);
            var update1 = Builders<Krstarenje>.Update.PushEach(Krstarenje => Krstarenje.Luke, noveLuke.Select(luka => new MongoDBRef("luke", luka.Id)));
            await _dbKrstarenja.UpdateOneAsync(krstarenje => krstarenje.Id == novoKrstarenje.Id, update1);
            var update = Builders<Kruzer>.Update.Push(Kruzer => Kruzer.Krstarenja, new MongoDBRef("krstarenja", novoKrstarenje.Id));
            await _dbKruzeri.UpdateOneAsync(Kruzer => Kruzer.Id == new ObjectId(kruzerID), update);
            //return RedirectToPage("/KruzerSingle", new { id = kruzerID });
            return RedirectToPage("/Profil");
        }

        public string saveBase64AsImage(string img, string folderName)
        {
            img = img.Substring(img.IndexOf(',') + 1);
            var imgConverted = Convert.FromBase64String(img);

            string imgName = System.Guid.NewGuid().ToString();

            var file = Path.Combine(_environment.ContentRootPath, "wwwroot/images/" + folderName + "/" + imgName + ".jpg");
            Directory.CreateDirectory(Path.GetDirectoryName(file));

            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                fileStream.Write(imgConverted, 0, imgConverted.Length);
                fileStream.Flush();
            }
            return imgName + ".jpg";
        }
    }
}
