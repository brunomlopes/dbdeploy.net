@ECHO OFF

IF "%1" == "" (
	Tools\nant\bin\nant.exe -buildfile:dbdeploy.build
) ELSE IF "%1" == "help" (
	Tools\nant\bin\nant.exe -buildfile:dbdeploy.build -projecthelp
) ELSE (
	Tools\nant\bin\nant.exe -buildfile:dbdeploy.build %*
)

