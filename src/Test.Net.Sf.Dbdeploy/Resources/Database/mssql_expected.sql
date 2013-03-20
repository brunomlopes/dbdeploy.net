
-- START CHANGE SCRIPT Scripts/001_change.sql

BEGIN TRANSACTION
GO

-- contents of change script 1

INSERT INTO ChangeLog (Folder, ScriptNumber, FileName, StartDate, CompleteDate, AppliedBy, Status, Output)
 VALUES ('Scripts', 1,'001_change.sql', getdate(), getdate(), user_name(), 1, '')
GO

COMMIT
GO

-- END CHANGE SCRIPT Scripts/001_change.sql


-- START CHANGE SCRIPT Scripts/002_change.sql

BEGIN TRANSACTION
GO

-- contents of change script 2

INSERT INTO ChangeLog (Folder, ScriptNumber, FileName, StartDate, CompleteDate, AppliedBy, Status, Output)
 VALUES ('Scripts', 2,'002_change.sql', getdate(), getdate(), user_name(), 1, '')
GO

COMMIT
GO

-- END CHANGE SCRIPT Scripts/002_change.sql

