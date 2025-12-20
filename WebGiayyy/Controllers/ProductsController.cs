using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebGiayyy.Models;

namespace WebGiayyy.Controllers
{

    public class ProductsController : Controller
    {
        private LTWEntities db = new LTWEntities();
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult OrderDetails(int id)
        {
            var order = db.Orders
                .Include("OrderItems")
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        [HttpPost]
        public ActionResult Checkout(string FullName, string Phone, string Address)
        {
            if (Session["giohang"] == null)
                return RedirectToAction("Cart");

            var giohang = (Dictionary<int, CTGioHang>)Session["giohang"];

            using (var db = new LTWEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    decimal total = 0;

                    
                    Order order = new Order
                    {
                        UserId = Session["UserId"] as int?,
                        CustomerName = FullName,
                        Phone = Phone,
                        AddressLine = Address,
                        Status = "Pending",
                        CreatedAt = DateTime.Now
                    };

                    db.Orders.Add(order);
                    db.SaveChanges(); 

                
                    foreach (var item in giohang.Values)
                    {
                        var product = db.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

                        if (product == null)
                            throw new Exception("Sản phẩm không tồn tại");

                        if (product.Stock < item.Quantity)
                            throw new Exception($"Sản phẩm {product.ProductName} không đủ tồn kho");

                        
                        product.Stock -= item.Quantity;

                      
                        OrderItem oi = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price
                        };

                        db.OrderItems.Add(oi);

                        total += product.Price * item.Quantity;
                    }

                   
                    order.TotalAmount = total;

                    db.SaveChanges();
                    tran.Commit();

                    Session["LastOrderId"] = order.OrderId;
                    return RedirectToAction("Success");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    TempData["Error"] = ex.Message;
                    return RedirectToAction("Cart");
                }
            }
        }


        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Products");
            }

            if (Session["giohang"] == null)
            {
                return RedirectToAction("Cart");
            }

            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string email, string password, string fullname, string phone)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(fullname))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            
            var exist = db.AppUsers.Any(u => u.Email == email);
            if (exist)
            {
                ViewBag.Error = "Email đã được đăng ký";
                return View();
            }

            AppUser user = new AppUser
            {
                Email = email,
                PasswordHash = password, 
                FullName = fullname,
                Phone = phone,
                Role = "USER",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.AppUsers.Add(user);
            db.SaveChanges();

            // Auto login sau đăng ký
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Email"] = user.Email;
            Session["Role"] = user.Role;

            return RedirectToAction("Index", "Products");
        }
        public ActionResult Search(string keyword, string khoanggia)
        {
            var query = db.Products.AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductName.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            // Xử lý khoảng giá
            if (!string.IsNullOrEmpty(khoanggia) && khoanggia != "-")
            {
                string[] parts = khoanggia.Split('-');

                decimal? minPrice = null;
                decimal? maxPrice = null;

                // trước dấu "-" là min
                if (!string.IsNullOrEmpty(parts[0]))
                    minPrice = decimal.Parse(parts[0]);

                // sau dấu "-" là max
                if (!string.IsNullOrEmpty(parts[1]))
                    maxPrice = decimal.Parse(parts[1]);

                // lọc theo min
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice;

                // lọc theo max
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice;
            }

            var results = query.ToList();
            ViewBag.Keyword = keyword;
            return View(results);
        }
        public ActionResult Cart()
        {
            // Lấy giỏ hàng từ Session

            if (Session["giohang"] == null)
            {
                Session["giohang"] = new Dictionary<int, CTGioHang>();
            }
            Dictionary<int, CTGioHang> giohang = (Dictionary<int, CTGioHang>)Session["giohang"];
            var products = db.Products.Where(pro => giohang.Keys.Contains(pro.ProductId));

            return View(products.ToList());
        }
        [HttpPost]
        public ActionResult AddToCart(int ProductId, int Quantity)
        {
            Dictionary<int, CTGioHang> giohang = new Dictionary<int, CTGioHang>();

            if (Session["giohang"] != null)
                giohang = (Dictionary<int, CTGioHang>)Session["giohang"];

            using (var db = new LTWEntities())
            {
                var product = db.Products.Find(ProductId);

                if (product == null)
                    return RedirectToAction("Index");

                if (giohang.ContainsKey(ProductId))
                {
                    giohang[ProductId].Quantity += Quantity;
                }
                else
                {
                    giohang.Add(ProductId, new CTGioHang
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.Price,   // 🔥 GIÁ LẤY TỪ DB
                        Quantity = Quantity,
                        Image = product.MainImage
                    });
                }
            }

            Session["giohang"] = giohang;
            return RedirectToAction("Cart");
        }



        public ActionResult Remove(int? cartItemId)
        {
            if (cartItemId == null) return RedirectToAction("Cart");

            var giohang = (Dictionary<int, CTGioHang>)Session["giohang"];
            giohang?.Remove(cartItemId.Value);

            return RedirectToAction("Cart");
        }
        public ActionResult SidebarCategories()
        {
            var groups = db.CategoryGroups
                           .Where(g => g.IsActive == true)
                           .OrderBy(g => g.SortOrder)
                           .ToList();

            var categories = db.Categories
                               .Where(c => c.IsActive == true)
                               .OrderBy(c => c.SortOrder)
                               .ToList();

            var model = groups.Select(g => new CategoryGroup
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                Items = categories
                        .Where(c => c.GroupId == g.GroupId)
                        .Select(c => new Category
                        {
                            CategoryId = c.CategoryId,
                            CatName = c.CatName,
                            CatSlug = c.CatSlug
                        })
                        .ToList()
            }).ToList();

            return PartialView("_SidebarCategories", model);
        }

        // LỌC THEO DANH MỤC-------------------------------
        public ActionResult Category(string slug, string groupName)
        {
            if (string.IsNullOrEmpty(slug))
                return HttpNotFound();

            // Lấy category
            var category = db.Categories.FirstOrDefault(c => c.CatSlug == slug);
            if (category == null)
                return HttpNotFound();

            // Lấy danh sách sản phẩm theo CategoryId
            var products = db.Products
                             .Where(p => p.CategoryId == category.CategoryId)
                             .ToList();

            ViewBag.After = category.CatName;
            return View(products);
        }

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(string slug)
        {

            var product = db.Products
       .Include(p => p.Category)
       .FirstOrDefault(p => p.Slug == slug && p.IsActive == true);

            if (product == null)
                return HttpNotFound();

            var relatedProducts = db.Products
                .Where(p => p.CategoryId == product.CategoryId
                            && p.ProductId != product.ProductId
                            && p.IsActive == true)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);


        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug");
            return View();
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,CategoryId,SKU,ProductName,Slug,MainImage,Summary,Price,Stock,IsActive,CreatedAt")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
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

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,CategoryId,SKU,ProductName,Slug,MainImage,Summary,Price,Stock,IsActive,CreatedAt")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CatSlug", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
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


        // ================= LOGIN =================
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = db.AppUsers
                .FirstOrDefault(u => u.Email == email
                                  && u.PasswordHash == password
                                  && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // SET SESSION
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Email"] = user.Email;
            Session["Role"] = user.Role;

            if (user.Role == "ADMIN")
            {
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Products");
        }

        public ActionResult KhachHang()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            int id = (int)Session["UserId"];
            var user = db.AppUsers.FirstOrDefault(u => u.UserId == id);

            return View(user);
        }

        // ĐĂNG XUẤT---------------------
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Products");
        }



    }
}
