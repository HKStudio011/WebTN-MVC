using System.ComponentModel.DataAnnotations.Schema;

namespace WebTN_MVC.Models.Product
{
    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductID { set; get; }

        public int CategoryID { set; get; }

        [ForeignKey("ProductID")]
        public Product Product { set; get; }

        [ForeignKey("CategoryID")]
        public CategoryProduct Category { set; get; }
    }
}