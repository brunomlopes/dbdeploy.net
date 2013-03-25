:setvar CustomerTableName TableThatDoesNotExist

PRINT 'Adding 2 customers'
GO

INSERT INTO [dbo].[$(CustomerTableName)] 
	([FirstName], [LastName], [Phone], [Email])
	SELECT N'Bob', 'Hansen', NULL, 'bob@tempuri.org' UNION
	SELECT N'Tim', 'Malloy', '555-123-4567', 'tim@tempuri.org'
GO

PRINT 'Done adding customers'
GO