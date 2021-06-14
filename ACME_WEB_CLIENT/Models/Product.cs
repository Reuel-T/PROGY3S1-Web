using System;
using System.Collections.Generic;

#nullable disable

namespace ACME_WEB_CLIENT.Models
{
    public partial class Product
    {
        public Product()
        {
        }

        public int pid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public int cid { get; set; }
        public string imageUrl { get; set; }
        public CidNavigation cidNavigation { get; set; }
        public List<ProductOrder> productOrders { get; set; }
        public List<ShoppingCart> shoppingCarts { get; set; }
        public List<Wishlist> wishlists { get; set; }
    }
}
