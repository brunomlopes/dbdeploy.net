CREATE TABLE ChangeLog (
        ChangeId INT(11) NOT NULL AUTO_INCREMENT,
        Folder VARCHAR(255) NOT NULL,
        ScriptNumber SMALLINT(6) NOT NULL,
        ScriptName VARCHAR(512) NOT NULL,
        StartDate DATETIME NOT NULL,
        CompleteDate DATETIME DEFAULT NULL,
        AppliedBy VARCHAR(128) NOT NULL,
        ScriptStatus TINYINT(4) NOT NULL,
        ScriptOutput TEXT NOT NULL,
        PRIMARY KEY (ChangeId),
        UNIQUE INDEX IX_ChangeLog (Folder, ScriptNumber)
);
