
-- START CHANGE SCRIPT v1.0/001_change.sql (1)

INSERT INTO ChangeLog (Folder, ScriptNumber, ScriptName, StartDate, AppliedBy, ScriptStatus, ScriptOutput)
 VALUES ('v1.0', 1,'001_change.sql', getdate(), user_name(), 3, '')
GO

BEGIN TRANSACTION
GO

-- contents of change script 1

UPDATE ChangeLog 
SET CompleteDate = getdate(), ScriptStatus = 1, ScriptOutput = '' 
WHERE Folder = 'v1.0' AND ScriptNumber = 1
GO

IF NOT EXISTS (SELECT 1 FROM ChangeLog WHERE Folder = 'v1.0' AND ScriptNumber = 1 AND ScriptStatus = 1)
BEGIN
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
    RAISERROR('Change script %s failed. The change scripts after it were not run.', 16, 1, 'v1.0/1')
    SET NOEXEC ON
END
GO

COMMIT
GO

-- END CHANGE SCRIPT v1.0/001_change.sql (1)


-- START CHANGE SCRIPT v1.0/002_change.sql (2)

INSERT INTO ChangeLog (Folder, ScriptNumber, ScriptName, StartDate, AppliedBy, ScriptStatus, ScriptOutput)
 VALUES ('v1.0', 2,'002_change.sql', getdate(), user_name(), 3, '')
GO

BEGIN TRANSACTION
GO

-- contents of change script 2

UPDATE ChangeLog 
SET CompleteDate = getdate(), ScriptStatus = 1, ScriptOutput = '' 
WHERE Folder = 'v1.0' AND ScriptNumber = 2
GO

IF NOT EXISTS (SELECT 1 FROM ChangeLog WHERE Folder = 'v1.0' AND ScriptNumber = 2 AND ScriptStatus = 1)
BEGIN
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
    RAISERROR('Change script %s failed. The change scripts after it were not run.', 16, 1, 'v1.0/2')
    SET NOEXEC ON
END
GO

COMMIT
GO

-- END CHANGE SCRIPT v1.0/002_change.sql (2)

