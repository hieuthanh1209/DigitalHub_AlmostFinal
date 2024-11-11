CREATE TABLE [dbo].[ProductViewHistory] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerID] INT NOT NULL,
    [ProductID] INT NOT NULL,
    [ViewDate] DATETIME NOT NULL,
    CONSTRAINT [FK_ProductViewHistory_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer]([IDCus]),
    CONSTRAINT [FK_ProductViewHistory_Product] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products]([ProductID])
);
