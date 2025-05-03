using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ScientificArticleManagement.Data;
using ScientificArticleManagement.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using System.Data;

namespace ScientificArticleManagement.Controllers
{
    //[Route("Users")]
    //[Route("User")]
    public class UsersController : BaseUserController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Add()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View(user);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            if (role == "Admin")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Index", "UserHome");
        }

        [HttpGet]
        public IActionResult AddAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewBag.IsAddAdmin = true;
            return View("Add");
        }

        [HttpPost]
        public IActionResult AddAdmin(User user, IFormFile ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("Role");

            // Check for duplicate Username & Email
            if (_context.Users.Any(u => u.Username == user.Username))
                ModelState.AddModelError("Username", "Username is already taken.");
            if (_context.Users.Any(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Email is already registered.");

            if (!ModelState.IsValid)
            {
                ViewBag.IsAddAdmin = true;
                ViewBag.DebugErrors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return View("Add", user);
            }

            user.Role = "Admin";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                user.Image = "/images/" + fileName;
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("AllUser");
        }

        [HttpPost]
        public IActionResult Add(User user, IFormFile? ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ModelState.Remove("Role");

            // Check for duplicate Username & Email
            if (_context.Users.Any(u => u.Username == user.Username))
                ModelState.AddModelError("Username", "Username is already taken.");
            if (_context.Users.Any(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Email is already registered.");

            if (!ModelState.IsValid)
            {
                ViewBag.DebugErrors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return View(user);
            }

            user.Role = "Author";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                user.Image = "/images/" + fileName;
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("All");
        }

        public IActionResult All(string name, string email, string gender, string username, string sortBy, string sortDir, int? page)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var users = _context.Users.AsQueryable();
            users = users.Where(u => u.Role == "Author");

            // Filtering
            if (!string.IsNullOrEmpty(name))
                users = users.Where(u => u.FullName.ToLower().Contains(name.ToLower().Trim()));

            if (!string.IsNullOrEmpty(email))
                users = users.Where(u => u.Email.ToLower().Contains(email.ToLower().Trim()));

            if (!string.IsNullOrEmpty(gender))
                users = users.Where(u => u.Gender.ToLower().Contains(gender.ToLower().Trim()));

            if (!string.IsNullOrEmpty(username))
                users = users.Where(u => u.Username.ToLower().Contains(username.ToLower().Trim()));

            // Paging
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var pagedList = users.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        [Route("Details/{id}")]
        public IActionResult Details(int id)
        {
            Console.WriteLine($"Trying to fetch user with ID: {id}");
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.RoleView = user.Role;
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user, IFormFile? ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("Role");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState Invalid - Errors:");

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in field '{state.Key}': {error.ErrorMessage}");
                    }
                }

                return View(user);
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            bool emailExists = _context.Users.Any(u => u.Email == user.Email && u.UserId != user.UserId);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered by another user.");
                return View(user);
            }

            // Update basic fields
            existingUser.Username = user.Username;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = user.Password;
            }
            ViewBag.Role = HttpContext.Session.GetString("Role");
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Address = user.Address;
            existingUser.BirthDate = user.BirthDate;
            existingUser.Gender = user.Gender;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                existingUser.Image = "/images/" + fileName;
            }

            _context.Users.Update(existingUser);
            _context.SaveChanges();

            if (ViewBag.Role == "Admin")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Details", new { id = existingUser.UserId });
        }

        public IActionResult AllUser(string username, string email, string role)
        {
            var currentRole = HttpContext.Session.GetString("Role");
            if (currentRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(username))
                users = users.Where(u => u.Username.Contains(username));

            if (!string.IsNullOrEmpty(email))
                users = users.Where(u => u.Email.Contains(email));

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);

            // Set ViewBag for filters
            ViewBag.Username = username;
            ViewBag.Email = email;
            ViewBag.RoleFilter = role;

            return View("AllUser", users.ToList());
        }

        public IActionResult AllAuthors(string username, string email, string role)
        {
            var currentRole = HttpContext.Session.GetString("Role");
            if (currentRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(username))
                users = users.Where(u => u.Username.Contains(username));

            if (!string.IsNullOrEmpty(email))
                users = users.Where(u => u.Email.Contains(email));

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);

            // Set ViewBag for filters
            ViewBag.Username = username;
            ViewBag.Email = email;
            ViewBag.RoleFilter = role;

            return View("AllAuthors", users.ToList());
        }

    }
}