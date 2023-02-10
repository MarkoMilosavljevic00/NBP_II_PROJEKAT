using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class KrstarenjeInfo
    {
        public string Destinacija { get; set; }
        public string GlavnaSlika { get; set; }
        public string NazivKruzera { get; set; }
        public int RejtingKruzera { get; set; }
        public string Luke { get; set; }
        public string Opis { get; set; }
        public ObjectId KrstarenjeId { get; set; }
        public ObjectId KruzerId { get; set; }


    }
}
