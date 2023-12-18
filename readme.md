# Adhesion Tester
An open source solution for the automated testing of adhesives through the use of force probes. This is a WPF application written in C# over the summer of 2016 by Samuel Lehmann, at the University of Alberta, to accelerate the work of Yue Wang and Dr. Dan Sameoto. This work is available [here](https://www.ncbi.nlm.nih.gov/pubmed/28038311 "Paper"). 

![Adhesive Test System](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/AdhesiveTestSystem.PNG)

#### Support for both linear and radial tests
###### Linear Trials
Linear tests allow the testing of adhesives in simple linear strokes, at any desired incoming or outgoing angle, with drag distances, preloads, preload wait times, velocities and more. 
![Linear Trial](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/mainPage.png)

###### Radial Trials
Radial tests allow the testing of an 'adhesion circle', allowing the accurate profiling of an adhesives directional dependence through a series of outwards strokes of a force probe radiating from a centerpoint. This trial type is more heavily detailed in our paper.
![Radial Trial](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/radial%20trial.gif)

In order to help in the visualization of this data, a partner application, written in MATLAB has been created, which generates graphs of this data. This application is also included in this repository and is distributed under the same license.

![Radial Trial Visualizations](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/radialGraphs.jpg)

#### Set it and forget it 
A series of trials can be setup dynamically, each featuring a large number of options, and each being repeatable as many times as desired, allowing set it and forget it usage, where trials can be run for days at a time with no maintenance, and complete confidence.
![Multiple trial setup](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/linear%20trial.gif)

#### Real-time data collection
![Data collection](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/dynamicGraphs.gif)

Convenient, automatic data collection that is saved as a .CSV file, easily editable  in Microsoft Excel, MATLAB, or other applications. Both normal and shear data is collected and the program can easily be modified to collect further data if needed.

### Further Features
Further features of this program include an automatic calibration feature, and a baseline detection feature that detects when contact has been broken between the force probe and the adhesive surface. These features are both simple to use and are heavily documented.

### Easy setup and well documented highly extensible code
The source code to this program is heavily commented and is designed to be easily modified. Documentation to this project includes a compiled HTML help file detailing functions and objects used throughout the source, along with a word document detailing the communication protocols used in this application, in an easy to follow high level overview.
#### Setup
This github repository includes an installer that will install the necessary files for the application on your system. Once you have done this, all you need to do is adjust the settings to correctly identify the channels for the normal and shear force, along with identifying the com port that the motor controller uses. ![Settings](https://raw.githubusercontent.com/slehmann1/adhesion_tester/master/res/settings.png) **This program was designed to communicate with an [ESP 301 Motor controller], and an [NI USB-6289] data acquisition hub. However**, if you do not wish to use these devices, this program can still be useful to you. All that is necessary is the replacement of either the espManager or DAQ classes with classes that communicate with the device of your choice.

### Licensing
This application is distributed under the [MIT License]. We would however greatly appreciate it if you provide a link to this page or in any future versions of this software you choose to release or any scientific articles that you publish. The reason for this is that an acknowledgement, while not necessary would allow a greater number of scientists to discover this resource.

### Development information
This is a WPF application, available for Windows. Unfortunately, this project does not support Linux or Mac. This application uses [OxyPlot] to aid in graphing of data, and  [Mahapps.Metro] to aid in the UI design. This application also uses [NI-DAQmx] to aid in communication with the National Instruments USB hub, whereas communication with the ESP-301 motor controller is implemented by myself.


[ESP 301 Motor controller]: https://www.newport.com/f/esp301-3-axis-dc-and-stepper-motion-controller
[NI USB-6289]: http://sine.ni.com/nips/cds/view/p/lang/en/nid/209154
[OxyPlot]: http://www.oxyplot.org/
[Mahapps.Metro]: http://mahapps.com/
[NI-DAQmx]: http://www.ni.com/download/ni-daqmx-15.1.1/5665/en/
[MIT License]: https://opensource.org/licenses/MIT
