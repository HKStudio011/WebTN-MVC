using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.Data;
using WebTN_MVC.Models;
using WebTN_MVC.Models.Blog;
using WebTN_MVC.Models.Product;

namespace WebTN_MVC.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class DBManage : Controller
    {
        private readonly AppDBContext _dBContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public DBManage(AppDBContext appDBContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _dBContext = appDBContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: DBManage
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDBAsync()
        {
            var success = await _dBContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xoá Database thành công." : "Không thể xoá Database!!!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApplyMigrationAsync()
        {
            await _dBContext.Database.MigrateAsync();
            StatusMessage = "Cập nhật Database thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var r in roleNames)
            {
                var roleName = r.GetRawConstantValue() as string;
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // admin, pass = admin123, email = admin@example.com
            var admin = await _userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(admin, password: "admin123");
                await _userManager.AddToRoleAsync(admin, RoleName.Administrator);
            }

            await _signInManager.SignInAsync(admin,isPersistent:false);

            SenDPostCategory();
            SeedProductCategory();

            StatusMessage = "Vừa send data";
            return RedirectToAction("Index");
        }

        private void SeedProductCategory()
        {
        
            _dBContext.CategoryProducts.RemoveRange(_dBContext.CategoryProducts.Where(c => c.Description.Contains("[fakeData]")));
            _dBContext.Products.RemoveRange(_dBContext.Products.Where(p => p.Content.Contains("[fakeData]")));

            _dBContext.SaveChanges();

             var fakerCategory = new Faker<CategoryProduct>();
             int cm = 1;
             fakerCategory.RuleFor( c => c.Title,  fk => $"Nhom SP{cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
             fakerCategory.RuleFor( c => c.Description,  fk => fk.Lorem.Sentences(5) + "[fakeData]");
             fakerCategory.RuleFor( c => c.Slug,  fk => fk.Lorem.Slug());

           

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();


             cate11.ParentCategory  = cate1;
             cate12.ParentCategory  = cate1;
             cate21.ParentCategory = cate2;
             cate211.ParentCategory = cate21;

             var categories = new CategoryProduct[] { cate1, cate2, cate12, cate11, cate21, cate211};
            _dBContext.CategoryProducts.AddRange(categories);
            


            // POST
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerProduct = new Faker< WebTN_MVC.Models.Product.Product>();
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Commerce.ProductDescription() +"[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2021,7,1)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"SP {bv++} " + f.Commerce.ProductName()); 
            fakerProduct.RuleFor(p => p.Price,  f => int.Parse(f.Commerce.Price(500, 1000, 0)));
         
            List<WebTN_MVC.Models.Product.Product> products = new List<WebTN_MVC.Models.Product.Product>();
            List<ProductCategoryProduct> product_categories = new List<ProductCategoryProduct>();


            for (int i = 0; i < 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                product_categories.Add(new ProductCategoryProduct() { 
                    Product = product,
                    Category = categories[rCateIndex.Next(5)]
                });
            } 

            _dBContext.AddRange(products);
            _dBContext.AddRange(product_categories); 
            // END POST

            _dBContext.SaveChanges();
        }

        private void SenDPostCategory()
        {
            _dBContext.Categories.RemoveRange(_dBContext.Categories.Where(c => c.Description.Contains("[fakeData]")));
            _dBContext.Posts.RemoveRange(_dBContext.Posts.Where(p => p.Content.Contains("[fakeData]")));

            _dBContext.SaveChanges();

            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());


            //-
            var cate1 = fakerCategory.Generate();
            //--
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            //-
            var cate2 = fakerCategory.Generate();
            //--
            var cate21 = fakerCategory.Generate();
            //---
            var cate211 = fakerCategory.Generate();


            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new Category[] { cate1, cate2, cate12, cate11, cate21, cate211 };
            _dBContext.Categories.AddRange(categories);



            // POST
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(202, 1, 1), new DateTime(202, 7, 1)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> post_categories = new List<PostCategory>();


            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                post_categories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[rCateIndex.Next(categories.Length)]
                });
            }

            _dBContext.AddRange(posts);
            _dBContext.AddRange(post_categories);
            // END POST

            _dBContext.SaveChanges();
        }
    }
}
