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
using WebTN_MVC.Areas.Blog.Models;
using WebTN_MVC.Models.Blog;
using Microsoft.AspNetCore.Identity;
using WebTN_MVC.Utilities;

namespace WebTN_MVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("/admin/blog/post/{action}/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public PostController(AppDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var posts = _context.Posts
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            int totalPosts = await posts.CountAsync();
            if (pagesize <= 0) pagesize = 10;
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);

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
            ViewBag.totalPosts = totalPosts;

            ViewBag.postIndex = (currentPage - 1) * pagesize;

            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                             .Take(pagesize)
                             .Include(p => p.PostCategories)
                             .ThenInclude(pc => pc.Category)
                             .ToListAsync();

            return View(postsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            await MultiSelectListCategroriesId();
            return View();
        }

        private async Task MultiSelectListCategroriesId()
        {
            var category = await _context.Categories.ToListAsync();
            ViewBag.categories = new MultiSelectList(category,
                                        nameof(Category.Id),
                                        nameof(Category.Title));
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryId")] CreatePostModel post)
        {
            if (!string.IsNullOrWhiteSpace(post.Slug))
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                await MultiSelectListCategroriesId();
                ModelState.AddModelError(nameof(Post.Slug), "Nhập slug khác.");
                return View(post);
            }

            if (ModelState.IsValid)
            {

                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user?.Id;
                _context.Add(post);
                if (post.CategoryId != null)
                {
                    foreach (var item in post.CategoryId)
                    {
                        _context.PostCategorys.Add(new PostCategory()
                        {
                            Post = post,
                            CategoryID = item,
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo bài viết mới";
                return RedirectToAction(nameof(Index));
            }
            await MultiSelectListCategroriesId();
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p=> p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryId  =  post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            await MultiSelectListCategroriesId();
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryId")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            
            if (!string.IsNullOrWhiteSpace(post.Slug))
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                await MultiSelectListCategroriesId();
                ModelState.AddModelError(nameof(Post.Slug), "Nhập slug khác.");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postNeedUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p=> p.PostId == id);

                    if(postNeedUpdate == null)
                    {
                        return NotFound();
                    }

                    postNeedUpdate.PostId = post.PostId;
                    postNeedUpdate.Title = post.Title;
                    postNeedUpdate.Content = post.Content;
                    postNeedUpdate.Description = post.Description;
                    postNeedUpdate.Slug = post.Slug;
                    postNeedUpdate.Published = post.Published;
                    postNeedUpdate.DateUpdated = DateTime.Now;

                    //update postcategory
                    if (post.CategoryId == null)
                    {
                        post.CategoryId = new int[] {};
                    } 

                    var oldCateIds = postNeedUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCateIds = post.CategoryId;

                    var removeCatePosts = from postCate in postNeedUpdate.PostCategories
                                          where !newCateIds.Contains(postCate.CategoryID)
                                          select postCate;
                    _context.PostCategorys.RemoveRange(removeCatePosts);

                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                     foreach (var CateId in addCateIds)
                     {
                         _context.PostCategorys.Add(new PostCategory(){
                             PostID = id,
                             CategoryID = CateId
                         });
                     }     

                    _context.Update(postNeedUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            await MultiSelectListCategroriesId();
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                StatusMessage = "Bạn vừa xóa bài viết: " + post.Title;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }

        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
