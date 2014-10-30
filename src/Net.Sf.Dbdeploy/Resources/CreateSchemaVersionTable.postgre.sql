CREATE TABLE $(QualifiedTableName) (
	ChangeId VARCHAR(64) NOT NULL,
	Folder VARCHAR(20) NOT NULL,
	ScriptNumber SMALLINT NOT NULL,
	ScriptName VARCHAR(256) NOT NULL,
	StartDate TIMESTAMP NOT NULL,
	CompleteDate TIMESTAMP,
	AppliedBy VARCHAR(64) NOT NULL,
	ScriptStatus SMALLINT NOT NULL,
	ScriptOutput TEXT,
	CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId),
	CONSTRAINT UK_$(TableName) UNIQUE (Folder, ScriptNumber));