using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ScientificArticleManagement.Data;

namespace ScientificArticleManagement.Controllers
{
    public class BaseUserController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = HttpContext.Session.GetString("Role");

            // For Author role (equivalent to previous Student role)
            if (role == "Author")
            {
                var id = HttpContext.Session.GetInt32("UserId");
                var user = _context.Users.FirstOrDefault(u => u.UserId == id);

                if (user != null)
                {
                    // You can add any user-specific data to ViewBag here
                    ViewBag.FullName = user.FullName;
                    ViewBag.UserImage = user.Image;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
