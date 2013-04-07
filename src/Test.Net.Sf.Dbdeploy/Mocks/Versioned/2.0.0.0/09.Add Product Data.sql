PRINT 'Adding 2 products'

INSERT INTO [dbo].[Product] 
	([Name], [Description], [Count])
	SELECT N'Mens Running Shoe', 'Breathable leather upper - combined with a midfoot overlay to give you a great fit and comfort.', 520 UNION
	SELECT N'Quality HD Tablet', '7 inch HD Display, Quality Audio, Dual-Band Dual-Antenna Wi-Fi, 16GB or 32GB', 43

PRINT 'Done adding products'
