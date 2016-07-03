@ECHO OFF
SET build=%1
IF "%~1"=="" SET build=0
SET "branch=master"
FOR /F "tokens=1" %%b IN ('git rev-parse --abbrev-ref HEAD') DO SET branch=%%b
SET "version="
FOR /F "tokens=1" %%t IN ('git tag') DO SET version=%%t
SET commit=0
IF "%version%"=="" (
    SET "version=0.1"
    FOR /F "tokens=1" %%r IN ('git rev-list %branch%') DO SET /A commit+=1
) ELSE (
    FOR /F "tokens=1" %%r IN ('git log %version%..HEAD --oneline') DO SET /A commit+=1
)
ECHO [assembly: System.Reflection.AssemblyVersion("%version%.%commit%.%build%")] > "%CD%\.build\VersionAssemblyInfo.cs""