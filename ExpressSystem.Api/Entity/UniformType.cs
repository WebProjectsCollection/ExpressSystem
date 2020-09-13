using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class UniformType
    {
        public string Season { get; set; }
        public string Style { get; set; }
        public int SiteID { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
