# Kinect Project

This is the stripped down version of the RoomAliveToolKit. It takes the full depth image and displays it.

# Prerequisites

* Visual Studio 2015 Community Edition (or better)
* Kinect for Windows v2 SDK

The project uses SharpDX and Math.NET Numerics packages. These will be downloaded and installed automatically via NuGet when RoomAlive Toolkit is built.

The 'Shaders' project requires Visual C++. Note that in Visual Studio 2015, Visual C++ is not installed by default. You may be prompted to install the necessary components when building the 'Shaders' project of the RoomAlive Toolkit.

# How to

	* Step 1: Assuming you already have calibrated, click on ProjectorMappingSample. Right-click and go to properties
	* Step 2: Click on Debug tab
	* Step 3: In the command line arguments box, enter the path of the calibration file.