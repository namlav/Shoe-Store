/* =========================================================
   SHOP GIÀY ONLINE – FULL DATABASE
   ========================================================= */

IF DB_ID('LTW') IS NOT NULL
BEGIN
    DROP DATABASE LTW;
END;
GO

CREATE DATABASE LTW COLLATE SQL_Latin1_General_CP1_CI_AS;
GO
USE LTW;
GO

/* ================= USER ================= */
CREATE TABLE AppUser(
    UserId INT IDENTITY PRIMARY KEY,
    Email VARCHAR(120) NOT NULL UNIQUE,
    PasswordHash VARCHAR(256) NOT NULL,
    FullName NVARCHAR(120),
    Phone VARCHAR(20),
    Role VARCHAR(20) NOT NULL, -- ADMIN / USER
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME()
);

/* ================= CATEGORY ================= */
CREATE TABLE CategoryGroup(
    GroupId INT IDENTITY PRIMARY KEY,
    GroupCode VARCHAR(40) UNIQUE,
    GroupName NVARCHAR(120),
    SortOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME()
);

CREATE TABLE Category(
    CategoryId INT IDENTITY PRIMARY KEY,
    GroupId INT NOT NULL,
    CatSlug VARCHAR(60) UNIQUE,
    CatName NVARCHAR(120),
    Description NVARCHAR(300),
    SortOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Category_Group FOREIGN KEY(GroupId)
        REFERENCES CategoryGroup(GroupId)
);

/* ================= PRODUCT ================= */
CREATE TABLE Product(
    ProductId INT IDENTITY PRIMARY KEY,
    CategoryId INT NOT NULL,
    SKU VARCHAR(40) UNIQUE,
    ProductName NVARCHAR(180),
    Slug VARCHAR(90) UNIQUE,
    MainImage NVARCHAR(250),
    Summary NVARCHAR(300),
    Price DECIMAL(12,0),
    Stock INT DEFAULT 100,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Product_Category FOREIGN KEY(CategoryId)
        REFERENCES Category(CategoryId)
);

/* ================= CART ================= */
CREATE TABLE Cart(
    CartId INT IDENTITY PRIMARY KEY,
    CartToken VARCHAR(64) UNIQUE,
    UserId INT NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Cart_User FOREIGN KEY(UserId)
        REFERENCES AppUser(UserId)
);

CREATE TABLE CartItem(
    CartItemId INT IDENTITY PRIMARY KEY,
    CartId INT,
    ProductId INT,
    Quantity INT CHECK (Quantity > 0),
    UnitPrice DECIMAL(12,0),
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CartItem_Cart FOREIGN KEY(CartId) REFERENCES Cart(CartId),
    CONSTRAINT FK_CartItem_Product FOREIGN KEY(ProductId) REFERENCES Product(ProductId),
    CONSTRAINT UQ_Cart_Product UNIQUE (CartId, ProductId)
);

/* ================= ORDER ================= */
CREATE TABLE Orders(
    OrderId INT IDENTITY PRIMARY KEY,
    UserId INT NULL,
    CustomerName NVARCHAR(120),
    Phone VARCHAR(20),
    AddressLine NVARCHAR(220),
    Status VARCHAR(20) DEFAULT 'PENDING',
    TotalAmount DECIMAL(12,0) DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Order_User FOREIGN KEY(UserId)
        REFERENCES AppUser(UserId)
);

CREATE TABLE OrderItem(
    OrderItemId INT IDENTITY PRIMARY KEY,
    OrderId INT,
    ProductId INT,
    ProductName NVARCHAR(180),
    Quantity INT CHECK (Quantity > 0),
    UnitPrice DECIMAL(12,0),
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY(OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItem_Product FOREIGN KEY(ProductId) REFERENCES Product(ProductId)
);

/* =================================================
   INSERT DATA
   ================================================= */

/* USERS */
INSERT INTO AppUser (Email, PasswordHash, FullName, Phone, Role)
VALUES
('admin@shoe.vn','admin123','Admin','0900000001','ADMIN'),
('user@shoe.vn','user123',N'Khách Hàng','0900000002','USER');

/* CATEGORY GROUP */
INSERT INTO CategoryGroup (GroupCode, GroupName, SortOrder)
VALUES
('MEN',N'Giày Nam',1),
('WOMEN',N'Giày Nữ',2);

/* CATEGORY */
INSERT INTO Category (GroupId, CatSlug, CatName)
VALUES
(1,'sneaker-nam',N'Sneaker Nam'),
(1,'tay-nam',N'Giày Tây Nam'),
(2,'sneaker-nu',N'Sneaker Nữ'),
(2,'cao-got',N'Giày Cao Gót');

/* PRODUCT – 12 SẢN PHẨM */
INSERT INTO Product
(CategoryId, SKU, ProductName, Slug, MainImage, Summary, Price)
VALUES
-- Sneaker Nam
(1,'SN-N-01',N'Vans Style 36 “Marshmallow” Dress Blue','vans-style-36-sn-n-01','card-item1.jpg',N'Sneaker nam',2750000),
(1,'SN-N-02',N'Nike Air Force 1 White Low','nike-af1-white-low-sn-n-02','card-item2.jpg',N'Sneaker nam',2950000),
(1,'SN-N-03',N'New Balance 574 Grey Blue','nb-574-grey-blue-sn-n-03','card-item3.jpg',N'Sneaker nam',1650000),
(1,'SN-N-04',N'New Balance Pro Court Navy CRT300','nb-pro-court-crt300-sn-n-04','card-item4.jpg',N'Sneaker nam',2450000),
(1,'SN-N-05',N'Balenciaga Triple S Trainer White','balenciaga-triple-s-sn-n-05','card-item5.jpg',N'Sneaker nam',2850000),
(1,'SN-N-06',N'Alexander McQueen Oversized Sneaker Black','mcqueen-oversized-black-sn-n-06','card-item6.jpg',N'Sneaker nam',1550000),
(1,'SN-N-07',N'Air Max 97 Ultra 17 Triple White','air-max-97-ultra-sn-n-07','card-item7.jpg',N'Sneaker nam',2700000),
(1,'SN-N-08',N'Air Force 1 ’07 LV8 Overbranding','af1-lv8-overbranding-sn-n-08','card-item8.jpg',N'Sneaker nam',2350000),
(1,'SN-N-09',N'Adidas Superstar Running White','adidas-superstar-white-sn-n-09','card-item9.jpg',N'Sneaker nam',3000000),
(1,'SN-N-10',N'Adidas Stan Smith Triple White','stan-smith-triple-white-sn-n-10','card-item10.jpg',N'Sneaker nam',2600000),
(1,'SN-N-11',N'Adidas Stan Smith Fairway','stan-smith-fairway-sn-n-11','card-item11.jpg',N'Sneaker nam',2900000),
(1,'SN-N-12',N'Adidas Prophere Grey Solar Red','adidas-prophere-grey-red-sn-n-12','card-item12.jpg',N'Sneaker nam',1750000),
(1,'SN-N-13',N'Adidas NMD R1 Grey','adidas-nmd-r1-grey-sn-n-13','card-item13.jpg',N'Sneaker nam',1600000),
(1,'SN-N-14',N'Adidas Alphabounce Beyond Grey Red','alphabounce-beyond-grey-red-sn-n-14','card-item14.jpg',N'Sneaker nam',3000000),
(1,'SN-N-15',N'Adidas Alphabounce Beyond Dark Grey','alphabounce-dark-grey-sn-n-15','card-item15.jpg',N'Sneaker nam',1850000),


-- Giày Tây Nam
(2,'GT-N-01',N'TONKIN CAPTOE OXFORD','tonkin-captoe-oxford-gt-n-01','card-lux1.jpg',N'Giày công sở',2950000),
(2,'GT-N-02',N'CHARLES CAPTOE OXFORD','charles-captoe-oxford-gt-n-02','card-lux2.jpg',N'Giày lịch lãm',2750000),
(2,'GT-N-03',N'SAVILLE CAPTOE OXFORD','saville-captoe-oxford-gt-n-03','card-lux3.jpg',N'Giày lịch lãm',2600000),
(2,'GT-N-04',N'SIR CLASSIC OXFORD','sir-classic-oxford-gt-n-04','card-lux4.jpg',N'Giày lịch lãm',2450000),
(2,'GT-N-05',N'BESUAL OXFORD','besual-oxford-gt-n-05','card-lux5.jpg',N'Giày lịch lãm',2300000),
(2,'GT-N-06',N'ÉMIN BROGUES OXFORD','emin-brogues-oxford-gt-n-06','card-lux6.jpg',N'Giày lịch lãm',2150000),
(2,'GT-N-07',N'PABLO CAPTOE OXFORD','pablo-captoe-oxford-gt-n-07','card-lux7.jpg',N'Giày lịch lãm',2800000),
(2,'GT-N-08',N'CLASSIC OXFORD','classic-oxford-gt-n-08','card-lux8.jpg',N'Giày lịch lãm',2000000),
(2,'GT-N-09',N'THE DON CAPTOE OXFORD','the-don-captoe-oxford-gt-n-09','card-lux9.jpg',N'Giày lịch lãm',2900000),
(2,'GT-N-10',N'THE NEWGEN OXFORD','the-newgen-oxford-gt-n-10','card-lux10.jpg',N'Giày lịch lãm',2550000),
(2,'GT-N-11',N'GIBSON CAPTOE OXFORD','gibson-captoe-oxford-gt-n-11','card-lux11.jpg',N'Giày lịch lãm',2700000),
(2,'GT-N-12',N'GIBSON CLASSIC OXFORD','gibson-classic-oxford-gt-n-12','card-lux12.jpg',N'Giày lịch lãm',2400000),


-- Sneaker Nữ
(3,'SN-NU-01',N'Vans Style 36 “Marshmallow” Dress Blue','vans-style-36-sn-nu-01','card-image1.jpg',N'Sneaker nữ',1850000),
(3,'SN-NU-02',N'Nike Air Force 1 White Low','nike-af1-white-low-sn-nu-02','card-image2.jpg',N'Sneaker nữ',3200000),
(3,'SN-NU-03',N'New Balance Pro Court Navy CRT300','nb-pro-court-crt300-sn-nu-03','card-image3.jpg',N'Sneaker nữ',2100000),
(3,'SN-NU-04',N'New Balance 574 Grey Blue','nb-574-grey-blue-sn-nu-04','card-image4.jpg',N'Sneaker nữ',2400000),
(3,'SN-NU-05',N'New Balance 574 Classic Grey','nb-574-classic-grey-sn-nu-05','card-image5.jpg',N'Sneaker nữ',1950000),
(3,'SN-NU-06',N'Balenciaga Triple S Trainer White','balenciaga-triple-s-sn-nu-06','card-image6.jpg',N'Sneaker nữ',3950000),
(3,'SN-NU-07',N'Alexander McQueen Oversized Sneaker Black','mcqueen-oversized-black-sn-nu-07','card-image7.jpg',N'Sneaker nữ',3600000),
(3,'SN-NU-08',N'Air Max 97 Ultra 17 Triple White','air-max-97-ultra-sn-nu-08','card-image8.jpg',N'Sneaker nữ',2900000),
(3,'SN-NU-09',N'Air Force 1 Shadow Pale Ivory','af1-shadow-ivory-sn-nu-09','card-image9.jpg',N'Sneaker nữ',2700000),
(3,'SN-NU-10',N'Air Force 1 ’07 LV8 Overbranding','af1-lv8-overbranding-sn-nu-10','card-image10.jpg',N'Sneaker nữ',2550000),
(3,'SN-NU-11',N'Adidas Superstar Running White','adidas-superstar-white-sn-nu-11','card-image11.jpg',N'Sneaker nữ',2200000),
(3,'SN-NU-12',N'Adidas Stan Smith Triple White','stan-smith-triple-white-sn-nu-12','card-image12.jpg',N'Sneaker nữ',2300000),
(3,'SN-NU-13',N'Adidas Stan Smith Fairway','stan-smith-fairway-sn-nu-13','card-image13.jpg',N'Sneaker nữ',2150000),
(3,'SN-NU-14',N'Adidas Prophere Grey Solar Red','adidas-prophere-grey-red-sn-nu-14','card-image14.jpg',N'Sneaker nữ',2600000),
(3,'SN-NU-15',N'Adidas NMD R1 Grey','adidas-nmd-r1-grey-sn-nu-15','card-image15.jpg',N'Sneaker nữ',2450000),

-- Cao Gót
(4,'CG-01',N'Giày Cao Gót Gót Trụ Phối Khóa','giay-cao-got-got-tru-cg-01','card-cg-1.jpg',N'Cao gót',650000),
(4,'CG-02',N'Giày Cao Gót Cao Gót Phối Dây Đá Trang Trí','giay-cao-got-phoi-day-da-cg-02','card-cg-2.jpg',N'Cao gót',700000),
(4,'CG-03',N'Giày Cao Gót Phối Dây Đá Nhỏ','giay-cao-got-day-da-nho-cg-03','card-cg-3.jpg',N'Cao gót',620000),
(4,'CG-04',N'Giày Cao Gót Phối Liệu Sequin','giay-cao-got-sequin-cg-04','card-cg-4.jpg',N'Cao gót',580000),
(4,'CG-05',N'Giày Cao Gót Pump Gót Thanh Phối Quai Trang Trí','giay-cao-got-pump-got-thanh-cg-05','card-cg-5.jpg',N'Cao gót',690000),
(4,'CG-06',N'Giày Cao Gót Slingback Mũi Vuông','giay-cao-got-slingback-mui-vuong-cg-06','card-cg-6.jpg',N'Cao gót',540000),
(4,'CG-07',N'Giày Cao Gót Slingback Gót Nhọn','giay-cao-got-slingback-got-nhon-cg-07','card-cg-7.jpg',N'Cao gót',600000),
(4,'CG-08',N'Giày Cao Gót Slingback Phối Nơ Trang Trí','giay-cao-got-slingback-phoi-no-cg-08','card-cg-8.jpg',N'Cao gót',560000),
(4,'CG-09',N'Giày Cao Gót Slingback Phối Dây Trang Trí','giay-cao-got-slingback-phoi-day-cg-09','card-cg-9.jpg',N'Cao gót',630000),
(4,'CG-10',N'Giày Cao Gót Pump Gót Trụ Quai Mary Jane Xéo','giay-cao-got-mary-jane-got-tru-cg-10','card-cg-10.jpg',N'Cao gót',680000);

