CREATE TABLE $(QualifiedTableName) (
	ChangeId bigserial,
	Folder character varying(256) NOT NULL,
	ScriptNumber smallint NOT NULL,
	ScriptName character varying(512) NOT NULL,
	StartDate timestamp NOT NULL,
	CompleteDate timestamp NULL,
	AppliedBy character varying(128) NOT NULL,
	ScriptStatus smallint NOT NULL,
	ScriptOutput text NOT NULL
);

ALTER TABLE $(QualifiedTableName) ADD CONSTRAINT PK_$(TableName) PRIMARY KEY (ChangeId);

CREATE INDEX IX_$(TableName) ON $(QualifiedTableName) (Folder, ScriptNumber)