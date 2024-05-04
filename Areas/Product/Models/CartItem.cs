using WebTN_MVC.Models.Product;

namespace WebTN_MVC.Areas.Product.Models 
{
    public class CartItem
    {
        public int quantity {set; get;}
        public WebTN_MVC.Models.Product.Product product {set; get;}
    }
}