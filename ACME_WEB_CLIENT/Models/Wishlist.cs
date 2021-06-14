using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class Wishlist
    {
        public int Wid { get; set; }
        public int Uid { get; set; }
        public int Pid { get; set; }

        public virtual Product PidNavigation { get; set; }
        public virtual User UidNavigation { get; set; }
    }
}
