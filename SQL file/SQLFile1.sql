CREATE DATABASE NVB_TruyenTranh;
USE NVB_TruyenTranh;

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NULL, -- nếu không mã hoá, lưu Password NVARCHAR(255) (nhưng KHÔNG khuyến khích)
    FullName NVARCHAR(200),
    Phone NVARCHAR(30),
    Address NVARCHAR(500),
    Role NVARCHAR(20) NOT NULL DEFAULT 'customer', -- customer | admin
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 2. Categories (thể loại)
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Slug NVARCHAR(120) NULL,
    Description NVARCHAR(1000) NULL
);

-- 3. Authors
CREATE TABLE Authors (
    AuthorId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Bio NVARCHAR(2000) NULL
);

-- 4. Publishers (nhà xuất bản)
CREATE TABLE Publishers (
    PublisherId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Website NVARCHAR(255) NULL
);

-- 5. Products (truyện / tập)
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(300) NOT NULL,
    Subtitle NVARCHAR(300) NULL,
    Description NVARCHAR(MAX) NULL,
    SKU NVARCHAR(100) NULL UNIQUE,
    Price DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    IsDigital BIT NOT NULL DEFAULT 0, -- nếu bán ebook vs bản vật
    PublisherId INT NULL,
    AuthorId INT NULL,
    PublishedDate DATE NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Products_Publisher FOREIGN KEY (PublisherId) REFERENCES Publishers(PublisherId),
    CONSTRAINT FK_Products_Author FOREIGN KEY (AuthorId) REFERENCES Authors(AuthorId)
);

-- 6. ProductImages
CREATE TABLE ProductImages (
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(1000) NOT NULL,
    AltText NVARCHAR(300) NULL,
    IsMain BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductImages_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- 7. ProductCategory (many-to-many)
CREATE TABLE ProductCategory (
    ProductId INT NOT NULL,
    CategoryId INT NOT NULL,
    PRIMARY KEY (ProductId, CategoryId),
    CONSTRAINT FK_PC_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_PC_Category FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

-- 8. Inventory
CREATE TABLE Inventory (
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    Quantity INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Inventory_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- 9. Orders
CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE, -- e.g. MAGA-20251209-0001
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, Shipped, Cancelled, Completed...
    TotalAmount DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    ShippingAddress NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Orders_User FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- 10. OrderItems
CREATE TABLE OrderItems (
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(12,2) NOT NULL, -- giá tại thời điểm đặt
    LineTotal AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT FK_OrderItems_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- 11. Carts (giỏ hàng tạm, có thể lưu theo user hoặc sessionId)
CREATE TABLE Carts (
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,        -- nếu user đã đăng nhập
    SessionId NVARCHAR(200) NULL, -- nếu lưu theo session cookie
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Carts_User FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE CartItems (
    CartItemId INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    AddedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CartItems_Cart FOREIGN KEY (CartId) REFERENCES Carts(CartId),
    CONSTRAINT FK_CartItems_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- 12. Reviews
CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NULL,
    Rating TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(2000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsApproved BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Reviews_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Reviews_User FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- 13. Payments (lưu trữ thông tin thanh toán tham khảo)
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL, -- e.g. paypal, momo, cod, credit_card
    PaymentProviderRef NVARCHAR(255) NULL, -- id trả về từ provider
    Amount DECIMAL(12,2) NOT NULL,
    PaidAt DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Payments_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
