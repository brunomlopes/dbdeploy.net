-- Script generated at 2007-05-05T13:59:45



-- Change script: #2: 002_second.sql

--//@UNDO

DELETE FROM Test WHERE id = 6

DELETE FROM changelog WHERE change_number = 2 AND delta_set = 'Main'
GO
--------------- Fragment ends: #2: 002_second.sql ---------------


-- Change script: #1: 001_first.sql


--//@UNDO

DROP TABLE Test

DELETE FROM changelog WHERE change_number = 1 AND delta_set = 'Main'
GO
--------------- Fragment ends: #1: 001_first.sql ---------------

