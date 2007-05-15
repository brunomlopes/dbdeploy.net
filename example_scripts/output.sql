-- Script generated at 2007-05-05T13:59:45



--------------- Fragment begins: #1: 001_first.sql ---------------
INSERT INTO changelog (change_number, delta_set, start_dt, applied_by, description) VALUES (1, 'Main', getdate(), user_name(), '001_first.sql')
GO

-- Change script: #1: 001_first.sql
CREATE TABLE Test (id int)



UPDATE changelog SET complete_dt = getdate() WHERE change_number = 1 AND delta_set = 'Main'
GO
--------------- Fragment ends: #1: 001_first.sql ---------------


--------------- Fragment begins: #2: 002_second.sql ---------------
INSERT INTO changelog (change_number, delta_set, start_dt, applied_by, description) VALUES (2, 'Main', getdate(), user_name(), '002_second.sql')
GO

-- Change script: #2: 002_second.sql
INSERT INTO Test VALUES (6)


UPDATE changelog SET complete_dt = getdate() WHERE change_number = 2 AND delta_set = 'Main'
GO
--------------- Fragment ends: #2: 002_second.sql ---------------

