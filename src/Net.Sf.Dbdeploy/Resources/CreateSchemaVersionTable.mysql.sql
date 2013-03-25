CREATE TABLE $(QualifiedTableName) (
	ChangeId INT NOT NULL AUTO_INCREMENT,
	Folder VARCHAR(256) NOT NULL,
	ScriptNumber SMALLINT NOT NULL,
	ScriptName VARCHAR(512) NOT NULL,
	StartDate DATETIME NOT NULL,
	CompleteDate DATETIME NULL,
	AppliedBy VARCHAR(128) NOT NULL,
	ScriptStatus TINYINT NOT NULL,
	ScriptOutput TEXT NOT NULL
);

ALTER TABLE $(QualifiedTableName) ADD CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId);

ALTER TABLE $(QualifiedTableName) ADD UNIQUE INDEX(Folder, ScriptNumber);