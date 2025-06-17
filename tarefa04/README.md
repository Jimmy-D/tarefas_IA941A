# DemoLIDA
This project provides a demo on how to use the [LIDA](https://github.com/CognitiveComputingResearchGroup/lida-framework) cognitive architecture for building an intelligent agent at the [WorldServer3D virtual environment](https://github.com/CST-Group/ws3d). It is a demonstration used at 
[IA941 - Laboratory of Cognitive Architectures](https://faculty.dca.fee.unicamp.br/gudwin/courses/IA941), a graduate-level course at the University of Campinas - Brazil. 

Requirements:
- JDK
- Docker

In order to use it, you first need to download the code and run the ws3d application:

```bash
user$ ./ws3d.sh
```

You might need to first have Docker installed on your machine, as the ws3d.sh bash script uses Docker to call the ws3d application
Then, in a different terminal, you can do:

```bash
user$ ./gradlew run
```

and you will see the robot running in ws3d. 
