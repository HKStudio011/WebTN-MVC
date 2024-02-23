using System;

namespace WebTN_MVC.Models
{
    public class PagingModel
    {
        public int currentpage { get; set; }
        public int countpages { get; set; }

        public Func<int?, string> generateUrl { get; set; }
        
    }
   
}