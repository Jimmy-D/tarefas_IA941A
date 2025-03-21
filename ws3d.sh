#! /bin/bash

xhost +
docker run --rm -it --name coppelia-sim \
 -e DISPLAY \
 --net=host \
 --privileged \
 -p 4011:4011 \
 -v /tmp/.X11-unix:/tmp/.X11-unix \
 brgsil/ws3d-coppelia "$@"
