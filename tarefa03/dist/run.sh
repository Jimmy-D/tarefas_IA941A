#! /bin/bash

./ws3d.sh &
sleep 10
java -jar DemoJSOAR.jar &
sleep 2
cp ../ClarionApp/bin/Release/ClarionApp.exe .
cp ../ClarionApp/bin/Release/ClarionLibrary.dll .
./ClarionApp.exe 