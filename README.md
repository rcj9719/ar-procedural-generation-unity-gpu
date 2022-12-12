# <div align="center"> ARC or ARCreation : AR app for procedural generation </div>

**University of Pennsylvania, CIS 565: GPU Programming and Architecture, Final Project**

* [Guanlin Huang](https://virulentkid.github.io/personal_web/index.html), [Shutong Wu](https://www.linkedin.com/in/shutong-wu-214043172/), [Rhuta Joshi](https://www.linkedin.com/in/rcj9719/)
 * Tested on: Samsung Tablet S8, Qualcomm SM8450 Snapdragon 8 Gen 1;CPU: Octa-core (1x3.00 GHz Cortex-X2 & 3x2.50 GHz Cortex-A710 & 4x1.80 GHz Cortex-A510), GPU: 	Adreno 730

### [Link to project demo here](https://youtu.be/ZUIvCwbsThM)

Overview
===========
Project ARC is proposed to implement an application for adding procedural elements to the real-world view. One hidden problem of procedural generation is its performance with a large number of generations, and we intend to solve this with GPU. We have implemented grass and customized the L-system in Unity and show it in AR with Unity AR Foundation.   

|Basic design of application|
|---|
|![](imgs/demo.gif)|


### Installation
The App has not been released to the public on a mobile app store, but the code can be downloaded and built in Unity. Download and install the .APK file to your android device. AR experiences on Android devices are driven by ARCore, available on [ARCore supported devices](https://developers.google.com/ar/devices). Ensure that your development device is compatible with AR. Alternatively, you can use a correctly configured AR-compatible Android Emulator instance.

### Usage Instruction

The App allows you to select a type of grass or procedurally generated trees you would like to place in the AR view. It works best in a larger area with some feature points where ARCore plane detection works well.
1. Click on the `Show Panel` switch to view a panel with items to place.
2. Scan the room till you see the light white dots on the plane indicating surface detection.
3. Once the surface is detected, you can tap anywhere on the plane to see the selected procedural item in AR view.
4. You can also turn on the `Presets` where we provide options to view a preset arrangement of trees, grass, and plants arranged as a garden.
5. An option to view some additional `Debug Info` is also provided
6. At any time, you can use the clear screen option to reset and start over.

### Developer Setup

- Developed using Unity 2021.3.13f1. We recommend using Unity Hub.
- Clone this repo to your machine (optionally, fork this repository if you plan on expanding on it).
- Open the project in Unity 2021.3.13f1, and open the 'ARCScene' scene (if it doesn't open automatically).
- Deploy to an [ARCore compatible device](https://developers.google.com/ar/discover/supported-devices).
- Version control system used during development - Plastic SCM

---  
### Implementation contents

- [Grass](#grass)
- [GPU-based L-system](#gpu-based-l-system)
  - [L-System Derivation & Interpretation](#l-system-derivation-and-interpretation)
  - [L-System Rendering](#l-system-rendering)
  - [2D Noise Procedural Generation](#2d-noise-procedural-generation)
- [Integrating it with AR](#integrating-it-with-ar)

### Presentations and other documentation
- [Project board, bug tracker, and resources](https://cis-565-final-project.notion.site/Team-ARC-f8764bf740f6408ebb3c7bdedbad31f6)   
- [Project Pitch](https://docs.google.com/presentation/d/19CZpP7CKG2L5rKTixmAmIi9_ogACH7zY3G-8qamP4r0/edit?usp=sharing)
- [Milestone 1 Presentation](https://docs.google.com/presentation/d/16BDfPikoMo0FX5iWDb8mwlZ5nQAJBaDj30A9Vw1sQ5o/edit#slide=id.p)   
- [Milestone 2 Presentation](https://docs.google.com/presentation/d/1iHksZZ4u2Z6-Yoie_S-0kXjdsAEHsFWQpKVJzvuKXFI/edit#slide=id.p)   
- [Milestone 3 Presentation](https://docs.google.com/presentation/d/1cWZAUilfjqYLi6OaeJzwCYyNQ0r6AfFJXVTBsvolfYg/edit#slide=id.g19cafe9bbc4_0_3)   
- [Final Presentation](https://docs.google.com/presentation/d/1nhRvU-0dief0bP7L1LuMD3uFziOVkGfzeUahdgH2EHA/edit?usp=sharing)   

Implementation
===========
## Grass
The grass is rendered through Unity Universal Render Pipeline compute shaders, which can take in a single mesh shape as input and generate other mesh as outputs. In this project, we decided to put a plane mesh as input, and for each vertex of that plane, we output a single triangle to represent a grass blade. To render those grass blades with decent color, we also need a fragment shader and interpolate between a base grass color and a tip grass color. The output should look like the following:
  
<img src="imgs/grassNonRand.png" width="600" height="400"/>
  
To make the grass more realistic, we want to change the height of the grass blades so that they do not look too triangle-like. Next, we need randomly twist the grass blades so that they will not be facing the same direction; we also need to bend the grass to mimic gravity. To accomplish the aforementioned transformational goals, we define the blade in space close to the vertex emitting it, after which it will be transformed to be local to the mesh. We implemented tangent space to cope with this need.

In order to have grass curvature and convincing grass movement, each blade of grass is divided into a number of segments. Compared to tessellation, this method saves more memory and is more efficient for constructing and compute. 


|Tangent Space ([Image source](https://en.wikipedia.org/wiki/Tangent_space#/media/File:Image_Tangent-plane.svg)) |Grass construction ([Image source](https://roystan.net/articles/grass-shader/)) |
|---|---|
|<img src="imgs/tangentSpace.png" width="250" height="200"/>|<img src="imgs/grass-construction.gif" width="200" height="200"/>|
  
The wind is implemented by sampling from a noise texture. The UV coordinate is constructed using the grass blades' input points; this will ensure that they will behave the same with multiple grass instances. The wind is then applied using a scaled rotation matrix to each segment of the grass blade. Combining with some variance in blade shapes, the final result looks like this:  

The grass also interacts with the device camera. This means that when a user moves the device camera closer or further away from the grass, the grass bends interactively. Similar to how the wind is applied, the interaction bending is applied with the scaled rotation matrix with respect to the distance between the device and the grass.   

|Grass under noise based wind forces|Grass interaction with device camera|
|---|---|
|<img src="imgs/grassIntro.gif" width="600" height="450"/>|<img src="imgs/grass2.gif" width="270" height="450"/>|

---

## GPU-based L-system
We implement our L-system generation process based on the paper [Parallel Generation of L-Systems](https://publik.tuwien.ac.at/files/PubDat_181216.pdf). Unlike the paper, which uses CUDA to implement the L-System, we use Unity with its compute shader for the generation. The implementation can be broken down into three parts:   
* L-System Derivation: Turn the axiom string into a derived string based on selected rulesets
* L-System Interpretation: Transform the string to a list of position/orientation/material arrays that we use to draw for each symbol
* Rendering: render all the items in the array to the scene

### L-System Derivation and Interpretation

|Parallel L-system Generation Workflow used|
|---|
|![](imgs/gpu-lsystem-generation.png)|

#### Stage 1: Preparation and loading

Before the derivation starts, there will be a preparation step where every customized rulesets will be loaded into the script. The compute shader will know what each character will derive into.

Consider the following L-System Example:

```
AXIOM: FAA
Rules:
  F -> F[+F][-F]
  A -> FF
```

The above rules get passed to the GPU in 2 buffers - the predecessor buffer and the successor buffer, which are obtained by serially appending all grammar rules.

```
Predecessor Buffer: FA  // Concatenating the left side of all rules
Successor Buffer: F[+F][-F]FF   // Concatenating the right side of all rules
```

#### Stage 2: Parallel Derivation - Expanding the string

In the derivation process, each thread will take care of each character in the string, and a prefix sum scan function is used to calculate the total length of the newly derived string. Because Compute Shader does not accept characters, we are converting characters to ASCII code to ensure the compute shader's dispatch goes smoothly.   
In each iteration of derivation(L-System might go through many iterations), we will first use the scan function to examine the total string length of the next iteration's derived string; then we use the prefix sum array to identify the derived characters' indices in the new string. A new string will be generated after this step.   
Our scan function can deal with 512*512 = 262144 elements for each L-System, and it is sufficient for this project and the most complex L-system generation.   

| Derivation example from Base Paper | Understanding our example |
|---|---|
|![Image with How Derive Work](imgs/scan.PNG)|![](imgs/lsystemderivation1.png)|


#### Stage 3: Parallel Interpretation - Positioning & branching based on expanded string

This is the paper's approach to the interpretation, but we decided to use a 1D linked list to help finish the interpretation more simply.   
After we get all the strings from derivation, we will then set data for:<br>
* SymbolBuffer: A compute buffer that identifies if each character is a symbol(Something to draw) or not
* DepthBuffer: A compute buffer that identifies each character's depth
* PosBuffer/OriBuffer: A kernel that identifies the position/orientation vector that the character might change to the next character<br>

| Interpretation example from Base Paper | Our Linked List based implementation |
|---|---|
|![Image With how Interpret Work](imgs/interpret.PNG)|![](imgs/lsysteminterpretation1.png)|

Then after a prefix sum scan of the Depth buffer and Pos/Ori Buffer, we will set data for LinkedBuffer, where each index will store its parent's index. This solves the hardest problem when constructing an L-system: having every symbol have its predecessor's data.   
Finally, we iterate through the array, fetching all the symbols that need to be a draw and their local coordinates. The interpretation marks are complete up to this point.

### L-System Rendering

|L-System Example 1|L-System Example 2|L-System Example 3|
|---|---|---|
|<img src="imgs/2.png" width="250" height="300"/>|<img src="imgs/6.png" width="250" height="300"/>|<img src="imgs/11.png" width="250" height="300"/>|

We have implemented rendering in two ways, one in CPU and one in GPU, to render L-System based on different needs.  
For CPU, we instantiate the gameobject we have for each symbol, and their performance is relatively acceptive under not complex scenes. The point of keeping this is for debugging and comparing its performance against GPU rendering in simple to not very complicated (below 100 L-system) scenes.  
For GPU, we initially passed the mesh/vertex/triangles information to the vertex shader and ran a shader file to render this at runtime. Yet, in our research, we find Unity has a DrawProcedural and DrawMesh function, where we can let GPU render mesh at giving world coordinates at runtime, so we also implement this rendering procedure.


### 2D Noise Procedural Generation and preset garden views
We also implemented a procedural noise generation where you can generate these L-system trees with a certain probability. We can scale each tree's likelihood of appearance, its distances from others, and the boundary of the generation.  

|Arrangement based on noise function|
|---|
|![Image of Noise Generation](imgs/noise.PNG)|

We have also provided an option for the users to view a pre-arranged set of procedural L-system trees and grass to look like a garden.

|Garden preset in Unity|Garden preset in AR|
|---|---|
|<img src="imgs/gardenPresetUnity.png" width="600" height="400"/>|<img src="imgs/gardenPreset.png" width="600" height="400"/>||


## Integrating it with AR

Setting up an AR environment included many steps elaborately explained in [Google documentation here.](https://developers.google.com/ar/develop/unity-arf/getting-started-ar-foundation)
Considering the future scope of the project, we have also set up ARCore extensions as [documented here.](https://developers.google.com/ar/develop/unity-arf/getting-started-extensions)

ARCore is Google's framework for building augmented reality experiences on smartphones. We are using Unity's AR Foundation to build this application. We need to take care of two things - detecting surfaces/planes and placing objects in the real-world scene based on user interaction, like tapping or dragging on the screen. This is mainly accomplished using the following:

1. ARPlaneManager - An ARPlaneManager detects ARPlanes and creates, updates, and removes game objects when the device's understanding of the environment changes.
2. ARRaycastManager - An ARRaycastManager exposes raycasting functionality in which we shoot a ray from the screen coordinates of our tap into the real-world detected, and we store the intersection points as hit points. Refer to the image shown below taken from Google Cdoelabs.
3. ARAnchorManager - ARAnchorManager is used to track elements (gameobjects) in the real world. In our application, we are tracking each L-system that the user places so that we can interact with them better and because we want to anchor and orient them correctly along the plane detected. The number of anchors can be reduced since they are resource-intensive. 

|ARRayCast Hit from touch point on screen|
|---|
|![Image Credits - Google Codelabs](imgs/hit-test-explanation.png)|


## Performance Analysis
 * Tested on: Samsung Tablet S8, Qualcomm SM8450 Snapdragon 8 Gen 1;CPU: Octa-core (1x3.00 GHz Cortex-X2 & 3x2.50 GHz Cortex-A710 & 4x1.80 GHz Cortex-A510), GPU: 	Adreno 730  

 For the performance analysis, we will first analyze each of the components we have implemented. Then we will examine the overall performance to show that using GPU for L-system generation is efficient.

### Grass Performance
The following graph shows how the number of grass blades will impact the framerate on the mobile device; the maximum framerate on our tester device is 31 FPS. With 800 blades covering approximately 1 square meter (M<sup>2</sup>) of the real-world area, our App can cover up to 46M<sup>2</sup> of space, or 37600 blades of grass, without sacrificing framerate. 
<img src="imgs/fpsGrass.png" width="600" height="350"/>
  
The pressure on GPU caused by increasing the number of grass blades shows why we would have a 37600 grass blade limit. After reaching 32000 grass blades, or approximately covering 40M<sup>2</sup> of area, the GPU usage reaches a throttle at 95%. From 37600 blades onwards, the device will sacrifice framerate to render the grass blades properly.
<img src="imgs/gpuGrass.png" width="600" height="350"/>  

### L-System Performance CPU vs. GPU
We have compared two resources using different methods to generate L-System. One uses CPU to generate L-System entirely; the other is our current work, which uses GPU to analyze the grammar and only uses CPU to instantiate the gameobject in the last step.  
For the sample to be compared with, we choose one of the [L-System Bush written by Paul Bourke](http://paulbourke.net/fractals/lsys/) as a test case to pay tribute to his work on L-System. We will measure the time that take for them to generate different L-Systems.  

|Paul Bourke Bush|
|---|
|<img src="imgs/bush.PNG" width="270" height="450"/>|


The performance analysis below shows the different times each method uses to generate the tree. At early iterations with only 100 or fewer symbols to draw, CPU/GPU takes a similar time to generate, and the CPU is even faster in milliseconds. But at later iterations, where thousands of symbols will be drawn to the screen, the time taken for the CPU version is exponentially increasing and become slower than the GPU version.  

<img src="imgs/PA.png" width="600" height="350"/>   

The reason for the time difference is that GPU's parallelism may not be fully utilized until the grammar is complex enough. But one problem with GPU and Unity's compute shader is that if the grammar is too complicated and the whole derived string exceeds the maximum length of string, then the GPU version would not work correctly. It will be a future upgrade if possible, but for now the current version could handle 262144 elements which is sufficient enough for most L-system trees.

### Overall Performance

Finally, we placed multiple trees of a single grammar at a different level of generations in AR while also having interactive grass blades in the scene to determine the overall performance. We can observe that as the generations increase, there is an expected drop in framerate because of the increased number of characters in each generation, especially when the number of items placed is high. Another possible reason for this drop, despite the parallel generation of these L-systems, is that this testing is performed where instances are rendered sequentially on the CPU.

![](imgs/overallPA.png)

## Credits
* [Ned Makes Games: Blade Grass](https://www.youtube.com/watch?v=6SFTcDNqwaA&ab_channel=NedMakesGames)
* [Parallel Generation of L-Systems](https://publik.tuwien.ac.at/files/PubDat_181216.pdf)
* [Roystan: Grass Shader Tutorial](https://roystan.net/articles/grass-shader/)
* [LSystem Ruleset](http://paulbourke.net/fractals/lsys/)

### Unity Free Asset Packs used include:
- Stylized Hand Painted Nature Pack by BigRookGames
- Hand Painted Flowers by Infinity3DGame


## Bloopers

|May the force be with you|
|---|
|![](imgs/interactBlooper.gif)|

|Grammatical mistakes|Copy paste|Reaching for the skies|
|---|---|---|
|<img src="imgs/blooper1.png" width="270" height="450"/>|<img src="imgs/blooper2.png" width="270" height="450"/>|<img src="imgs/blooper3.png" width="270" height="450"/>|
