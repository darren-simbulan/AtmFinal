using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using AtmFinal.Models;
namespace ATMSystem.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/BalanceInquiry
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
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
            string accountNumber = HttpContext.Session.GetString("account_number");

            if (string.IsNullOrEmpty(accountNumber))
            {
                ViewBag.Message = "User not logged in.";
                return View();
            }

            string connStr = _configuration.GetConnectionString("MyDbConnection");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // Get current balance
                decimal currentBalance = 0;

                using (var cmd = new MySqlCommand("SELECT balance FROM users WHERE card_number= @acc", conn))
                {
                    cmd.Parameters.AddWithValue("@acc", accountNumber);
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        ViewBag.Message = "Account not found.";
                        return View();
                    }

                    currentBalance = Convert.ToDecimal(result);
                }

                if (amount <= 0)
                {
                    ViewBag.Message = "Invalid amount.";
                    return View();
                }

                if (amount > currentBalance)
                {
                    ViewBag.Message = "Insufficient balance.";
                    return View();
                }

                // Update balance
                decimal newBalance = currentBalance - amount;

                using (var cmd = new MySqlCommand("UPDATE users SET balance = @bal WHERE card_number = @acc", conn))
                {
                    cmd.Parameters.AddWithValue("@bal", newBalance);
                    cmd.Parameters.AddWithValue("@acc", accountNumber);
                    cmd.ExecuteNonQuery();
                }
                HttpContext.Session.SetString("balance", newBalance.ToString());
                ViewBag.Message = $"Successfully withdrew ₱{amount}. Remaining balance: ₱{newBalance}";

                conn.Close();
            }

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
            string accountNumber = HttpContext.Session.GetString("account_number");

            if (string.IsNullOrEmpty(accountNumber))
            {
                ViewBag.Message = "User not logged in.";
                return View();
            }

            string connStr = _configuration.GetConnectionString("MyDbConnection");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // Get current balance
                decimal currentBalance = 0;

                using (var cmd = new MySqlCommand("SELECT balance FROM users WHERE card_number= @acc", conn))
                {
                    cmd.Parameters.AddWithValue("@acc", accountNumber);
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        ViewBag.Message = "Account not found.";
                        return View();
                    }

                    currentBalance = Convert.ToDecimal(result);
                }

                if (amount <= 0)
                {
                    ViewBag.Message = "Invalid amount.";
                    return View();
                }

              

                // Update balance
                decimal newBalance = currentBalance + amount;

                using (var cmd = new MySqlCommand("UPDATE users SET balance = @bal WHERE card_number = @acc", conn))
                {
                    cmd.Parameters.AddWithValue("@bal", newBalance);
                    cmd.Parameters.AddWithValue("@acc", accountNumber);
                    cmd.ExecuteNonQuery();
                }
                HttpContext.Session.SetString("balance", newBalance.ToString());
                ViewBag.Message = $"Successfully Deposit ₱{amount}. Remaining balance: ₱{newBalance}";

                conn.Close();
            }

            return View();
        }


        // Fund Transfer
        public IActionResult Transfer()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Transfer(string recipientAccountNumber, decimal amount)
        {
            string senderAccount = HttpContext.Session.GetString("account_number");

            if (string.IsNullOrEmpty(senderAccount))
            {
                ViewBag.Message = "You must be logged in.";
                return View();
            }

            if (amount <= 0)
            {
                ViewBag.Message = "Invalid amount.";
                return View();
            }

            string connStr = _configuration.GetConnectionString("MyDbConnection");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 1. Get sender balance
                    decimal senderBalance = 0;
                    using (var cmd = new MySqlCommand("SELECT balance FROM users WHERE card_number = @acc", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@acc", senderAccount);
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            ViewBag.Message = "Sender account not found.";
                            return View();
                        }

                        senderBalance = Convert.ToDecimal(result);
                    }

                    if (amount > senderBalance)
                    {
                        ViewBag.Message = "Insufficient funds.";
                        return View();
                    }

                    // 2. Get recipient balance
                    decimal recipientBalance = 0;
                    using (var cmd = new MySqlCommand("SELECT balance FROM users WHERE card_number = @rec", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@rec", recipientAccountNumber);
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            ViewBag.Message = "Recipient account not found.";
                            return View();
                        }

                        recipientBalance = Convert.ToDecimal(result);
                    }

                    // 3. Deduct from sender
                    using (var cmd = new MySqlCommand("UPDATE users SET balance = @bal WHERE card_number = @acc", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@bal", senderBalance - amount);
                        var newBalance = senderBalance - amount;
                        cmd.Parameters.AddWithValue("@acc", senderAccount);
                        cmd.ExecuteNonQuery();
                        HttpContext.Session.SetString("balance", newBalance.ToString());
                    }

                    // 4. Add to recipient
                    using (var cmd = new MySqlCommand("UPDATE users SET balance = @bal WHERE card_number = @rec", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@bal", recipientBalance + amount);
                        cmd.Parameters.AddWithValue("@rec", recipientAccountNumber);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    ViewBag.Message = $"Successfully transferred ₱{amount} to account {recipientAccountNumber}.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Message = $"Transfer failed: {ex.Message}";
                }

                conn.Close();
            }

            return View();
        }


    }

}