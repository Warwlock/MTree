# MTree - Open Source

**MTree** is a powerful tree creation tool for the Unity Engine. It allows you to create procedural trees directly within the Unity Editor, complete with wind animation, LODs, and mesh merging.

Originally a paid asset, the original developer ([MaximeHerpin](https://github.com/MaximeHerpin)) has allowed this tool to be released as Open Source under the MIT License.

Features
------------

*   **Procedural Generation:** Create unique trees using inspector-based workflows.
*   **Mesh Export:** Export your generated trees to standard Mesh format to use in your projects without needing MTree scripts at runtime (except for wind).
*   **Wind System:** Includes `MTreeWind.cs` for vertex-displacement wind animations.
*   **LOD Support:** Automatically generates Levels of Detail with dithered corss-fade.
*   **Full Source Code:** Modify and extend the tool as needed.


Compatibility
------------

*   **Unity Versions:** Compatible with most Unity versions including Unity 6.
*   **Render Pipelines:** Works with all Built-in, URP, and HDRP pipelines.
    *   *Note:* The materials rely on **Shader Graph**. You must have the Shader Graph package installed.
    *   The provided Shader Graphs are compatible across pipelines, but ensure your project is set up to handle Shader Graph materials.


Installation
------------

### Option 1: Install via Package Manager (Git URL)
- Open the **Package Manager** and select **Add package from git URL** from the add menu.
- Enter **`https://github.com/Warwlock/MTree.git`** to install this package.
- If Unity could not find **git**, consider installing it [here](https://git-scm.com/downloads).

### Option 2: Manual Install
- Download the source code as a ZIP file.
- Extract the folder into your Unity project's `Packages` folder (e.g., `Packages/MTree`).
- If you want to add it to `Assets` folder, rename `Samples~` folder to `Samples` to include tree templates.

### Include Tree Templates
- Go to **Package Manager** and select **MTree** package.
- Under the `Samples` tab, import `Example Templates`.


Documentation
------------

**https://github.com/Warwlock/MTree/blob/main/Documentation~/Documentation.md**


Contributing
------------

Contributions are welcome! Since this is now an open-source project, feel free to submit Pull Requests to fix bugs, clean up the Shader Graphs, or improve the overall tree generation system.


License & Origin
------------

This project is a community-maintained version of MTree.

*   **Original Creator:** [MaximeHerpin](https://github.com/MaximeHerpin)
*   **License:** MIT License
*   **Source of Permission:** https://discussions.unity.com/t/mtree-tree-creation/713264/875

*Note: The original developer stated: "Whoever reads this and has a version of MTree has my permission to publish it with a MIT (or equivalent) license."*

---
*Big thanks to [mherpin](https://github.com/MaximeHerpin) for creating this tool and giving it to the community!*