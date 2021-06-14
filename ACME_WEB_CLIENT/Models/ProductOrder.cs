using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class ProductOrder
    {
        public int oid { get; set; }
        public DateTime date { get; set; }
        public int qty { get; set; }
        public decimal price { get; set; }
        public int pid { get; set; }
        public int uid { get; set; }

        public virtual Product pidNavigation { get; set; }
        public virtual User uidNavigation { get; set; }
    }
}
