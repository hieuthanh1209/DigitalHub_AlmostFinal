-- Bảng AdminUser
CREATE TABLE [dbo].[AdminUser] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [NameUser] NVARCHAR(255) NULL,
    [RoleUser] NVARCHAR(50) NULL,
    [PasswordUser] NCHAR(50) NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

-- Bảng Category
CREATE TABLE [dbo].[Category] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [IDCate] NCHAR(20) NOT NULL,
    [NameCate] NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED ([IDCate] ASC)
);

-- Bảng Customer
CREATE TABLE [dbo].[Customer] (
    [IDCus] INT IDENTITY(1, 1) NOT NULL,
    [NameCus] NVARCHAR(255) NULL,
    [PhoneCus] NVARCHAR(15) NULL,
    [EmailCus] NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED ([IDCus] ASC)
);

-- Bảng Products với cột DiscountPrice để lưu giá sau khi giảm
CREATE TABLE [dbo].[Products] (
    [ProductID] INT IDENTITY(1, 1) NOT NULL,
    [NamePro] NVARCHAR(255) NULL,
    [DecriptionPro] NVARCHAR(MAX) NULL,
    [Category] NCHAR(20) NULL,
    [Price] DECIMAL(18, 2) NULL,          -- Giá gốc
    [DiscountPrice] DECIMAL(18, 2) NULL,  -- Giá sau khi giảm
    [ImagePro] NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([Category]) REFERENCES [dbo].[Category] ([IDCate])
);

-- Bảng OrderPro
CREATE TABLE [dbo].[OrderPro] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [DateOrder] DATE NULL,
    [IDCus] INT NULL,
    [AddressDeliverry] NVARCHAR(255) NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([IDCus]) REFERENCES [dbo].[Customer] ([IDCus])
);

-- Bảng OrderDetail
CREATE TABLE [dbo].[OrderDetail] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [IDProduct] INT NULL,
    [IDOrder] INT NULL,
    [Quantity] INT NULL,
    [UnitPrice] DECIMAL(18, 2) NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([IDProduct]) REFERENCES [dbo].[Products] ([ProductID]),
    FOREIGN KEY ([IDOrder]) REFERENCES [dbo].[OrderPro] ([ID])
);

-- Bảng ProductViewHistory
CREATE TABLE [dbo].[ProductViewHistory] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerID] INT NOT NULL,
    [ProductID] INT NOT NULL,
    [ViewDate] DATETIME NOT NULL,
    CONSTRAINT [FK_ProductViewHistory_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer]([IDCus]),
    CONSTRAINT [FK_ProductViewHistory_Product] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products]([ProductID])
);
CREATE TABLE [dbo].[Wishlist] (
    [CustomerID] INT NOT NULL,                             -- Mã khách hàng
    [ProductID] INT NOT NULL,                              -- Mã sản phẩm
    [DateAdded] DATETIME DEFAULT GETDATE(),                -- Ngày thêm sản phẩm vào mục yêu thích
    PRIMARY KEY ([CustomerID]),                            -- Khóa chính là CustomerID, chỉ cho phép một mục yêu thích mỗi tài khoản
    CONSTRAINT [FK_Wishlist_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer]([IDCus]),  -- Liên kết với bảng Customer
    CONSTRAINT [FK_Wishlist_Product] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products]([ProductID])  -- Liên kết với bảng Products
);