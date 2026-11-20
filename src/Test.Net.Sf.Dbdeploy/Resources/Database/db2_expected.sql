
-- START CHANGE SCRIPT v1.0/001_change.sql (1)

INSERT INTO ChangeLog (Folder, ScriptNumber, ScriptName, StartDate, AppliedBy, ScriptStatus, ScriptOutput)
 VALUES ('v1.0', 1, '001_change.sql', CURRENT TIMESTAMP, CURRENT USER, 3, '');

BEGIN TRANSACTION

-- contents of change script 1

UPDATE ChangeLog 
SET CompleteDate = getdate(), ScriptStatus = 1, ScriptOutput = '' 
WHERE Folder = 'v1.0' AND ScriptNumber = 1;

-- END CHANGE SCRIPT v1.0/001_change.sql (1)
COMMIT TRANSACTION

-- START CHANGE SCRIPT v1.0/002_change.sql (2)

INSERT INTO ChangeLog (Folder, ScriptNumber, ScriptName, StartDate, AppliedBy, ScriptStatus, ScriptOutput)
 VALUES ('v1.0', 2, '002_change.sql', CURRENT TIMESTAMP, CURRENT USER, 3, '');

BEGIN TRANSACTION

-- contents of change script 2

UPDATE ChangeLog 
SET CompleteDate = getdate(), ScriptStatus = 1, ScriptOutput = '' 
WHERE Folder = 'v1.0' AND ScriptNumber = 2;

-- END CHANGE SCRIPT v1.0/002_change.sql (2)
COMMIT TRANSACTION
