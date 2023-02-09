using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PROJEKAT_MONGODB.Model
{
    public class Luka
    {
        public ObjectId Id { get; set; }
        public string Naziv { get; set; }
        public string Mesto { get; set; }
        //public string Drzava { get; set; }
    }
}
