using System.ComponentModel.DataAnnotations;
using WebTN_MVC.Models.Product;

namespace WebTN_MVC.Areas.Product.Models
{
    public class CreateProductModel : WebTN_MVC.Models.Product.Product
    {
        [Display(Name ="Chuyên mục")]
        public int[]? CategoryId { get; set; }
    }
}