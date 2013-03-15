PRINT 'Adding nullable Sold Column'

ALTER TABLE [dbo].[Product]
ADD Sold INT NULL

PRINT 'Setting Sold Column default'

UPDATE [dbo].[Product]
	SET Sold = '0'

PRINT 'Setting Sold Column to not nullable'

ALTER TABLE [dbo].[Product]
ALTER COLUMN Sold INT NOT NULL
