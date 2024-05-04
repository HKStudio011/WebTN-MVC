
using System.Collections.Generic;
using WebTN_MVC.Models.Blog;
using WebTN_MVC.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace WebTN_MVC.Components
{
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent {

        public class CategorySidebarData 
        {
            public List<CategoryProduct> Categories { get; set; }
            public int level { get; set; }

            public string categoryslug { get; set;}

        }

        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }

    }
}