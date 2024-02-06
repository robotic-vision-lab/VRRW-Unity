## Virtual Reality Robotic Workspace (Unity-Side Package) 

### Overview 

This repository contains the Unity-side package for the virtual reality (VR)
robotic workspace software project. 

### Citation

If you find this project useful, then please consider citing our work.

```bibitex
@inproceedings{tram2023intuitive,
  title={Intuitive Robot Integration via Virtual Reality Workspaces},
  author={Tram, Minh Q. and Cloud, Joseph M. and Beksi, William J},
  booktitle={Proceedings of the IEEE International Conference on Robotics and Automation (ICRA)},
  pages={11654--11660},
  year={2023}
}
```

### Before You Begin

1. Make sure that SteamVR is installed and running on your machine.
2. Make sure that you have a VR headset connected to your machine.
3. Make sure you complete the initial SteamVR Room Setup.
4. Make sure that you have the ROS-side properly configured and running.

### Quick Start

1. Clone this repository:

```console
user@DESKTOP:~$ git clone https://github.com/robotic-vision-lab/VRRW-Unity.git
```

2. Open the project in Unity Hub:

The project was originally build on Unity 2021.3.3f1, but it has recently been
refreshed to 2021.3.32f1 and verified to be working.

3. Start the ROS-side backend API:

See [VRRW-ROS](https://github.com/robotic-vision-lab/VRRW-ROS.git) for more information.

4. Open the `VR Robotic Workspace` scene:

This should happen by default. However, if it does not then you can open the
scene by navigating to `Assets > Scenes > VR Robotic Workspace` from within the
Unity Editor.

5. Start the Unity-side package:

Hit the play button in the Unity Editor to start the package. Please note that
at this point you should have SteamVR running with a VR headset connected and
the ROS-side API running.

The robot should snap into position and you may begin interacting with the
robot and additional objects in the scene.

### License

[![license](https://img.shields.io/badge/license-Apache%202-blue)](https://github.com/robotic-vision-lab/VRRW-Unity/blob/main/LICENSE)
