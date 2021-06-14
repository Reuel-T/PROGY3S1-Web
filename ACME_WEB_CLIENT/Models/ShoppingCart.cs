using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class ShoppingCart
    {
        public int Sid { get; set; }
        public int Qty { get; set; }
        public int Uid { get; set; }
        public int Pid { get; set; }

        public virtual Product PidNavigation { get; set; }
        public virtual User UidNavigation { get; set; }
    }
}
