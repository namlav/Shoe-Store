using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebGiayyy.Models;

namespace WebGiayyy.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private LTWEntities db = new LTWEntities();

        private void LoadGroupList()
        {
            using (var db = new LTWEntities())
            {
                ViewBag.GroupList = new SelectList(
                    db.CategoryGroups.Where(g => g.IsActive == true),
                    "GroupId",
                    "GroupName"
                );
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public ActionResult CreateDoanhMuc(CategoryCreateVM model)
        {
            using (var db = new LTWEntities())
            {
                int groupId;

                // 👉 Tạo group mới
                if (!string.IsNullOrEmpty(model.NewGroupCode)
                    && !string.IsNullOrEmpty(model.NewGroupName))
                {
                    var newGroup = new CategoryGroup
                    {
                        GroupCode = model.NewGroupCode,
                        GroupName = model.NewGroupName,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.CategoryGroups.Add(newGroup);
                    db.SaveChanges();

                    groupId = newGroup.GroupId;
                }
                // 👉 Chọn group cũ
                else if (model.GroupId.HasValue)
                {
                    groupId = model.GroupId.Value;
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng chọn hoặc tạo nhóm danh mục");
                    LoadGroupList();
                    return View(model);
                }

                var category = new Category
                {
                    CatName = model.CatName,
                    CatSlug = model.CatSlug,
                    Description = model.Description,
                    GroupId = groupId,
                    SortOrder = model.SortOrder,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                db.Categories.Add(category);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/Products
        public ActionResult CreateDoanhMuc()
        {
            ViewBag.GroupList = new SelectList(
                db.CategoryGroups.Where(g => g.IsActive == true),
                "GroupId",
                "GroupName"
            );
            return View();
        }
        public ActionResult QuanLyDanhMuc()
        {
            var danhMuc = db.Categories.OrderByDescending(dm => dm.CreatedAt).ToList();
            return View(danhMuc);
        }

        public ActionResult QuanLySanPham()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }
        public ActionResult QuanLyNguoiDung()
        {
            var users = db.AppUsers
                          .OrderBy(u => u.UserId)                  
                          .ToList();
            return View(users);
        }
        public ActionResult QuanLyDonHang()
        {
            var orders = db.Orders
                           .OrderByDescending(o => o.OrderId)
                           .ToList();

            return View(orders);
        }
        public ActionResult ChiTietDonHang(int id)
        {
            var order = db.Orders
                          .Include(o => o.OrderItems.Select(i => i.Product))
                          .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return HttpNotFound();

            ViewBag.Order = order;

            // Danh sách trạng thái
            ViewBag.StatusList = new SelectList(
                new List<SelectListItem>
                {
            new SelectListItem { Text = "Chờ xử lý", Value = "Pending" },
            new SelectListItem { Text = "Đang giao", Value = "Shipping" },
            new SelectListItem { Text = "Hoàn thành", Value = "Completed" },
            new SelectListItem { Text = "Hủy", Value = "Canceled" }
                },
                "Value",
                "Text",
                order.Status
            );

            return View(order.OrderItems.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChiTietDonHang(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
                return HttpNotFound();

            order.Status = status;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật đơn hàng thành công!";
            return RedirectToAction("ChiTietDonHang", new { id = orderId });
        }
        public ActionResult CapNhatTrangThai(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
                return HttpNotFound();

            order.Status = status;
            db.SaveChanges();

            return RedirectToAction("ChiTietDonHang", new { id = orderId });
        }
        public ActionResult DoanhThu()
        {
            var today = DateTime.Today;

            ViewBag.DoanhThuHomNay = db.Orders
                .Where(o => o.CreatedAt >= today
                         && o.Status == "Completed")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.TongDonHang = db.Orders.Count();

            ViewBag.DonHoanThanh = db.Orders
                .Count(o => o.Status == "Completed");

            return View();
        }
        [HttpPost]
        public ActionResult DeleteMultiple(int[] selectedIds)
        {
            if (selectedIds != null)
            {
                var products = db.Products
                                 .Where(p => selectedIds.Contains(p.ProductId))
                                 .ToList();

                db.Products.RemoveRange(products);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
        [Bind(Include = "ProductId,CategoryId,SKU,ProductName,Slug,Summary,Price,Stock,IsActive")] Product product,
        HttpPostedFileBase MainImageFile)
        {
            if (ModelState.IsValid)
            {
                // Upload ảnh
                if (MainImageFile != null && MainImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(MainImageFile.FileName);
                    string folderPath = Server.MapPath("~/Uploads/Products");

                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, fileName);
                    MainImageFile.SaveAs(filePath);

                    product.MainImage = "/Uploads/Products/" + fileName;
                }

                // Set ngày tạo
                product.CreatedAt = DateTime.Now;

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }


        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase ImageUpload)
        {
            if (ModelState.IsValid)
            {
                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageUpload.FileName);

                    // 👉 ĐƯỜNG DẪN LƯU TRÊN SERVER

                    // 👉 LƯU FILE
                    product.MainImage = Path.GetFileName(ImageUpload.FileName);

                    // 👉 CHỈ LƯU TÊN ẢNH VÀO DB
                    product.MainImage = fileName;
                }
                // nếu không upload ảnh → giữ ảnh cũ (hidden field)

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }


        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return HttpNotFound();

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }
        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int ProductId)
        {
            var product = db.Products.Find(ProductId);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Products", new { area = "" });
        }
    }
}
