PRINT 'Adding nullable Gender Column'

ALTER TABLE [dbo].[Customer] 
ADD Gender CHAR NULL
GO