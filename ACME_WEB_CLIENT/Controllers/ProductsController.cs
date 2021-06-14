using ACME_WEB_CLIENT.Models;
using ACME_WEB_CLIENT.Services;
using ACME_WEB_CLIENT.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.Controllers
{
    public class ProductsController : Controller
    {
        private HttpClient HttpClient = new HttpClient();
        private readonly CartService CartService;

        public ProductsController(CartService cs)
        {
            this.CartService = cs;
            HttpClient.BaseAddress = new Uri("https://localhost:44346/api/");
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //initial get products
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //gets products
            HttpResponseMessage response = await HttpClient.GetAsync("products/getproducts");

            //if got products
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                List<Product> products = JsonSerializer.Deserialize<List<Product>>(responseContent);

                //gets categories
                HttpResponseMessage catResponse = await HttpClient.GetAsync("categories/getcategories");

                //if got categories
                if (catResponse.IsSuccessStatusCode)
                {
                    string catResponseContent = await catResponse.Content.ReadAsStringAsync();

                    List<CidNavigation> categories = new List<CidNavigation>();
                    categories.Add(new CidNavigation { cid = 0, categoryName = "All" });
                    categories.AddRange(JsonSerializer.Deserialize<List<CidNavigation>>(catResponseContent));

                    ViewData["c"] = new SelectList(categories, "cid", "categoryName");

                    return View(products);
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        //get products on search
        [HttpPost]
        public async Task<IActionResult> Index(string s, string c)
        {
            //q and c are url parameters, the search term and category
            string query = s;

            if (query == null)
            {
                query = "";
            }

            int cat = int.Parse(c);

            HttpResponseMessage response = await HttpClient.GetAsync($"products/getproducts?q={query}&c={cat}");

            //if got products
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                List<Product> products = JsonSerializer.Deserialize<List<Product>>(responseContent);

                HttpResponseMessage catResponse = await HttpClient.GetAsync("categories/getcategories");

                //if got categories
                if (catResponse.IsSuccessStatusCode)
                {
                    string catResponseContent = await catResponse.Content.ReadAsStringAsync();

                    List<CidNavigation> categories = new List<CidNavigation>();
                    categories.Add(new CidNavigation { cid = 0, categoryName = "All" });
                    categories.AddRange(JsonSerializer.Deserialize<List<CidNavigation>>(catResponseContent));

                    ViewData["c"] = new SelectList(categories, "cid", "categoryName");

                    return View(products);
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int productID) //adds the selected product to the users shopping cart
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            else
            {
                //makes the request
                HttpResponseMessage response = await HttpClient.GetAsync($"Products/GetProduct/{productID}");

                //if the request is successful
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    Product p = JsonSerializer.Deserialize<Product>(responseContent);

                    //add the product to the user's cart
                    CartService.CartProduct = p;

                    //take the user to the cart screen
                    return RedirectToAction("Cart", "Products");
                }
                else
                {
                    Console.WriteLine(response.StatusCode.ToString());
                    return RedirectToAction("Index", "Products");
                }
            }
        }

        [HttpGet]
        public IActionResult Cart() //views the user's shopping cart
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            return View(CartService);
        }

        [HttpGet]
        public IActionResult ClearCart() //clears the user's shopping cart
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            CartService.CartProduct = null;
            return RedirectToAction("Cart", "Products");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout() //does a check out with the user's cart
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            else
            {
                if (CartService.CartProduct != null)
                {
                    //gets the product from the cart service
                    Product p = CartService.CartProduct;

                    ProductOrder po = new ProductOrder();
                    po.pid = p.pid;
                    po.uid = (int)HttpContext.Session.GetInt32("UID");
                    po.price = p.price;
                    po.qty = 1;//right now we only allow one product in the cart

                    //preps the request
                    HttpRequestMessage request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;

                    //serializes object to JSON
                    string json = JsonSerializer.Serialize(po);

                    //preps JSON String for httpRequest
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    ByteArrayContent b = new ByteArrayContent(buffer);
                    b.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //makes the request
                    HttpResponseMessage response = await HttpClient.PostAsync("Orders/PostProductOrder", b);

                    if (response.IsSuccessStatusCode)
                    {
                        //clear the cart after checking out
                        CartService.CartProduct = null;

                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseContent);

                        Console.WriteLine("Added Order");
                        //redirects the user to the product page
                        return RedirectToAction("ViewOrders");
                    }
                    else
                    {
                        //if there is an error, return to the cart screen
                        return RedirectToAction("Cart", "Products");
                    }
                }
                else
                {
                    //if there is an error, return to the cart screen
                    return RedirectToAction("Cart", "Products");
                }
            }

        }

        [HttpGet]
        public async Task<IActionResult> ViewOrders()
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            else
            {
                //object for post method
                User u = new User();
                u.uid = (int)HttpContext.Session.GetInt32("UID");

                //preps for request
                HttpRequestMessage request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;

                //serializes object to JSON
                string json = JsonSerializer.Serialize(u);

                //preps JSON String for httpRequest
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                ByteArrayContent b = new ByteArrayContent(buffer);
                b.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //makes the request
                HttpResponseMessage response = await HttpClient.PostAsync("orders/GetProductOrders", b);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    List<ProductOrder> orders = JsonSerializer.Deserialize<List<ProductOrder>>(responseContent);

                    orders = orders.OrderByDescending(x => x.date).ToList();

                    Console.WriteLine(responseContent);

                    return View(orders);
                }
                else
                {
                    return RedirectToAction("Index", "Products");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            else 
            {
                //if the user is not an admin
                if(HttpContext.Session.GetInt32("RID") != 10) 
                {
                    TempData["msg"] = "Please Log In with an admin account to use this Feature";
                    return RedirectToAction("login", "Users");
                }
                else 
                {
                    //gets categories for a select list
                    HttpResponseMessage catResponse = await HttpClient.GetAsync("categories/getcategories");

                    if (catResponse.IsSuccessStatusCode)
                    {
                        //if we do get the categories
                        string catResponseContent = await catResponse.Content.ReadAsStringAsync();

                        List<CidNavigation> categories = new List<CidNavigation>();
                        categories.AddRange(JsonSerializer.Deserialize<List<CidNavigation>>(catResponseContent));

                        ViewData["cats"] = new SelectList(categories, "cid", "categoryName");
                    }
                    else
                    {
                        return View();
                    }

                    return View();
                    //user is an admin
                }
                
            }//user is not logged in
        }





        [HttpPost]
        public async Task<IActionResult> Analytics(AnalyticsVM analyticsVm)
        {
            //if the user is not signed in, redirect them to login
            if (HttpContext.Session.GetInt32("UID") == null)
            {
                TempData["msg"] = "Please Log In to Use this Feature";
                return RedirectToAction("login", "Users");
            }
            else 
            {
                //if the user is not an admin
                if (HttpContext.Session.GetInt32("RID") != 10)
                {
                    TempData["msg"] = "Please Log In with an admin account to use this Feature";
                    return RedirectToAction("login", "Users");
                }
                else 
                {
                    //gets the start and end dates for analytics
                    DateTime startDate = analyticsVm.StartDate.Date;
                    DateTime endDate = analyticsVm.EndDate.Date;

                    //validate dates
                    if (endDate <= startDate)
                    {
                        ModelState.AddModelError("EndDate", "End Date cannot be earlier or equal to start date");
                        return View(analyticsVm);
                    }
                    else
                    {
                        //Dates are Valid

                        //gets categories for a select list
                        HttpResponseMessage catResponse = await HttpClient.GetAsync("categories/getcategories");

                        if (catResponse.IsSuccessStatusCode)
                        {
                            //if we do get the categories
                            string catResponseContent = await catResponse.Content.ReadAsStringAsync();

                            List<CidNavigation> categories = new List<CidNavigation>();
                            categories.AddRange(JsonSerializer.Deserialize<List<CidNavigation>>(catResponseContent));

                            ViewData["cats"] = new SelectList(categories, "cid", "categoryName");

                            TempData["selectedCat"] = categories.Where(x => x.cid == int.Parse(analyticsVm.CategoryID)).First().categoryName;

                            //get the products
                            //preps the request
                            HttpRequestMessage request = new HttpRequestMessage();
                            request.Method = HttpMethod.Post;

                            //serializes object to JSON
                            string json = JsonSerializer.Serialize(new AnalyticsVM { StartDate = startDate, EndDate = endDate, CategoryID = analyticsVm.CategoryID });

                            //preps JSON String for httpRequest
                            byte[] buffer = Encoding.UTF8.GetBytes(json);
                            ByteArrayContent b = new ByteArrayContent(buffer);
                            b.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                            //makes the request
                            HttpResponseMessage response = await HttpClient.PostAsync("orders/getanalytics", b);

                            //if we got the analytics
                            if (response.IsSuccessStatusCode)
                            {
                                //if the login is successful
                                string responseContent = await response.Content.ReadAsStringAsync();

                                //list of orders
                                List<ProductOrder> orders = JsonSerializer.Deserialize<List<ProductOrder>>(responseContent);

                                if (orders.Count > 0)
                                {
                                    List<SimpleReportViewModel> reportModel = GetReportData(orders, GetDatesInRange(startDate, endDate));

                                    //saves report data to be passed to another view
                                    TempData["reportModel"] = JsonSerializer.Serialize(reportModel);

                                    return RedirectToAction("Line");
                                }//returns
                                else
                                {
                                    //no orders for analytics
                                    return View();
                                }//returns
                            }//if response successful
                            else
                            {
                                //if response fails
                                return View();
                            }//returns
                        }//if response successful
                        else
                        {
                            //error return
                            return View();
                        }//response failed
                    }//else - Dates Invalid
                }
            }
        }//analytics

        //gets the dates for a selected date range
        public List<DateTime> GetDatesInRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();

            while (startDate <= endDate)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(1);
            }
            return dates;
        }

        //sorts the data into a model for analytics graph
        public List<SimpleReportViewModel> GetReportData(List<ProductOrder> orders, List<DateTime> dates)
        {
            List<SimpleReportViewModel> model = new List<SimpleReportViewModel>();

            foreach (DateTime date in dates)
            {
                model.Add(new SimpleReportViewModel
                {
                    DimensionOne = date.ToString("dd MMMM yyyy"),
                    Quantity = orders.Where(x => x.date.Date == date.Date).Count()
                });
            }

            return model;
        }

        //displays analytics graph
        [HttpGet]
        public IActionResult Line() 
        {
            List<SimpleReportViewModel> lstModel;

            if (TempData.ContainsKey("reportModel")) 
            {
                lstModel = JsonSerializer.Deserialize<List<SimpleReportViewModel>>(TempData["reportModel"].ToString());
                return View(lstModel);
            }
            else
            {
                return RedirectToAction("Analytics");
            }
        }
    }
}
