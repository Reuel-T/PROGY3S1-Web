using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class CidNavigation
    {
        public CidNavigation()
        {
           
        }

        public int cid { get; set; }
        public string categoryName { get; set; }

        public virtual List<Product> products { get; set; }
    }
}
