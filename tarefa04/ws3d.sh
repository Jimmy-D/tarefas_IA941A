#! /bin/bash
xhost +
echo "Running WS3D by docker (you may need docker installed in your machine to use it!)"
docker run --rm -it --name coppelia-sim \
 -e DISPLAY \
 --net=host \
 --privileged \
 brgsil/ws3d-coppelia



