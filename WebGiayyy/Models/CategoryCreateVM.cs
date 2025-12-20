using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebGiayyy.Models
{
    public class CategoryCreateVM
    {
        // Category
        public string CatName { get; set; }
        public string CatSlug { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public bool IsActive { get; set; }

        // Group
        public int? GroupId { get; set; }      // chọn group cũ
        public string NewGroupCode { get; set; } // tạo group mới
        public string NewGroupName { get; set; }
    }
}