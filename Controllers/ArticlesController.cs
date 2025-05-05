using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using ScientificArticleManagement.Data;
using ScientificArticleManagement.Models;
using System;
using System.Linq;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScientificArticleManagement.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách Topics cho dropdown
        private List<Topic> GetTopicList() => _context.Topics.ToList();

        [HttpGet]
        public IActionResult All(
            string title, string author,
            string status, 
            int? topicId, 
            int? page = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var articles = _context.Articles
               .Include(a => a.Author)
               .Include(a => a.Topic)
               .AsQueryable();

            // Nếu không phải admin => chỉ hiển thị bài đã duyệt
            if (role != "Admin")
                articles = articles.Where(a => a.Status == "Approved");
            ViewBag.Role = role;

            // 🔍 Filter
            if (!string.IsNullOrEmpty(title))
                articles = articles.Where(a => a.Title.ToLower().Contains(title.ToLower()));

            if (!string.IsNullOrEmpty(author))
                articles = articles.Where(a => a.Author.FullName.ToLower().Contains(author.ToLower()));

            if (!string.IsNullOrEmpty(status))
                articles = articles.Where(a => a.Status == status);

            if (topicId.HasValue)
                articles = articles.Where(a => a.TopicId == topicId.Value);

            articles = articles.OrderBy(a => a.SubmissionDate);
            // 📄 Phân trang
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Gửi dữ liệu cho dropdown và giữ filter
            //ViewBag.Topics = GetTopicList();
            ViewBag.Topics = _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName);
            ViewBag.SearchTitle = title;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedTopicId = topicId; 

            return View(articles.OrderByDescending(a => a.SubmissionDate).ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult MyArticles(string title, string status, int? topicId, int? page = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var articles = _context.Articles
                .Where(a => a.UserId == userId.Value)
                .Include(a => a.Topic)
                .Include(a => a.Author)
                .OrderByDescending(a => a.SubmissionDate)
                .AsQueryable();

            // Filter logic (giữ nguyên)
            if (!string.IsNullOrEmpty(title))
                articles = articles.Where(a => a.Title.Contains(title));

            if (!string.IsNullOrEmpty(status))
                articles = articles.Where(a => a.Status == status);

            if (topicId.HasValue)
                articles = articles.Where(a => a.TopicId == topicId.Value);

            // Pagination
            int pageSize = 9;
            int pageNumber = page ?? 1;

            ViewBag.Topics = _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName);
            ViewBag.SearchTitle = title;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedTopicId = topicId;
            ViewBag.userId = userId;

            return View(articles.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            ViewBag.UserId = userId;
            Console.WriteLine($"❌ UserId: {userId} ");

            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }


            var article = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Topic)
                .FirstOrDefault(a => a.Id == id);


            if (article == null)
            {
                Console.WriteLine("❌ Không tìm thấy bài viết!");
                return NotFound();
            }

            ViewBag.Role = HttpContext.Session.GetString("Role");
            article.CurrentView++;
            _context.SaveChangesAsync();

            return View(article);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Author")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Topics = new SelectList(
                _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName),
                "Key",
                "Value"
            );
            return View();
        }

        [HttpPost]
        public IActionResult Create(Article article)
        {
            if (HttpContext.Session.GetString("Role") != "Author")
                return RedirectToAction("AccessDenied", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Gán giá trị trước khi kiểm tra ModelState
            article.UserId = userId.Value;
            article.SubmissionDate = DateTime.Now;
            article.Status = "Pending";

            // Clear các lỗi validation cho các trường đã set thủ công
            ModelState.Remove("UserId");
            ModelState.Remove("Topic");
            ModelState.Remove("Status");
            ModelState.Remove("Author"); // Nếu không yêu cầu validate Author

            if (ModelState.IsValid)
            {
                _context.Articles.Add(article);
                _context.SaveChanges();

                TempData["Success"] = "✅ Đã gửi bài viết.";
                return RedirectToAction("MyArticles", new { id = article.UserId });
            }
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"📌 Nhận được TopicId: {article.TopicId}");

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"❌ Field '{state.Key}': {error.ErrorMessage}");
                    }
                }
            }

            ViewBag.Topics = new SelectList(
                _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName),
                "Key",
                "Value"
            );
            return View(article);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Author")
                return RedirectToAction("AccessDenied", "Account");

            var article = _context.Articles.FirstOrDefault(a => a.Id == id);
            if (article == null || article.Status != "Pending")
            {
                TempData["Error"] = "❌ Không thể sửa bài đã duyệt/từ chối.";
                return RedirectToAction("MyArticles");
            }

            ViewBag.Topics = new SelectList(
                _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName),
                "Key",
                "Value"
            );
            return View(article);
        }

        [HttpPost]
        public IActionResult Edit(Article article)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (HttpContext.Session.GetString("Role") != "Author")
                return RedirectToAction("AccessDenied", "Account");

            var existing = _context.Articles
                .AsNoTracking() // Tắt tracking để tránj các vấn đề về navigation properties
                .FirstOrDefault(a => a.Id == article.Id && a.UserId == userId);
            if (existing == null || existing.Status != "Pending")
            {
                TempData["Error"] = "❌ Không thể sửa bài đã duyệt/từ chối.";
                return RedirectToAction("MyArticles", new { id = article.UserId });
            }
            // Tạo model mới chỉ với các trường cần thiết
            var modelToUpdate = new Article
            {
                Id = article.Id,
                UserId = existing.UserId,
                SubmissionDate = existing.SubmissionDate,
                Status = existing.Status,
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
                TopicId = article.TopicId
            };

            // Attach và chỉ update các trường cần thiết
            _context.Articles.Attach(modelToUpdate);
            _context.Entry(modelToUpdate).Property(x => x.Title).IsModified = true;
            _context.Entry(modelToUpdate).Property(x => x.Summary).IsModified = true;
            _context.Entry(modelToUpdate).Property(x => x.Content).IsModified = true;
            _context.Entry(modelToUpdate).Property(x => x.TopicId).IsModified = true;


            try
            {
                _context.SaveChanges();
                TempData["Success"] = "✅ Đã cập nhật bài viết.";
                return RedirectToAction("MyArticles", new { id = userId });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"❌ Lỗi khi cập nhật: {ex.Message}");
                TempData["Error"] = "❌ Có lỗi xảy ra khi cập nhật bài viết.";
            }

            ViewBag.Topics = new SelectList(
                _context.Topics.ToDictionary(t => t.TopicId, t => t.TopicName),
                "Key",
                "Value"
            );
            return View(article);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (HttpContext.Session.GetString("Role") != "Author")
                return RedirectToAction("AccessDenied", "Account");

            var article = _context.Articles.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (article == null || article.Status != "Pending")
            {
                TempData["Error"] = "❌ Không thể xóa bài đã duyệt/từ chối.";
                return RedirectToAction("MyArticles", new { id = userId });
            }

            _context.Articles.Remove(article);
            _context.SaveChanges();

            TempData["Success"] = "✅ Đã xóa bài viết.";
            return RedirectToAction("MyArticles", new { id = userId });
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var article = _context.Articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return NotFound();

            article.Status = "Approved";
            article.AcceptedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "✅ Đã duyệt bài.";
            return RedirectToAction("All");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("AccessDenied", "Account");

            var article = _context.Articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return NotFound();

            article.Status = "Rejected";
            article.DeniedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "❌ Đã từ chối bài.";
            return RedirectToAction("All");
        }
    }
}
