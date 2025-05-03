using Microsoft.AspNetCore.Mvc;
using ScientificArticleManagement.Data;
using Microsoft.EntityFrameworkCore;
using ScientificArticleManagement.Models;
using System;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScientificArticleManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
            string searchString,
            string statusFilter,
            int? topicFilter,
            int? authorFilter,
            int? page = 1)
        {
            // Kiểm tra quyền Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Lấy dữ liệu bài viết
            var articles = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Topic)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(a => a.Title.Contains(searchString)
                    || a.Summary.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                articles = articles.Where(a => a.Status == statusFilter);
            }

            if (topicFilter.HasValue)
            {
                articles = articles.Where(a => a.TopicId == topicFilter.Value);
            }

            if (authorFilter.HasValue)
            {
                articles = articles.Where(a => a.UserId == authorFilter.Value);
            }

            // Thống kê
            ViewBag.TotalArticles = _context.Articles.Count();
            ViewBag.PendingArticles = _context.Articles.Count(a => a.Status == "Pending");
            ViewBag.ApprovedArticles = _context.Articles.Count(a => a.Status == "Approved");
            ViewBag.RejectedArticles = _context.Articles.Count(a => a.Status == "Rejected");

            // Thống kê theo tác giả
            ViewBag.AuthorsStats = _context.Articles
                .Include(a => a.Author)
                .GroupBy(a => a.Author.FullName)
                .Select(g => new
                {
                    AuthorName = g.Key,
                    ArticleCount = g.Count(),
                    PendingCount = g.Count(a => a.Status == "Pending"),
                    ApprovedCount = g.Count(a => a.Status == "Approved")
                })
                .OrderByDescending(x => x.ArticleCount)
                .ToList();

            // Thống kê theo chủ đề
            ViewBag.TopicsStats = _context.Articles
                .Include(a => a.Topic)
                .GroupBy(a => a.Topic.TopicName)
                .Select(g => new
                {
                    TopicName = g.Key,
                    ArticleCount = g.Count(),
                    PendingCount = g.Count(a => a.Status == "Pending"),
                    ApprovedCount = g.Count(a => a.Status == "Approved")
                })
                .OrderByDescending(x => x.ArticleCount)
                .ToList();

            // Dữ liệu cho dropdown filter
            ViewBag.Topics = _context.Topics.ToList();
            ViewBag.Authors = _context.Users.Where(u => u.Role == "Author").ToList();
            ViewBag.Statuses = new[] { "Pending", "Approved", "Rejected" };

            // Phân trang
            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(articles.OrderByDescending(a => a.SubmissionDate).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null)
            {
                return NotFound();
            }

            article.Status = "Approved";
            article.AcceptedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "Bài viết đã được duyệt thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null)
            {
                return NotFound();
            }

            article.Status = "Rejected";
            article.DeniedDate = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "Bài viết đã bị từ chối.";
            return RedirectToAction(nameof(Index));
        }
    }
}