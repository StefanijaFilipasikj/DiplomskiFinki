using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomskiFinki.Data;

namespace DiplomskiFinki.Controllers
{
    public class DiplomasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiplomasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Diplomas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Diplomas.ToListAsync());
        }
    }
}
