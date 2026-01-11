using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class NvbTheLoaiController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public NvbTheLoaiController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== DANH SÁCH THỂ LOẠI =====
        public IActionResult Index()
        {
            var theLoai = _context.TheLoais.ToList();
            return View(theLoai);
        }

        // ===== CREATE =====
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TheLoai model)
        {
            if (ModelState.IsValid)
            {
                _context.TheLoais.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ===== EDIT =====
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var theLoai = _context.TheLoais.Find(id);
            if (theLoai == null) return NotFound();
            return View(theLoai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TheLoai model)
        {
            if (ModelState.IsValid)
            {
                _context.TheLoais.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
