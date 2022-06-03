# OpenZenUnity

Unity Demo for OpenZen usage.

![picture](screenshot.png)

## Usage

Connect a LP-Research IMU via USB or Bluetooth and open the Unity project. Once
you loaded the project, click on Assets -> Scenes in the Project explorer and double-click
the DiscoverSensorsScene item. You can start the project, this may take a couple of seconds because
OpenZen searches for all connected sensors. After this, you can select one sensor and should see the virtual
sensor rotate, if you rotate the real-world sensor. You may need to move the Unity
camera to align the rotation directions between the virtual and real-world sensor.

The scene ConnectByNameScene demonstrates how you can connect to a sensor directly without
running the sensor discovery first.

## OpenZen Library

This repository contains a pre-compiled OpenZen DLL for Windows 64-bit. You can find the
source of OpenZen [here](https://bitbucket.org/lpresearch/openzen/).

To update OpenZen Unity plugin, you need to update the C# bindings inside `Assets/Plugins/OpenZen`:

1. Generate [OpenZen C# interface](https://bitbucket.org/lpresearch/openzen/src/master/bindings/) using SWIG.

2. Build OpenZen library with `ZEN_CSHARP` turned on.

3. Copy the generated items from step 1, `OpenZen.dll` and `SiUSBXp.dll` from step 2 into `Assets/Plugins/OpenZen` folder.

## External Licences

This repository contains the SiLabs DLL USB Xpress driver. Please see
Silabs_Licence_Agreement.txt for licence details.
