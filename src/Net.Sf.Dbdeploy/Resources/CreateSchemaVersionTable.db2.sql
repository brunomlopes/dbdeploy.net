IF (NOT EXISTS (SELECT 1 FROM SYSCAT.SCHEMATA WHERE SCHEMANAME='$(SchemaName)')) THEN 
	CREATE SCHEMA $(SchemaName) AUTHORIZATION DB2ADMIN
END IF;

CREATE TABLE $(QualifiedTableName) (
	ChangeId INT GENERATED ALWAYS AS IDENTITY (START WITH 1, INCREMENT BY 1, CACHE 20) NOT NULL,
	Folder VARGRAPHIC(256) NOT NULL,
	ScriptNumber SMALLINT NOT NULL,
	ScriptName VARGRAPHIC(512) NOT NULL,
	StartDate TIMESTAMP NOT NULL,
	CompleteDate TIMESTAMP NULL,
	AppliedBy VARGRAPHIC(128) NOT NULL,
	ScriptStatus SMALLINT NOT NULL,
	ScriptOutput DBCLOB(10485760) LOGGED NOT COMPACT NOT NULL,
	CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId)
);

CREATE UNIQUE INDEX IX_$(TableName) ON $(QualifiedTableName)
(
	Folder ASC,
	ScriptNumber ASC
);