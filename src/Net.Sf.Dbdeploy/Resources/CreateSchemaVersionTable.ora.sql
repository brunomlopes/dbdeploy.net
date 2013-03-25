CREATE TABLE $(QualifiedTableName) (
	ChangeId INT NOT NULL AUTO_INCREMENT,
	Folder VARCHAR2(256) NOT NULL,
	ScriptNumber SMALLINT NOT NULL,
	ScriptName VARCHAR2(512) NOT NULL,
	StartDate TIMESTAMP NOT NULL,
	CompleteDate TIMESTAMP NULL,
	AppliedBy VARCHAR2(128) NOT NULL,
	ScriptStatus TINYINT NOT NULL,
	ScriptOutput CLOB NOT NULL
);

ALTER TABLE $(QualifiedTableName) ADD CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId);

CREATE INDEX IX_$(TableName) ON $(QualifiedTableName) (Folder, ScriptNumber)