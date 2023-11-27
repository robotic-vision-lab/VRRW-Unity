# Unity VR Robotic Workspace

This repository contains the Unity project for the VR Robotic Workspace software package, as part of
the paper Intuitive Robot Integration via Virtual Reality Workspaces, published to ICRA 2023.

## Before You Begin

1. Make sure that SteamVR is installed and running on your machine
2. Make sure that you have a VR headset connected to your machine
3. Make sure you complete initial SteamVR Room Setup
4. Make sure that you have a ROS-side properly configured and running

## Quick Start

1. Clone this repository

```console
user@DESKTOP:~$ git clone https://github.com/robotic-vision-lab/Unity-VR-Robotic-Workspace.git
```

2. Open the project in Unity Hub

The project was originally build on Unity 2021.3.3f1, but it has recently been refreshed to
2021.3.32f1, and has been verified to be working.

3. Start the ROS-side backend API

See [Unity Robotics ROS-Side API]() for more information.

4. Open the `VR Robotic Workspace` scene

This should happen by default. However, if it does not, you can open the scene by navigating to
`Assets > Scenes > VR Robotic Workspace` from within he Unity Editor.

5. Start the Unity-side package

Hit the play button in the Unity Editor to start the package. Please note that at this point, you
should have SteamVR running with a headset connected, and the ROS-side API running.

At this point, the robot should snap into position, and you may begin interacting with the robot and
additional objects in the scene.
