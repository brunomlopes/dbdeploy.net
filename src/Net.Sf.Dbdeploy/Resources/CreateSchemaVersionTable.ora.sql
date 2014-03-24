CREATE TABLE $(QualifiedTableName) (
	ChangeId VARCHAR2(64) NOT NULL, 
	Folder VARCHAR2(20) NOT NULL, 
	ScriptNumber NUMBER(2) NOT NULL, 
	ScriptName VARCHAR2(256) NOT NULL, 
	StartDate TIMESTAMP NOT NULL, 
	CompleteDate TIMESTAMP NULL, 
	AppliedBy VARCHAR2(64) NOT NULL, 
	ScriptStatus NUMBER(1) NOT NULL,
	ScriptOutput CLOB NULL,
	CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId),
	CONSTRAINT UK_$(TableName) UNIQUE (Folder, ScriptNumber));