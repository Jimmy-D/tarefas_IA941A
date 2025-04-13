# WS3DApp

This is a very first program to use with the new WS3D environment that can be run with:

```bash
#! /bin/bash
xhost +

XAUTH=`xauth info | grep file | awk '{print $3}'`

docker run --rm -it --name coppelia-sim \
    -e DISPLAY \
    --net=host \
    --device /dev/snd \
    --privileged \
    -v $XAUTH:/root/.Xauthority \
    -p 4011:4011 \
    brgsil/ws3d-coppelia
```

Save this code in a file with the name ws3d.sh and call

```bash
chmod ugo+x ws3d.sh
```
to make it executable.
Run
```bash
./ws3d.sh
```
to run it. 
Then, in a second shell, run
```bash
./gradlew build
```
to build the WS3DApp and
```bash
./gradlew run
```
to run it. 
