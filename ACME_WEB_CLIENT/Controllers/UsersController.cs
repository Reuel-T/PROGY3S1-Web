using ACME_WEB_CLIENT.Models;
using ACME_WEB_CLIENT.Utility;
using ACME_WEB_CLIENT.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.Controllers
{
    public class UsersController : Controller
    {
        private HttpClient HttpClient = new HttpClient();
        
        public UsersController() 
        {
            HttpClient.BaseAddress = new Uri("https://localhost:44346/api/users/");
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //clears the session for a new user to log in
        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpGet]
        public IActionResult SignUp() 
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> SignUp(NewUserVM nu) 
        {
            #region User Validation
            bool passwordValid = false;
            bool passwordsMatch = false;

            //checks if a password is a suitable length
            if (nu.Password.Length > 3)
            {
                passwordValid = true;
            }

            //checks if the two password fields contain the same values
            if (nu.Password.Equals(nu.PasswordConfirm))
            {
                passwordsMatch = true;
            }

            //passes the error to the model
            if (passwordValid == false)
            {
                ModelState.AddModelError("Password", "Password is not long enough");
            }

            //passes the error to the model
            if (passwordsMatch == false)
            {
                ModelState.AddModelError("PasswordConfirm", "Passwords do not match");
            }
            #endregion

            //preps the request
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            User u = new User();

            u.username = nu.Username.Trim();
            u.password = Encryptor.hashString(nu.Password.Trim() + nu.Username.Trim());
            u.rid = 12; //Role ID for standard user

            //serializes object to JSON
            string json = JsonSerializer.Serialize(u);

            //preps JSON String for httpRequest
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ByteArrayContent b = new ByteArrayContent(buffer);
            b.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //makes the request
            HttpResponseMessage response = await HttpClient.PostAsync("signup", b);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                User responseUser = JsonSerializer.Deserialize<User>(responseContent);

                TempData["msg"] = "Registration Successful, Please Log In to Continue";

                return RedirectToAction("Login");
            }
            else
            {
                Console.WriteLine("Login Failed");
                string responseContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("PasswordConfirm", "Sign Up Failed");
                Console.WriteLine(responseContent);
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login) 
        {
            //preps the request
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            login.Username = login.Username.Trim();
            login.Password = Encryptor.hashString(login.Password.Trim() + login.Username.Trim());

            //serializes object to JSON
            string json = JsonSerializer.Serialize(login);

            //preps JSON String for httpRequest
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ByteArrayContent b = new ByteArrayContent(buffer);
            b.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //makes the request
            HttpResponseMessage response = await HttpClient.PostAsync("login", b);

            if (response.IsSuccessStatusCode) 
            {
                //if the login is successful
                Console.WriteLine("Login Passed");
                string responseContent = await response.Content.ReadAsStringAsync();

                User u = JsonSerializer.Deserialize<User>(responseContent);

                //save user id to a session variable
                HttpContext.Session.SetInt32("UID", u.uid);
                HttpContext.Session.SetInt32("RID", u.rid);
                HttpContext.Session.SetString("USERNAME", u.username);

                return RedirectToAction("Index", "Products");
            }
            else 
            {
                Console.WriteLine("Login Failed");
                string responseContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("Password", "Login Failed");
                Console.WriteLine(responseContent);
                return View();
            }  
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Products");
        }
    }
}
