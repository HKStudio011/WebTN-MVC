using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTN_MVC.Models.Product {

    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        [Key]
        public int Id { get; set; }

        // abc.png, 123.jpg ... 
        // => /contents/Products/abc.pgn, /contents/Products/123.jpg
        public string FileName { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }


    }
}