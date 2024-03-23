using System.ComponentModel.DataAnnotations;
using WebTN_MVC.Areas.Blog.Controllers;
using WebTN_MVC.Models.Blog;

namespace WebTN_MVC.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name ="Chuyên mục")]
        public int[]? CategoryId { get; set; }
    }
}