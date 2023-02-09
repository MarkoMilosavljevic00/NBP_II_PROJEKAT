using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using PROJEKAT_MONGODB.Model;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PROJEKAT_MONGODB.Pages
{
    public class DodajKruzer : PageModel
    {
        private IWebHostEnvironment _environment;
        private readonly IMongoCollection<Korisnik> _dbKorisnici;
        private readonly IMongoCollection<Kruzer> _dbKruzeri;
        private readonly IMongoCollection<Kabina> _dbKabine;
        private readonly IMongoCollection<Luka> _dbGradovi;
        public DodajKruzer(IWebHostEnvironment ev, IDatabaseSettings settings)
        {
            _environment = ev;
            var client = new MongoClient("mongodb://localhost/?safe=true");
            var database = client.GetDatabase("SEVENSEAS");
            _dbKorisnici = database.GetCollection<Korisnik>("korisnici");
            _dbKruzeri = database.GetCollection<Kruzer>("kruzeri");
            _dbKabine = database.GetCollection<Kabina>("kabine");
            _dbGradovi = database.GetCollection<Luka>("gradovi");

        }
        //[BindProperty]
        //public string glavnaSlika { get; set; }
        //[BindProperty]
        //public string slika1 { get; set; }
        //[BindProperty]
        //public string slika2 { get; set; }
        //[BindProperty]
        //public string slika3 { get; set; }
        [BindProperty]
        public string[] slike { get; set; }
        [BindProperty]
        public Kruzer noviKruzer { get; set; }
        [BindProperty]
        public List<string> kabine { get; set; }
        [BindProperty]
        public string imageError { get; set; }
        //[BindProperty]
        //public string cities { get; set; }
        //[BindProperty]
        //public string states { get; set; }

        public string Message { get; set; }

        public ActionResult OnGet()
        {
            String email = HttpContext.Session.GetString("Email");
            if (email == null) return RedirectToPage("/Prijava");
            if (email != null)
            {
                Korisnik k = _dbKorisnici.AsQueryable<Korisnik>().Where(x => x.Email == email).FirstOrDefault();
                if (k.Tip == 1)
                    return RedirectToPage("/Prijava");
                else { Message = "Menadzer"; }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToPage("/Index");
            Korisnik kor = await _dbKorisnici.Find(kor => kor.Email == HttpContext.Session.GetString("Email")).FirstOrDefaultAsync();

            if(string.IsNullOrEmpty(slike[0]))
            {
                imageError = "Morate upload-ovati barem 1 sliku!";
                return Page();
            }

            string folderName = System.Guid.NewGuid().ToString();
            string fileName = "";
            try
            {
                for(int i=0; i<slike.Length; i++)
                {
                    if(!string.IsNullOrEmpty(slike[i]))
                    {
                        fileName = saveBase64AsImage(slike[i], folderName);
                        noviKruzer.Slike.Add("images/" + folderName + "/" + fileName);
                    }
                }
            }
            catch (FormatException fe)
            {
                RedirectToPage("/Error?errorCode=" + fe);
            }

            if (!validirajKruzer())
                return RedirectToPage();
            await _dbKruzeri.InsertOneAsync(noviKruzer);

            List<Kabina> noveKabine = new List<Kabina>();
            foreach (string k in kabine)
            {
                Kabina novaKabina = new Kabina();
                string labela = k.Substring(0, k.LastIndexOf('-'));
                int kapacitet;
                bool uspesno = Int32.TryParse(k.Substring(k.LastIndexOf('-') + 1), out kapacitet);
                if (!uspesno) return RedirectToPage("/Error");
                novaKabina.BrojMesta = kapacitet;
                novaKabina.BrojKabine = labela;
                noveKabine.Add(novaKabina);
            }
            foreach (Kabina k in noveKabine)
            {
                k.Kruzer = new MongoDBRef("kruzeri", noviKruzer.Id);
            }
            await _dbKabine.InsertManyAsync(noveKabine);
            var update = Builders<Kruzer>.Update.PushEach(Kruzer => Kruzer.Kabine, noveKabine.Select(kabina => new MongoDBRef("kabine", kabina.Id)));
            await _dbKruzeri.UpdateOneAsync(kruzer => kruzer.Id == noviKruzer.Id, update);

            var filter = Builders<Korisnik>.Filter.Eq(kori => kori.Id, kor.Id);
            var up = Builders<Korisnik>.Update.Set("Kruzer", new MongoDBRef("kruzeri", noviKruzer.Id));
            //if (_dbGradovi.Find(grad => grad.Naziv==noviKruzer.Grad).FirstOrDefault() == null)

            await _dbKorisnici.UpdateOneAsync(filter, up);
            return RedirectToPage("/Index");
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

        public bool validirajKruzer()
        {
            if (string.IsNullOrEmpty(noviKruzer.Opis) || (string.IsNullOrWhiteSpace(noviKruzer.Opis)))
                return false;
            if (string.IsNullOrEmpty(noviKruzer.Naziv) || (string.IsNullOrWhiteSpace(noviKruzer.Naziv)))
                return false;
            if (string.IsNullOrEmpty(noviKruzer.BrojTelefona) || (string.IsNullOrWhiteSpace(noviKruzer.BrojTelefona)))
                return false;
            if (noviKruzer.BrojZvezdica < 1 || noviKruzer.BrojZvezdica > 6)
                return false;
            return true;

        }

    }
}
