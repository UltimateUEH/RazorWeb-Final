using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RazorWeb.Models;

namespace RazorWeb.Pages_Blog
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly RazorWeb.Models.AppDbContext _context;

        public IndexModel(RazorWeb.Models.AppDbContext context)
        {
            _context = context;
        }

        public IList<Article> Article { get;set; } = default!;

        public const int ITEMS_PER_PAGE = 15;

        [BindProperty(SupportsGet = true, Name = "p")]
        public int currentPage { get; set; }

        public int countPages { get; set; }

        public async Task OnGetAsync(string SearchString)
        {
            //Article = await _context.Articles.ToListAsync();

            int totalArtile = await _context.Articles.CountAsync();

            countPages = (int)Math.Ceiling((double)totalArtile / ITEMS_PER_PAGE);

            if (currentPage < 1)
            {
                currentPage = 1;
            }
            else if (currentPage > countPages)
            {
                currentPage = countPages;
            }

            var query = (from a in _context.Articles
                        orderby a.Created descending
                        select a)
                        .Skip((currentPage - 1) * 10)
                        .Take(ITEMS_PER_PAGE);

            if (!string.IsNullOrEmpty(SearchString))
            {
                Article = query.Where(a => a.Title.Contains(SearchString)).ToList();
            }
            else
            {
                Article = await query.ToListAsync();
            }
        }
    }
}
