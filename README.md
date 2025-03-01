# 🚀 SpaceExplore VR 🚀
### **A VR Space Exploration Experience**

## **Introduction**
**SpaceExplore** is a VR project designed to provide users with an **immersive space exploration experience** focused on relaxation, discovery, and seamless planet landings. Inspired by open-world games like *Forza Horizon* and *No Man’s Sky*, this project enables users to **freely control a spaceship, traverse planets, and experience atmospheric landings**.

Built using **Unity 3D**, the project utilizes the **Meta XR Core SDK** and **Meta XR Interaction SDK** to ensure **smooth VR interactions, joystick-based spaceship controls, and realistic environment transitions**.

> **Note:** For this repository, the assets are not included due to their large file sizes and licensing restrictions. Some assets are not free to distribute. Please refer to the links below to obtain them:
> - **Spaceship:** [Star Sparrow Modular Spaceship (Free)](https://assetstore.unity.com/packages/3d/vehicles/space/star-sparrow-modular-spaceship-73167)
> - **Universe Background:** [Deep Space Skybox Pack (Free)](https://assetstore.unity.com/packages/2d/textures-materials/deep-space-skybox-pack-11056)
> - **Multi-Planets Texture:** [Procedural Planets and Stars (Not Free)](https://assetstore.unity.com/packages/vfx/shaders/substances/procedural-planets-and-stars-106662)
> - **Meta XR Core SDK:** [Download](https://developers.meta.com/horizon/downloads/package/meta-xr-core-sdk/)
> - **Meta Interaction SDK:** [Download](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-sdk-overview/)

---

## **🎮 Features**
- **VR Spaceship Control:** Navigate the spaceship using Oculus Quest controllers in first-person or third-person view.
- **Seamless Planet Landing:** Experience smooth transitions between space and planetary surfaces with atmospheric effects.
- **Dynamic Atmosphere Rendering:** Implemented a fading shader effect for realistic entry into a planet’s atmosphere.
- **Optimized Scene Transitions:** Designed custom landing algorithms to enhance immersion while maintaining performance.
- **Multiple View Modes:** Switch between first-person cockpit mode and third-person spaceship view dynamically.
- **Immersive Audio:** Implemented Meta XR Audio SDK for 3D spatial sound, enhancing the player's sense of presence.

---

## **🛠️ Project Setup**

### **1️⃣ Clone the Repository**
```bash
git clone https://github.com/YOUR_GITHUB_USERNAME/SpaceExplore.git
cd SpaceExplore
```
### **2️⃣ Install Required Packages**
Make sure you have Unity 2021.3+ (LTS recommended) installed.

#### **Import Meta XR SDKs**
You will need the following Meta XR packages for VR interactions and tracking:
- **Meta XR Core SDK:** Provides tracking, camera rig, and movement capabilities. 
  [Download Meta XR Core SDK](https://developers.meta.com/horizon/downloads/package/meta-xr-core-sdk/)
- **Meta XR Interaction SDK:** Handles controller input, UI interactions, and object grabbing. 
  [Download Meta Interaction SDK](https://developers.meta.com/horizon/documentation/unity/unity-isdk-interaction-sdk-overview/)

### **3️⃣ Import Project Assets**
Due to size and licensing restrictions, the following asset files are not included in this repository. Please obtain them from the Unity Asset Store:
- **Spaceship:** [Star Sparrow Modular Spaceship (Free)](https://assetstore.unity.com/packages/3d/vehicles/space/star-sparrow-modular-spaceship-73167)
- **Universe Background:** [Deep Space Skybox Pack (Free)](https://assetstore.unity.com/packages/2d/textures-materials/deep-space-skybox-pack-11056)
- **Multi-Planets Texture:** [Procedural Planets and Stars (Not Free)](https://assetstore.unity.com/packages/vfx/shaders/substances/procedural-planets-and-stars-106662)

After obtaining these assets, import them into Unity via the Package Manager or the Asset Store.

## **🌌 Implementation Details**

### **VR Tracking & Spaceship Control**
The project uses Meta XR Core SDK to set up the OVR Camera Rig and Tracking Space for accurate head and hand tracking. Spaceship navigation is implemented using `OVRInput` and `Rigidbody` physics.

### **Seamless Planet Landing System**
The landing system evolved through several methods:
1. **Large Sphere Inside the Planet:** Initially used but found impractical for VR scaling.
2. **Scene Transition at Altitude Threshold:** Effective but caused abrupt changes.
3. **Hybrid Approach:** Combines atmospheric fading via a shader with a gradual scene transition.

### **Atmosphere Shader (Fading Effect)**
The shader’s opacity is controlled by adjusting its properties based on the spaceship's altitude.

## **🎮 How to Play**
- **Launch the Game:** Start the project in Unity and launch the game in VR (compatible with Meta Quest 2/Pro).
- **Control the Spaceship:** Use the joystick to navigate the spaceship through space.
- **Planetary Landing:** As you approach a planet, watch the atmosphere gradually fade. Press "B" for auto-landing, or manually descend to land.
- **Explore:** Once landed, explore the planet's surface or take off again to resume space travel.

## **📌 Next Steps**
Future Features:
- Expanding to more planets with unique landscapes.
- Adding space stations for mid-space exploration.
- Implementing real-time voice communication in VR.
- Enhancing terrain details for improved realism.

## **🤝 Contributions**
If you'd like to contribute or test the project, please open a GitHub issue or submit a pull request.

## **📜 License**
This project is for educational and personal development purposes. All assets used are subject to their respective licenses from the Unity Asset Store and Meta XR SDK. Redistribution of the assets is not permitted.

**GitHub Repo:** [GitHub Link](https://github.com/YOUR_GITHUB_USERNAME/SpaceExplore)
