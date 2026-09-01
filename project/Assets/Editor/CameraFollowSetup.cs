using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class CameraFollowSetup
{
    static string[] gameplayScenePaths = new string[]
    {
        "Assets/Scenes/CityOfLies/ExGF/Playthrough1/ExGF_Diner_P1.unity",
        "Assets/Scenes/CityOfLies/ExGF/Playthrough1/ExGF_Kitchen_P1.unity",
        "Assets/Scenes/CityOfLies/ExGF/Playthrough1/ExGF_ExRoom_P1.unity",
        "Assets/Scenes/CityOfLies/ExGF/Playthrough1/ExGF_ChefRoom_P1.unity",
        "Assets/Scenes/CityOfLies/ExGF/Playthrough1/ExGF_Court_P1.unity",
        "Assets/Scenes/CityOfLies/ExGF/Playthrough2/ExGF_ExRoom_P2.unity",
    };

    [MenuItem("CityOfLies/Setup Camera Follow In All Scenes")]
    static void SetupCameraFollow()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        int processedCount = 0;
        int skippedCount = 0;

        foreach (string scenePath in gameplayScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogWarning("[CameraFollowSetup] No Main Camera found in " + scenePath + " — skipped.");
                skippedCount++;
                continue;
            }

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                cameraFollow.target = player.transform;
                Debug.Log("[CameraFollowSetup] Wired " + scene.name + " — Main Camera follows " + player.name);
            }
            else
            {
                Debug.LogWarning("[CameraFollowSetup] No GameObject tagged 'Player' found in " + scene.name + " — CameraFollow added but target not assigned. Assign it manually in the Inspector.");
            }

            EditorSceneManager.SaveScene(scene);
            processedCount++;
        }

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log("[CameraFollowSetup] Camera follow setup done. Processed: " + processedCount + ", skipped: " + skippedCount);
    }

    [MenuItem("CityOfLies/Setup Camera Bounds In All Scenes")]
    static void SetupCameraBounds()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        foreach (string scenePath in gameplayScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogWarning("[CameraBoundsSetup] No Main Camera in " + scene.name + " — skipped.");
                continue;
            }

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

            if (cameraFollow == null)
            {
                Debug.LogWarning("[CameraBoundsSetup] No CameraFollow component on Main Camera in " + scene.name + " — run 'Setup Camera Follow In All Scenes' first, then re-run this.");
                continue;
            }

            bool boundsAssigned = TryAssignBoundsFromTilemap(scene.name, cameraFollow);

            if (!boundsAssigned)
            {
                Debug.LogWarning(
                    "[CameraBoundsSetup] Could not auto-calculate bounds for " + scene.name + ". " +
                    "No reliable room boundary source found (expected a Grid named 'WallsAndFloors' with child Tilemaps). " +
                    "Open " + scene.name + " in the Editor and set CameraFollow.minBounds and CameraFollow.maxBounds manually on Main Camera.");
            }

            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log("[CameraBoundsSetup] Camera bounds setup done.");
    }

    static bool TryAssignBoundsFromTilemap(string sceneName, CameraFollow cameraFollow)
    {
        GameObject wallsAndFloors = GameObject.Find("WallsAndFloors");

        if (wallsAndFloors == null)
        {
            return false;
        }

        Tilemap[] tilemaps = wallsAndFloors.GetComponentsInChildren<Tilemap>();

        if (tilemaps.Length == 0)
        {
            return false;
        }

        bool anyValidTilemap = false;
        float worldMinX = float.MaxValue;
        float worldMinY = float.MaxValue;
        float worldMaxX = float.MinValue;
        float worldMaxY = float.MinValue;

        foreach (Tilemap tilemap in tilemaps)
        {
            BoundsInt cells = tilemap.cellBounds;

            if (cells.size.x == 0 || cells.size.y == 0)
            {
                continue;
            }

            Vector3 cellMin = new Vector3Int(cells.xMin, cells.yMin, 0);
            Vector3 cellMax = new Vector3Int(cells.xMax, cells.yMax, 0);

            Vector3 worldMin = tilemap.CellToWorld(new Vector3Int(cells.xMin, cells.yMin, 0));
            Vector3 worldMax = tilemap.CellToWorld(new Vector3Int(cells.xMax, cells.yMax, 0));

            if (worldMin.x < worldMinX) worldMinX = worldMin.x;
            if (worldMin.y < worldMinY) worldMinY = worldMin.y;
            if (worldMax.x > worldMaxX) worldMaxX = worldMax.x;
            if (worldMax.y > worldMaxY) worldMaxY = worldMax.y;

            anyValidTilemap = true;
        }

        if (!anyValidTilemap)
        {
            return false;
        }

        cameraFollow.minBounds = new Vector2(worldMinX, worldMinY);
        cameraFollow.maxBounds = new Vector2(worldMaxX, worldMaxY);

        Debug.Log("[CameraBoundsSetup] " + sceneName + " bounds set from WallsAndFloors tilemap — min: " + cameraFollow.minBounds + ", max: " + cameraFollow.maxBounds);

        return true;
    }
}
