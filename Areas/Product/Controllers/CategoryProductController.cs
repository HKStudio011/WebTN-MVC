using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Asn1.Cms;
using WebTN_MVC.Data;
using WebTN_MVC.Models;
using WebTN_MVC.Models.Product;

namespace WebTN_MVC.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("/admin/product/category/{action}/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryProductController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CategoryProductController> _logger;

        public CategoryProductController(AppDBContext context, ILogger<CategoryProductController> loger)
        {
            _context = context;
            _logger = loger;
        }


        // GET: Category
        public async Task<IActionResult> Index()
        {
            var pr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var categories = (await pr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ParentCategoryId"] = await GetSelectList();
            return View();
        }

        private void CreateSelectListItem(List<CategoryProduct> source, List<CategoryProduct> des, int level)
        {
            foreach (var item in source)
            {
                string prefix = string.Concat(Enumerable.Repeat("----", level));
                des.Add(new CategoryProduct()
                {
                    Id = item.Id,
                    Title = prefix + " " + item.Title
                });
                if (item.CategoryChildren?.Count > 0)
                {
                    CreateSelectListItem(item.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }

        private async Task<SelectList> GetSelectList()
        {
            var pr = (from c in _context.CategoryProducts select c)
                        .Include(c => c.ParentCategory)
                        .Include(c => c.CategoryChildren);

            var categories = (await pr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryProduct()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var listItem = new List<CategoryProduct>();

            CreateSelectListItem(categories, listItem, 0);

            var selectList = new SelectList(listItem, "Id", "Title");

            return selectList;
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ParentCategoryId"] = await GetSelectList();
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["ParentCategoryId"] = await GetSelectList();
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                canUpdate = false;
            }

            // Kiem tra thiet lap muc cha phu hop
            if (canUpdate && category.ParentCategoryId != null)
            {
                var childCates =
                            (from c in _context.CategoryProducts select c)
                            .Include(c => c.CategoryChildren)
                            .ToList()
                            .Where(c => c.ParentCategoryId == category.Id);


                // Func check Id 
                Func<List<CategoryProduct>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                    {
                        foreach (var cate in cates)
                        {
                            if (cate.Id == category.ParentCategoryId)
                            {
                                canUpdate = false;
                                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                                return true;
                            }
                            if (cate.CategoryChildren != null)
                                return checkCateIds(cate.CategoryChildren.ToList());

                        }
                        return false;
                    };
                // End Func 
                checkCateIds(childCates.ToList());
            }

            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (category.ParentCategoryId == -1) category.ParentCategoryId = null;

                    var dtc = _context.CategoryProducts.FirstOrDefault(c => c.Id == id);
                    _context.Entry(dtc).State = EntityState.Detached;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = await GetSelectList();
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryProducts.Include(c => c.CategoryChildren)
                                                    .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            foreach (var categoryChildren in category.CategoryChildren)
            {
                categoryChildren.ParentCategoryId = category.ParentCategoryId;
            }

            _context.CategoryProducts.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
