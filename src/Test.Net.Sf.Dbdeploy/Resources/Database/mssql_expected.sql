
-- START CHANGE SCRIPT #1: 001_change.sql

BEGIN TRANSACTION
GO

-- contents of change script 1

INSERT INTO ChangeLog (ScriptNumber, CompleteDate, AppliedBy, FileName)
 VALUES (1, getdate(), user_name(), '001_change.sql')
GO

COMMIT
GO

-- END CHANGE SCRIPT #1: 001_change.sql


-- START CHANGE SCRIPT #2: 002_change.sql

BEGIN TRANSACTION
GO

-- contents of change script 2

INSERT INTO ChangeLog (ScriptNumber, CompleteDate, AppliedBy, FileName)
 VALUES (2, getdate(), user_name(), '002_change.sql')
GO

COMMIT
GO

-- END CHANGE SCRIPT #2: 002_change.sql

