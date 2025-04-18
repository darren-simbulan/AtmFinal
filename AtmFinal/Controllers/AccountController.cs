using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace ATMSystem.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/BalanceInquiry
        public IActionResult BalanceIquiry()
        {

            string balance = HttpContext.Session.GetString("balance");

            ViewBag.Balance = @balance;
            return View();
        }

        // GET: /Account/Withdraw
        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Withdraw(decimal amount)
        {
            // Logic to withdraw from user's balance
            // Update database, then redirect or show result
            ViewBag.Message = $"You withdrew ₱{amount}";
            return View();
        }

        // GET: /Account/Deposit
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Deposit(decimal amount)
        {
            // Logic to add to user's balance
            ViewBag.Message = $"You deposited ₱{amount}";
            return View();
        }

        // GET: /Account/Transfer
        public IActionResult Transfer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Transfer(string recipientAccountNumber, decimal amount)
        {
            // Logic to transfer money to another account
            ViewBag.Message = $"Transferred ₱{amount} to account {recipientAccountNumber}";
            return View();
        }
    }

}
