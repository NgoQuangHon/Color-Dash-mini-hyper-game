Color Dash
A 3D hyper-casual runner game built with Unity and C#. Control a
rolling ball, avoid obstacles, collect coins, and try to beat your
highest score.
Project status: In development

Gameplay
- Control a ball moving along a 3D track.
- Avoid obstacles and survive for as long as possible.
- Collect coins placed along the track.
- Track the current score and save a best score.
- Use the game UI to start, pause, resume, restart, and view the Game
  Over state.
Built With
- Game Engine: Unity 6
- Language: C#
- Rendering: 3D
- Version Control: Git and GitHub
Project Structure
ColorDash/
├── Assets/           # Scripts, scenes, prefabs, materials, audio and other game assets
├── Packages/         # Unity package manifest and package configuration
├── ProjectSettings/  # Unity project settings
└── README.md
Unity-generated folders such as Library/, Temp/, Obj/, and Logs/
are excluded from version control through .gitignore.
Getting Started
1. Install Unity Hub and a compatible Unity 6 editor version.
2. Clone or download this repository.
3. In Unity Hub, choose Add and select the cloned ColorDash
   folder.
4. Open the project with the compatible Unity editor.
5. Open the game's main scene from the Assets folder and press
   Play.
Unity may take some time to import assets and rebuild its local
Library folder on the first launch.
Development Goals
Planned improvements may include:
- Refining obstacle and coin spawning patterns.
- Improving player feedback, collision handling, and game-state
  transitions.
- Adding sound effects and background audio.
- Optimizing repeated object creation with object pooling.
- Polishing the UI and preparing a Windows build.
Notes
- Keep Assets/, Packages/, and ProjectSettings/ in version
  control so the project can be opened by other developers.
- Do not commit generated Unity folders such as Library/, Temp/,
  Obj/, or Logs/.
- If the project uses large binary assets, consider Git LFS where
  appropriate.
Author
Ngo Quang Hon

GitHub: @NgoQuangHon
