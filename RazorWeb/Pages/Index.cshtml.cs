using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorWeb.Models;

namespace RazorWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _myBlogContext;

        public IndexModel(ILogger<IndexModel> logger, AppDbContext myBlogContext)
        {
            _logger = logger;
            _myBlogContext = myBlogContext;
        }

        public void OnGet()
        {
            var posts = (from a in _myBlogContext.Articles
                        orderby a.Created descending
                        select a).ToList();

            ViewData["Posts"] = posts;
        }
    }
}
