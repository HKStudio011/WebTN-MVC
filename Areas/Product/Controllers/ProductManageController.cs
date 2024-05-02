using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.Data;
using WebTN_MVC.Models;
using WebTN_MVC.Areas.Product;
using Microsoft.AspNetCore.Identity;
using WebTN_MVC.Utilities;
using WebTN_MVC.Models.Product;
using WebTN_MVC.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;

namespace WebTN_MVC.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("/admin/product/productmanage/{action}/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class ProductManageController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public ProductManageController(AppDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var products = _context.Products
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            int totalProducts = await products.CountAsync();
            if (pagesize <= 0) pagesize = 10;
            int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;

            ViewBag.productIndex = (currentPage - 1) * pagesize;

            var productsInPage = await products.Skip((currentPage - 1) * pagesize)
                             .Take(pagesize)
                             .Include(p => p.ProductCategoryProducts)
                             .ThenInclude(pc => pc.Category)
                             .ToListAsync();

            return View(productsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            await MultiSelectListCategroriesId();
            return View();
        }

        private async Task MultiSelectListCategroriesId()
        {
            var category = await _context.CategoryProducts.ToListAsync();
            ViewBag.categories = new MultiSelectList(category,
                                        nameof(CategoryProduct.Id),
                                        nameof(CategoryProduct.Title));
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryId,Price")] CreateProductModel product)
        {
            if (!string.IsNullOrWhiteSpace(product.Slug))
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                await MultiSelectListCategroriesId();
                ModelState.AddModelError(nameof(product.Slug), "Nhập slug khác.");
                return View(product);
            }

            if (ModelState.IsValid)
            {

                var user = await _userManager.GetUserAsync(this.User);
                product.DateCreated = product.DateUpdated = DateTime.Now;
                product.AuthorId = user?.Id;
                _context.Add(product);
                if (product.CategoryId != null)
                {
                    foreach (var item in product.CategoryId)
                    {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                        {
                            Product = product,
                            CategoryID = item,
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo sản phẩm mới";
                return RedirectToAction(nameof(Index));
            }
            await MultiSelectListCategroriesId();
            return View(product);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p => p.ProductCategoryProducts).FirstOrDefaultAsync(p=> p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            var productEdit = new CreateProductModel()
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                CategoryId  =  product.ProductCategoryProducts.Select(pc => pc.CategoryID).ToArray(),
                Price = product.Price
            };

            await MultiSelectListCategroriesId();
            return View(productEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryId,Price")] CreateProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            
            if (!string.IsNullOrWhiteSpace(product.Slug))
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id))
            {
                await MultiSelectListCategroriesId();
                ModelState.AddModelError(nameof(product.Slug), "Nhập slug khác.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productNeedUpdate = await _context.Products.Include(p => p.ProductCategoryProducts).FirstOrDefaultAsync(p=> p.ProductId == id);

                    if(productNeedUpdate == null)
                    {
                        return NotFound();
                    }

                    productNeedUpdate.ProductId = product.ProductId;
                    productNeedUpdate.Title = product.Title;
                    productNeedUpdate.Content = product.Content;
                    productNeedUpdate.Description = product.Description;
                    productNeedUpdate.Slug = product.Slug;
                    productNeedUpdate.Published = product.Published;
                    productNeedUpdate.DateUpdated = DateTime.Now;
                    productNeedUpdate.Price = product.Price;


                    //update postcategory
                    if (product.CategoryId == null)
                    {
                        product.CategoryId = new int[] {};
                    } 

                    var oldCateIds = productNeedUpdate.ProductCategoryProducts.Select(c => c.CategoryID).ToArray();
                    var newCateIds = product.CategoryId;

                    var removeCateProduct = from productCate in productNeedUpdate.ProductCategoryProducts
                                          where !newCateIds.Contains(productCate.CategoryID)
                                          select productCate;
                    _context.ProductCategoryProducts.RemoveRange(removeCateProduct);

                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                     foreach (var CateId in addCateIds)
                     {
                         _context.ProductCategoryProducts.Add(new ProductCategoryProduct(){
                             ProductID = id,
                             CategoryID = CateId
                         });
                     }     

                    _context.Update(productNeedUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật sản phẩm";
                return RedirectToAction(nameof(Index));
            }
            await MultiSelectListCategroriesId();
            return View(product);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                StatusMessage = "Bạn vừa xóa sản phẩm: " + product.Title;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }

        }

        private bool PostExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage = "Phải chọn file upload")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload { get; set; }
        }

        [HttpGet]
        public IActionResult UploadPhoto(int id)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                            .Include(p => p.Photos)
                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            ViewData["product"] = product;
            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                .Include(p => p.Photos)
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            ViewData["product"] = product;

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                            + Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);
                            
                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto() {
                    ProductId = product.ProductId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }


            return View(f);
        }   

        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                .Include(p => p.Photos)
                .FirstOrDefault();

            if (product == null)
            {
                return Json(
                    new {
                        success = 0,
                        message = "Product not found",
                    }
                );
            }

           var listphotos = product.Photos.Select(photo => new {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(
                new { 
                    success = 1,
                    photos = listphotos
                }
            );

            
        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            var photo = _context.ProductPhotos.Where(p => p.Id == id).FirstOrDefault();
            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var filename = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(filename);
            }         
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id, [Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                .Include(p => p.Photos)
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                            + Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);
                            
                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto() {
                    ProductId = product.ProductId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }


            return Ok();
        }   

    }
}
