using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class DimOverlaySetup
{
    [MenuItem("CityOfLies/Setup Dim Overlays")]
    static void SetupDimOverlays()
    {
        DialogueRunner dialogueRunner = null;
        UIManager uiManager = null;
        BagUI bagUI = null;

        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            GameObject prefabRoot = prefabStage.prefabContentsRoot;
            dialogueRunner = prefabRoot.GetComponentInChildren<DialogueRunner>();
            uiManager = prefabRoot.GetComponentInChildren<UIManager>();
            bagUI = prefabRoot.GetComponentInChildren<BagUI>();
            Debug.Log("[DimOverlaySetup] Prefab Mode detected — searching within prefab root: " + prefabRoot.name);
        }
        else
        {
            dialogueRunner = Object.FindObjectOfType<DialogueRunner>();
            uiManager = Object.FindObjectOfType<UIManager>();
            bagUI = Object.FindObjectOfType<BagUI>();
            Debug.Log("[DimOverlaySetup] No Prefab Mode — searching active scene.");
        }

        if (dialogueRunner == null)
        {
            Debug.LogWarning("[DimOverlaySetup] DialogueRunner not found. If using a prefab, open PersistentSystems.prefab in Prefab Mode first.");
            return;
        }

        if (uiManager == null)
        {
            Debug.LogWarning("[DimOverlaySetup] UIManager not found. If using a prefab, open PersistentSystems.prefab in Prefab Mode first.");
            return;
        }

        if (bagUI == null)
        {
            Debug.LogWarning("[DimOverlaySetup] BagUI not found. If using a prefab, open PersistentSystems.prefab in Prefab Mode first.");
            return;
        }

        SerializedObject dialogueSO = new SerializedObject(dialogueRunner);
        SerializedObject uiSO = new SerializedObject(uiManager);
        SerializedObject bagSO = new SerializedObject(bagUI);

        GameObject dialoguePanel = dialogueSO.FindProperty("dialoguePanel").objectReferenceValue as GameObject;
        GameObject cluePanel = uiSO.FindProperty("cluePanel").objectReferenceValue as GameObject;
        GameObject reviewPanel = bagSO.FindProperty("reviewPanel").objectReferenceValue as GameObject;
        GameObject bagHud = bagSO.FindProperty("bagHud").objectReferenceValue as GameObject;

        if (dialoguePanel == null)
        {
            Debug.LogWarning("[DimOverlaySetup] dialoguePanel field on DialogueRunner is not assigned.");
            return;
        }

        if (cluePanel == null)
        {
            Debug.LogWarning("[DimOverlaySetup] cluePanel field on UIManager is not assigned.");
            return;
        }

        if (reviewPanel == null)
        {
            Debug.LogWarning("[DimOverlaySetup] reviewPanel field on BagUI is not assigned.");
            return;
        }

        int bagHudIndex = bagHud != null ? bagHud.transform.GetSiblingIndex() : -1;

        GameObject dialogueOverlay = GetOrCreateOverlay("DialogueDimOverlay", dialoguePanel, bagHudIndex);
        GameObject clueOverlay = GetOrCreateOverlay("ClueDimOverlay", cluePanel, bagHudIndex);
        GameObject bagReviewOverlay = GetOrCreateOverlay("BagReviewDimOverlay", reviewPanel, bagHudIndex);

        SerializedProperty dialogueOverlayProp = dialogueSO.FindProperty("dialogueDimOverlay");
        if (dialogueOverlayProp != null)
        {
            dialogueOverlayProp.objectReferenceValue = dialogueOverlay;
            dialogueSO.ApplyModifiedProperties();
        }

        SerializedProperty clueOverlayProp = uiSO.FindProperty("clueDimOverlay");
        if (clueOverlayProp != null)
        {
            clueOverlayProp.objectReferenceValue = clueOverlay;
            uiSO.ApplyModifiedProperties();
        }

        SerializedProperty bagReviewOverlayProp = bagSO.FindProperty("bagReviewDimOverlay");
        if (bagReviewOverlayProp != null)
        {
            bagReviewOverlayProp.objectReferenceValue = bagReviewOverlay;
            bagSO.ApplyModifiedProperties();
        }

        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
        else
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        Debug.Log("[DimOverlaySetup] Done. All three dim overlays created as siblings and wired. Save (Ctrl+S) to persist.");
    }

    // Creates (or finds existing) overlay as a sibling of the target panel,
    // inserted immediately before it in the hierarchy so it renders behind it.
    // Warns if the resulting sibling index would fall at or before BagHUD (bagHudIndex == -1 skips the check).
    static GameObject GetOrCreateOverlay(string overlayName, GameObject panel, int bagHudIndex)
    {
        Transform parent = panel.transform.parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == overlayName)
            {
                Debug.Log("[DimOverlaySetup] " + overlayName + " already exists — reusing.");
                return parent.GetChild(i).gameObject;
            }
        }

        GameObject go = new GameObject(overlayName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        // Place immediately before the panel so it renders behind it.
        // After SetSiblingIndex the overlay is at insertIndex and the panel shifts to insertIndex+1.
        int insertIndex = panel.transform.GetSiblingIndex();
        go.transform.SetSiblingIndex(insertIndex);

        if (bagHudIndex >= 0 && insertIndex <= bagHudIndex)
        {
            Debug.LogWarning("[DimOverlaySetup] " + overlayName + " was placed at sibling index " + insertIndex +
                ", which is at or before BagHUD (index " + bagHudIndex + "). " +
                "Move " + panel.name + " to a higher sibling index than BagHUD in the Canvas hierarchy, then re-run Setup Dim Overlays.");
        }
        else
        {
            Debug.Log("[DimOverlaySetup] Created " + overlayName + " at sibling index " + insertIndex +
                " (before " + panel.name + " at index " + (insertIndex + 1) + ", BagHUD at index " + bagHudIndex + ").");
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 150f / 255f);
        img.raycastTarget = true;

        go.SetActive(false);

        return go;
    }
}
