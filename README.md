# LBA2 Unity Animator
This is a new program to animate LBA2 models in Unity. It uses generated models from my own Blender Python script. Check the following link for more information about custom LBA2 models: https://github.com/LBALab/LBA2_Blender_To_Model_2.

The original idea was to be able to animate the models within Blender and export them using a Python script. However, I was unable to find a good way to export Blender's rotations to the LBA2 animation format. This may be done at a later time. But, it's not a priority at the moment. Thankfully, Unity provides nice functions for converting rotations to euler angles which is exactly what LBA2 uses. Needless to say, everything concerning the rotation conversions works flawlessly. Animating the models feels very intuitive.

An example on how my Unity program can be used.
[![Watch the video](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Images/Video_1.png)](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Videos/LBA2_Animator_Demo.mp4)

Additions / Fixes:
- Selecting bones more easily. Triangles are now highlighted on the model.
- Colors are now by polygon instead of by vertex. They now look like they would in the game.

Todo:
- Add animation "loop" frame.
- Correct animation "slerping".
- Add translation to bones that need to use translation besides rotation.

Animations will need to be tested within Yaz0r's LBA1 / LBA2 model viewer since my own implementation is still not perfect. Yaz0r's viewer can be downloaded from the Magic Ball network. Here is the link for that: https://www.magicball.net/downloads/programs/development/lba_model_viewer

How one of my models looks in Yaz0r's program.
[![Watch the video](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Images/Video_2.png)](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Videos/Custom_Hand_LBA2.mp4)

How the same model looks right in LBA2 replacing Twinsen.
[![Watch the video](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Images/Video_3.png)](https://github.com/MrQuetch/LBA2_Unity_Animator/blob/main/LBA2_Animator_Only/Videos/Custom_Hand_LBA2_InGame.mp4)

Like I've mentioned before, I owe a lot of credit to Xesf. Without his documentation on the LBA2 formats, none of this would have been possible. If you're interested in hacking, modifying, or contributing to the Little Big Adventure games, be sure to checkout the "LBA Lab". It's the first Github organization I've been a part of and it's a nice area for all LBA tools. Here's the link: https://github.com/orgs/LBALab/repositories.
