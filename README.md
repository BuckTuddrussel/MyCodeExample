# Unity C# - MyCodeExample - Recruitment task 2.1

[![License](https://img.shields.io/badge/License-EUPL-blue.svg)](LICENSE)

A while ago, I tackled this simple recruiting task, which seems like it should serve as a good sample of my approach to coding. 
Please note that this task-project was straightforward, and I deliberately avoided any complex tricks.
Think of it as a glimpse into how I work and who I am :)


The project utilizes a combination of design patterns to meet specific needs:
- MVC (Model-View-Controller): Responsible for managing the game's logic, separating it into three distinct components: 
  Model (any data), View (screen) and Controller (managers, features, services).
- Message Bus/Event Bus: Implemented to handle the dynamic and unpredictable evolution of games, 
  this pattern minimizes dependency complexities, enabling decoupled code.

## Table of Contents
- [Installation](#installation)
- [Controls](#controls)
- [Interaction](#interaction)
- [Features](#features)
- [Optimization](#optimization)
- [License](#license)

## Installation

1. Download or clone this repository to your local machine using:
  ```
  git clone https://github.com/BuckTuddrussel/MyCodeExample.git
  ```
  
2. Open the project in Unity.

3. (Optionally) Load the "Main" scene located in Assets/_MyCodeExample.Game/Scenes

4. Run the app by clicking the play button.

## Controls
Arrow keys or Left Mouse click to control UI navigation,
F1-F4 to fade in mockup provided with the recruitment task.

## Interaction
The application is simple, just left and right buttons — content is generated with each restart.

## Features
- Unit tests
- Animated UI
- Button without UnityEngine.UI.Button (or at least two of them 😊)
- Custom unity package 
- The starting point is the Program.cs class, which closely resembles the entry point of a native C# application. (Just for fun 😎)

## Optimization
- Implemented separate texture atlases to reduce draw calls, improve load time, decrease VRAM usage and reduce build size.
- Implemented caching for heavier animations (tweens and sequences).
- Disabling Canvas instead of GameObject to prevent unnecessary RectTransform data discarding and rebuilding of canvas elements.
- Introduced basic code splitting to the assembly, allowing some code to be shared with other apps for better performance and decoupling.
- Disabled pixel-perfect, mode as it's not suitable for non-pixel-art styles; at this resolution, the differences aren't noticeable.
- Disabled domain reload and adapted the application for faster editor load times.
- Removed unnecessary UnityPackages to improve load time, decrease RAM usage, and reduce build size.
- Adjusted the camera to focus solely on UI elements, eliminating the need for complex tricks with "overlayers" to manage UI element visibility.
- Minimized the use of GameObjects and MonoBehaviours wherever possible to reduce their overhead.

### Additional assumptions and notes:
- Although frameworks like Entitas or Zenject could have been utilized, doing so would have deviated from the purpose of this task.
- To maintain originality and avoid pre-built solutions, I opted not to use UniTask and instead developed custom extensions to Tasks, albeit at a higher cost.
- I assumed that DataItem requests were resource-intensive so they are canceld if the user changes the page before receiving the data.
- Due to inconsistencies between the mockup and the task description, I contacted the stakeholders for clarification and adjusted the application to match the mockup's appearance (e.g., using 36pt instead of 48pt font size).
- Vanilla task files with translated description are included in this repository.
- This repository does not utilize LFS to enable downloading without cloning 😊.

## License
This project is licensed under the EUPL License, see the [**LICENSE**](License) file for details.
> MyCodeExample was made with Unity®. Unity is a trademark or registered trademark of Unity Technologies