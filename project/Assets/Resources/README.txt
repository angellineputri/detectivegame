This folder is required by GameBootstrap.cs.

Move Assets/Prefabs/PersistentSystems.prefab into this folder via the Unity Editor
Project panel (drag-and-drop). Do NOT move it via the file system — Unity must track
the move to keep all scene/prefab references intact (GUID-based).

After moving, the file should be at:
  Assets/Resources/PersistentSystems.prefab

Then remove the PersistentSystems instance from every scene that has it placed in the
Hierarchy (currently: SampleScene, MainMenu, and all ExGF scenes). GameBootstrap.cs
handles instantiation automatically before any scene loads.
