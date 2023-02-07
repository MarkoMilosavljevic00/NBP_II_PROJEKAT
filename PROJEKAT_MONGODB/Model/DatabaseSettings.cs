using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROJEKAT_MONGODB.Model
{
    public class DatabaseSettings : IDatabaseSettings
    {

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings
    {

        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
   
}
