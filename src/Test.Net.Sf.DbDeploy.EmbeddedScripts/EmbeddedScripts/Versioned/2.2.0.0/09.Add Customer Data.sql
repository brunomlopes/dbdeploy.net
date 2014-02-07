PRINT 'Adding 2 Customers'

INSERT INTO [dbo].[Customer] 
	([Name])
	SELECT N'John' UNION
	SELECT N'Mary'

PRINT 'Done adding Customers'
