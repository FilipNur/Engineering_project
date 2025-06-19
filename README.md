# Description
Project made for engineering degree, made in Unity3D.

![](/Images/Interface1.png)

Project consists of robot arm with 4 motors. It's path can be programmed by inputing values in the interface as shown below

![](/Images/Interface3.png)

Interfaces can be changed by choosing them from drop down list in top left corner of the screen.

Each position is shows in form of a ball representing point in trajectory. Balls change from color green to red in order of first -> last position.
Trajectory between two points can be defined to be linear or joint. In linear trajectory tool moves in a straight line from one point to the other. In joint trajectory all motors begin moving at the same time and end movement at the same time, as only important thing is that tool starts at point A and ends up at point B.

Additional functions include:
- camera view can be modivied by holding scroll wheel and moving mouse or scrolling up and down
- change max motor speed
- change max linear speed
- change max motor acceleration
- change max linear acceleration
- after generating trajectory, slider can be used to manually change arm position
- rounding angles to servo precision (normally angles are calculated as real values but those values can be rounded to nearest value which is multiple of simulated servo motor)
- after each trajectory calculation ```.csv``` file is outputted in format of : time [s], q0 [deg] , q2 [deg], q3 [deg], q4 [deg], space between claws [mm], where qx is angle of motor x, where all 0 angles make robot point straight up

![](/Images/Interface2.png)

# Matlab
Repository contains additional ```.m``` matlab files that contain functionality used to generate plots out of collected ```.csv``` data.