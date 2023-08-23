# MASS RCC | Unity3D, C#
 
 A ship simulation, including vessels like ship, a drone and an AUV(Autonomous Underwater Vehicle)
 You have the ability to set a waypoint by indicating the longitude and latitude, where you can make use of the autopilot of manually navigate from your current position to the desired point.
 
 You can view a demo here: https://youtu.be/rJusdYxs_WE

![image](https://github.com/qurafa/MASS-RCC/assets/57468292/f7ac790a-c4ca-417a-a002-88a31c3df06b)

 
 *Controls*
 - You are able to switch vessels from the BOAT to the AUV to the Drone by holding the SPACE-BAR key.
 - Clicking the 1 and 2 keys on your keyboard switches from the player view to the ECDIS view
 - You can move the vessels(Boat and AUV) using the W,S,A,D keys. you are also able to control the depth of the AUV using the Q and E keys.
 - To move the Drone, you make use of the arrow keys and to move forwards, backwards or sideways and use the W,S,A,D,Q,E keys to rotate and finally the Z or X keys to increase or decrease altitude.
 - The camera rotates as you move your mouse.
 - Holding the R-key fixes the camera to the forward direction of the vessel.
 - Holding the U-key removes some UI features, so you only see the player view. Doing the same again return the UI features
 - Clicking the T-key on your keyboard after entering the destination turns on the autopilot feature for the Boat.
 - The T-key also turns on and off the autopilot for the AUV.

*Navigation*
 - You can see the ship's longitude and latitude on the top right corner of the screen.
 - You can enter a longitude and latitude you wish to navigate to in the bottom left corner of the screen
 - When you enter your destination a 'black line' appears showing you the direction to your destination
 - When you are not switched to the AUV, it automatically faces and moves towards the Boat while moving back upwards towards the ocean surface.
 - The unless you turn of the autopilot for the AUV, it should automatically hover around the Boat, or start moving towards the entered destination, if any.
 
*ECDIS*
- When you are in the ECDIS view, you can tap the G-key to release the lock and use the W,S,A,D keys to move the around and see the different parts of the map.
- The larger green arrow pointer represents the BOAT and the smaller green arrow pointer represents the AUV
- On the ECDIS, green dots, with a speed and distance represents other vessels present on the water.

*Other*
- You should be able to hear different sounds, including the sound of other boats, the sound of the ocean, above and below sealevel depending on the location of the camera.
- The proximity alert turns on when the boat is close to an object.
- When the proximity alert turns on and the boat is on autopilot, then the boat stops(could be changed later)

Made using the [Crest Ocean System](https://github.com/wave-harmonic/crest) to simulate the visuals of the water and it's physiscs

Feel free to contact me if you have any more questions :)
