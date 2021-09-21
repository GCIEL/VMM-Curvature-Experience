# VMM-Curvature-Experience

An alpha-level prototype virtual reality experience developed to illustrate the special curvature properties of quadric surfaces. Developed during a Mentored Advanced Project entitled Virtual Mathematical Models at Grinnell College during the summer of 2019 with Professor Christopher French.

<p align="center">
  <img src="/resources/curvature01.PNG" alt="The introductory scene of the experience tells that the player has crash-landed on an unknown planet and explains how to navigate the text based tutorial." width="400">

  <img src="/resources/curvature02.PNG" alt="A player uses the curvature compass to take a visual reading of the curvature of the point in front of them. The peanut shape of the compass illustrates the planet is locally curving more left-to-right than it is curving front-to-back." width="400">
</p>

The key concept illustrated in this experience will be that of curvature, which describes how a surface behaves locally around a given point. At each point on a surface, we can ask how the surface curves as we look in a certain direction. We call points that are locally spherical *umbilical points* and it turns out that different quadric surfaces have unique configurations of such points.

In the first experience the  player has crash-landed on an unknown planet. In order to escape they are tasked with finding all of the umbilical points on planet. This unknown planet is a three-axial ellipsoid, and it turns out that there are exactly four umbilical points the player will have to find. To find these points the player will walk about on the ellipsoid and attempt to gain an intuition of curvature using a compass and minimap. The compass shows the player how the surface that they are walking on curves at a given point by visually displaying the measure of curvature in different directions. The player uses the compass by pointing their right controller at a point on the ground in front of them and pressing down the right trigger. Then, the compass displays a peanut shaped array of orange tangent directions. Each of these orange tangent directions is scaled to demonstrate the measure of curvature in that direction, and two cyan tangents display the most important measures of curvature called the principle curvatures. The minimap allows the player to keep track of their discovered umbilical points as well as their path on the ellipsoid. Additionally, the minimap is intended to give the player another way of visualizing the curvature of the ellipsoid. After finding all of the umbilical points of the ellipsoid, the player can escape from the planet and has finished the experience.

**You can watch a recording of a walkthrough of the experience [here](https://www.youtube.com/watch?v=LUnQkEoExlo).**

<p align="center">
  <img src="/resources/curvature04.PNG" alt="The player looks at their minimap which shows them their path traveled on the surface of the planet as well as a marked umbilical point which they have already discovered." width="400">

  <img src="/resources/curvature06.PNG" alt="The player has found all four umbilical points and has escaped the planet. From high up the player can see the entire planet as well as two of their discovered umbilical points." width="400">
</p>

The main Unity scene file is [Curvature/Assets/Scenes/Ellipsoid](https://github.com/GCIEL/VMM-Curvature-Experience/blob/master/Curvature%20Experience/Assets/Scenes/Ellipsoid.unity) and the important code that I developed is in the [Curvature Experience/Assets/Scripts](https://github.com/GCIEL/VMM-Curvature-Experience/tree/master/Curvature%20Experience/Assets/Scripts) folder. Here are the [poster](https://github.com/GCIEL/VMM-Curvature-Experience/blob/master/resources/vmm_final_poster.pdf) and [paper](https://github.com/GCIEL/VMM-Curvature-Experience/blob/master/resources/vmm_final_paper.pdf) that I wrote for the VMM MAP summer project.

## Built With

* [Unity](https://unity3d.com/) - The game engine and development environment used.
* [SteamVR SDK](https://assetstore.unity.com/packages/templates/systems/steamvr-plugin-32647) - Unity plugin used to target HTC Vive and Oculus Rift VR headsets (using version 1.2.3).
* [Blender](https://www.blender.org/) - Used to create 3D models and textures.

## Authors and Contributors
*  **Tal Rastopchin** - Initial work on virtual reality experience - [GitHub](https://github.com/trastopchin)

## License

This project is licensed under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License (CC BY-NC-SA 4.0)](https://creativecommons.org/licenses/by-nc-sa/4.0/).

## Acknowledgments

* This project was funded by Grinnell College.
* The project was supervised by **Christopher French**.
* My MAP coworkers **Charun Upara** and **William Song**.
