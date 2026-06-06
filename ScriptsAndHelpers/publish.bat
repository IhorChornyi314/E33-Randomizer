echo off

rem Keep this logic in sync with publish.sh so it works for both Windows and Linux developers.

set "version=%1"

if "%version%"=="" (
 echo "you must pass a version as the first argument"
 exit 1
)

cd ..\E33Randomizer
dotnet publish E33Randomizer.csproj --runtime=win-x64
dotnet publish E33Randomizer.csproj --runtime=linux-x64
cd ..\

set "WINFILE=e33.randomizer.x86_x64.windows.%version%.zip"
set "LINUXFILE=e33.randomizer.x86_x64.linux.%version%.tar.gz"

IF EXIST "$WINFILE" (
    rm $WINFILE
)

IF EXIST "$LINUXFILE" (
    rm $LINUXFILE
)

set "lastDir=%cd%"

cd E33Randomizer\bin\Release\net10.0\win-x64\publish
echo "Creating $lastDir/%WINFILE%"
zip -r %lastDir%/%WINFILE% *
cd %lastDir%

cd E33Randomizer\bin\Release\net10.0\linux-x64\publish
echo "Creating $lastDir/%LINUXFILE%"
tar -czf %lastDir%/%LINUXFILE% *
cd %lastDir%