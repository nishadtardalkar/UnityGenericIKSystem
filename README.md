# UnityGenericIKSystem
An easy to understand and open source implementation of Inverse Kinematics for Unity 3D

[![FAILED TO LOAD IMAGE](https://img.youtube.com/vi/4FMOBhg3xaM/0.jpg)](https://www.youtube.com/watch?v=4FMOBhg3xaM)
![FAILED TO LOAD GIF](GIF/IK.gif)
<br>
<br>
<br>
<b>UPDATE</b> :<br>
The lengths are now calculated automatically, no need to manually type them.
<br><br><br>
<b>UPDATE</b> :<br>
As, the algorithm that the plugin follows make changes in the hierarchy, a reset button is added to revert back to original scene.
<br><br><br>
<b>Steps to use plugin</b><br>
<ol>
  <li>Download the IKSolver script in your Assets</li>
  <li>Make sure your bones are not having child-parent relations</li>
  <li>Seperate them if they do have it</li>
  <li>Create two empty gameobjects for Target and Pole</li>
  <li>Assign IKSolver script to Target gameobject</li>
  <li>Assign Bone Transforms to the script in order of leaf to root</li>
  <li>Set lengths of bones in world space</li>
  <li>Assign Pole Transform to the script as well</li>
  <li>Set iteration count as less as possible such that the system accurately follows the target</li>
  <li>Enable EditMode option if you want to test IK in Unity Scene window</li>
</ol>

<b>License : MIT</b>
<b>Enjoy...</b>
