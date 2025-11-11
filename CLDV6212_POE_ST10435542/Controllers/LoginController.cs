using Microsoft.AspNetCore.Mvc;

namespace CLDV6212_POE_ST10435542.Controllers
{
    // The LoginController is responsible for handling all the requests related to the login page
    public class LoginController : Controller
    {
        // Hardcoded the username and password into the LoginController so that it is not dependent on retrieving the login detaisl from the table in database. Will create a designated table for account management in later parts
        private const string Username = "admin";
        private const string Password = "123";

        // The Index method is responsible for returning the login page view
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        // The Login method is responsible for handling the login process. Passes the username and password to the view
        public IActionResult Login(string username, string password)
        {
            if (username == Username && password == Password)
            {
                // If the users username and password matches that in the Controller, then they'll be directed to home page (granted access)
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            // If the user is found, it redirects to the LoginController index view to handle the authentication
            return View("Index");
        }
    }
}
