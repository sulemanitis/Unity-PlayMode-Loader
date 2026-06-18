# Unity Play Mode Loader

A lightweight Unity Editor utility that automatically loads a designated startup scene when entering Play Mode and restores your previously opened scene setup when exiting Play Mode.

Perfect for projects that always need to start from a Loading Scene, Bootstrap Scene, Initialization Scene, or Main Menu scene during testing.

---

## Why?

In larger Unity projects, gameplay scenes often depend on systems initialized by a bootstrap scene:

* Save Systems
* Audio Managers
* Dependency Injection
* Addressables Initialization
* Analytics
* Localization
* Loading Screens

Opening a gameplay scene directly and pressing Play can cause missing references, initialization issues, or inconsistent behavior.

This tool solves that problem by automatically:

1. Saving your currently opened scenes.
2. Opening your designated startup scene.
3. Entering Play Mode.
4. Restoring your original scene setup when Play Mode ends.

---

## Features

* Automatically loads a startup scene before Play Mode.
* Restores previously opened scenes after Play Mode.
* Supports multiple open scenes.
* Optional "Save Before Play".
* Project-specific settings.
* No runtime dependencies.
* No packages required.
* Works entirely inside the Unity Editor.

---

## Installation

### Option 1: Copy Script

Place the script inside:

Assets/Editor/

Example:

Assets/Editor/PlayModeLoader.cs

---

### Option 2: Git Submodule

Add this repository as a Git submodule inside your project's Editor folder.

---

## Setup

Open:

Edit → Project Settings → Play Mode Loader

Configure:

* Enable Loader
* Save Before Play
* Loading Scene Path

Select the scene that should always be loaded before entering Play Mode.

Example:

Assets/Scenes/Loading.unity

---

## Workflow

Suppose you are currently editing:

* Level_05
* LightingScene
* UIScene

When you press Play:

Level_05
LightingScene
UIScene

↓

Loading

↓

Play Mode

When you stop Play Mode:

Loading

↓

Level_05
LightingScene
UIScene

Your original scene setup is restored automatically.

---

## Use Cases

### Bootstrap Scene

Always start from:

Assets/Scenes/Bootstrap.unity

### Loading Scene

Always start from:

Assets/Scenes/Loading.unity

### Main Menu

Always start from:

Assets/Scenes/MainMenu.unity

---

## Requirements

* Unity 2021+
* Editor-only

---

## License

MIT License
