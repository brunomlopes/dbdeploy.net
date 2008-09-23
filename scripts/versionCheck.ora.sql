CREATE OR REPLACE PROCEDURE versionCheck (deltaSet IN VARCHAR2
  , databaseVersion INTEGER
  , tableName VARCHAR2) IS
currentDatabaseVersion INTEGER;
errMsg VARCHAR2(1000);
sqlString VARCHAR2(1000);
BEGIN
sqlString := 'SELECT NVL(MAX(change_number),0) FROM ' || tableName || ' WHERE delta_set = ''' || deltaSet || '''';
execute immediate sqlString INTO currentDatabaseVersion;
IF currentDatabaseVersion <> databaseVersion THEN
    errMsg := 'Error: current database version on delta_set <' || deltaSet || '> is not ' || databaseVersion || ', but ' || TO_CHAR(currentDatabaseVersion);
    RAISE_APPLICATION_ERROR (-20001, errMsg);
END IF;
END versionCheck;