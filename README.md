# UnityIKSystem
An easy to understand and open source implementation of Inverse Kinematics for Unity 3D

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/XFs8cucJ764/0.jpg)](https://www.youtube.com/watch?v=XFs8cucJ764)

<br>
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

<b>Enjoy...</b>