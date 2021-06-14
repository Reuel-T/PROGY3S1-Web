using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class User
    {
        public User() { }
        public int uid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int rid { get; set; }
        public object ridNavigation { get; set; }
        public List<ProductOrder> productOrders { get; set; }
        public List<ShoppingCart> shoppingCarts { get; set; }
        public List<Wishlist> wishlists { get; set; }
    }
}
