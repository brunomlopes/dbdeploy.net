:setvar CustomerTableName Customer

PRINT 'Adding nullable Email Column'
GO

ALTER TABLE [dbo].[$(CustomerTableName)]
ADD Email VARCHAR(100) NULL
GO

PRINT 'Setting Email Column default'
GO

UPDATE [dbo].[$(CustomerTableName)]
	SET Email = ''
GO

PRINT 'Setting Email Column to not nullable'
GO

ALTER TABLE [dbo].[$(CustomerTableName)]
ALTER COLUMN Email VARCHAR(100) NOT NULL
GO