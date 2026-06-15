#!/bin/bash
# Keep this logic in sync with publish.bat so it works for both Windows and Linux developers.

version=$1

if [[ "$version" == "" ]]; then
 echo "you must pass a version as the first argument"
 exit 1
fi

cd ../E33Randomizer
dotnet publish E33Randomizer.csproj --runtime=win-x64 /p:Version=$version
if [ $? -ne 0 ]; then
    echo "Failed to publish"
    cd ../
    exit $?
fi

dotnet publish E33Randomizer.csproj --runtime=linux-x64  /p:Version=$version
if [ $? -ne 0 ]; then
    echo "Failed to publish"
    cd ../
    exit $?
fi
cd ../

WINFILE="e33.randomizer.x86_x64.windows.$version.zip"
LINUXFILE="e33.randomizer.x86_x64.linux.$version.tar.gz"

if [[ -f "$WINFILE" ]]; then
    rm $WINFILE
fi

if [[ -f "$LINUXFILE" ]]; then
    rm $LINUXFILE
fi

lastDir=$(pwd)

cd E33Randomizer/bin/Release/net10.0/win-x64/publish/
echo "Creating $lastDir/$WINFILE"
zip -r $lastDir/$WINFILE *
cd $lastDir

cd E33Randomizer/bin/Release/net10.0/linux-x64/publish
echo "Creating $lastDir/$LINUXFILE"
tar -czf $lastDir/$LINUXFILE *
cd $lastDir