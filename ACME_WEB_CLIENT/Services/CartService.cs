using ACME_WEB_CLIENT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.Services
{
    public class CartService
    {
        //service that holds the user's shopping cart.
        //if time permits, move cart to DB instead
        public CartService() 
        {
            
        }

        public Product CartProduct { get; set; }
    }
}
