CREATE TABLE $(QualifiedTableName) (
	ChangeId VARCHAR(64) NOT NULL,
	Folder VARCHAR(256) NOT NULL,
	ScriptNumber SMALLINT NOT NULL,
	ScriptName VARCHAR(512) NOT NULL,
	StartDate DATETIME NOT NULL,
	CompleteDate DATETIME NULL,
	AppliedBy VARCHAR(128) NOT NULL,
	ScriptStatus TINYINT NOT NULL,
	ScriptOutput TEXT NULL,
	PRIMARY KEY PK_$(TableName) (ChangeId),
	UNIQUE KEY (Folder, ScriptNumber)
);