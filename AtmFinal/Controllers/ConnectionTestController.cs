using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;

namespace AtmFinal.Controllers
{
    public class ConnectionTestController : Controller
    {
        private readonly IConfiguration _configuration;

        public ConnectionTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            string connStr = _configuration.GetConnectionString("MyDbConnection");
            string result;

            try
            {
                using var conn = new MySqlConnection(connStr);
                conn.Open();
                result = "✅ Successfully connected to the MySQL database!";
                conn.Close();
            }
            catch (Exception ex)
            {
                result = $"❌ Connection failed: {ex.Message}";
            }

            ViewBag.Result = result;
            return View();
        }
    }
}
