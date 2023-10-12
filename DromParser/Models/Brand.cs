using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromParser.Models
{
    public class Brand
    {
        public string Name { get; set; } // Пример: Acura
        public string UrlToPageWithModels { get; set; }
        
        public List<BrandModel> BrandModels { get; set; }
    }
}
