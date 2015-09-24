SET release=0
SET version=0
FOR /F "tokens=1" %%a IN ('git rev-list master') DO SET /A release+=1
FOR /F "tokens=*" %%a IN ('git tag') DO SET /A version+=1
echo [assembly: System.Reflection.AssemblyVersion("0.5.%version%.%release%")] > VersionAssemblyInfo.cs