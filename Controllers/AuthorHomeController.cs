using Microsoft.AspNetCore.Mvc;
using ScientificArticleManagement.Data;
using ScientificArticleManagement.Models;

namespace ScientificArticleManagement.Controllers
{
    public class AuthorHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Author")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewBag.Role = role;

            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                ViewBag.FullName = user.FullName;
                ViewBag.UserImage = user.Image;
            }

            // Example data fetching - replace with your actual article/course logic
            ViewBag.TopArticles = _context.Articles
                .Where(a => a.Status == "Approved")
                .OrderByDescending(a => a.CurrentView)
                .Take(6)
                .ToList();

            ViewBag.NewestArticles = _context.Articles
                .Where(a => a.Status == "Approved")
                .OrderByDescending(a => a.AcceptedDate)
                .Take(6)
                .ToList();

            ViewBag.Topics = _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName);

            ViewBag.ArticlesByTopic = _context.Articles
                .Where(a => a.Status == "Approved")
                .GroupBy(a => a.TopicId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.CurrentView).Take(6).ToList()
                );

            return View();
        }
    }
}