using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class Drzava
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }//da l ima smisla da stavimo opis drzave,posto imamo opis gradq, al mozemo sto da ne

        public List<Luka> Gradovi { get; set; }
    }
}
