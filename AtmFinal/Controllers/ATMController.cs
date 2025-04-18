using AtmFinal.Models;
using AtmFinal.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtmFinal.Controllers
{
    public class ATMController : Controller
    {
        private readonly UserService _userService;

        public ATMController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string cardNumber, string pin)
        {
            var user = _userService.Login(cardNumber, pin);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("balance", user.Balance.ToString());
                HttpContext.Session.SetString("account_number", user.CardNumber.ToString());
                TempData["User"] = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid card number or PIN.";
            return View();
        }

        public IActionResult Dashboard()
        {
            if (!TempData.ContainsKey("User")) return RedirectToAction("Login");

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(TempData["User"].ToString());
            TempData.Keep("User");
            return View(user);
        }

        [HttpPost]
        public IActionResult Withdraw(decimal amount)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(TempData["User"].ToString());

            if (amount > 0 && user.Balance >= amount)
            {
                user.Balance -= amount;
                _userService.UpdateBalance(user.Id, user.Balance);
                TempData["User"] = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            }

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "ATM");

        }

    }
}
