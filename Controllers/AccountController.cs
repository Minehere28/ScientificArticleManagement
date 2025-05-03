using Microsoft.AspNetCore.Mvc;
using Mono.TextTemplating;
using ScientificArticleManagement.Data;
using ScientificArticleManagement.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScientificArticleManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            ModelState.Remove("Role");
            ModelState.Remove("Image");

            // Set default role for new users
            user.Role = "Author";

            // Check for existing username
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                Console.WriteLine("Username đã tồn tại");
                ModelState.AddModelError("Username", "Username already exists.");
                return View(user);
            }

            // Check for existing email
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState hợp lệ, bắt đầu lưu vào DB");
                try
                {
                    user.Role = "Author";
                    _context.Users.Add(user);
                    int recordsAffected = _context.SaveChanges();
                    Console.WriteLine($"Saved {recordsAffected} records");
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Error: {ex.Message}");
                    ModelState.AddModelError("", "Error saving to database");
                }
            }

            // Log validation errors
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"❌ Error in field '{state.Key}': {error.ErrorMessage}");
                    }
                }
            }

            return View(user);
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Set session values
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Role", user.Role);

                HttpContext.Session.CommitAsync().Wait();

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("Index", "AuthorHome");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}