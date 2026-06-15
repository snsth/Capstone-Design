using UnityEngine;
using UnityEditor;

public static class BR_UISetup
{
    // Creates the BR_UIManager on a dedicated GameObject.
    // All UI is built at runtime — no Inspector wiring needed.
    [MenuItem("BackRoom/1. Create UI Manager")]
    static void CreateUIManager()
    {
        // Remove any existing BR_UIManager to avoid duplicates
        var existing = Object.FindAnyObjectByType<BR_UIManager>();
        if (existing != null)
        {
            Debug.Log("[BR_UISetup] BR_UIManager already exists — skipping creation.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var go = new GameObject("BR_UIManager");
        Undo.RegisterCreatedObjectUndo(go, "Create BR_UIManager");
        go.AddComponent<BR_UIManager>();

        Selection.activeGameObject = go;
        Debug.Log("[BR_UISetup] BR_UIManager created.\nAll UI is generated at runtime — just press Play.");
    }

    // Creates a SpotLight under the Camera and wires it to BR_Inventory.
    [MenuItem("BackRoom/2. Setup Flashlight Light")]
    static void SetupFlashlight()
    {
        // Accept MainCamera or any Camera in the scene
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[BR_UISetup] No Camera found in scene.");
            return;
        }

        // Check entire scene for an existing controller (might be under a different camera)
        var existing = Object.FindAnyObjectByType<BR_FlashlightController>();
        if (existing != null)
        {
            Debug.Log($"[BR_UISetup] Flashlight already exists on '{existing.gameObject.name}'.");
            TryWireFlashlight(existing);
            return;
        }

        var flGo = new GameObject("BR_Flashlight");
        Undo.RegisterCreatedObjectUndo(flGo, "Create BR_Flashlight");
        flGo.transform.SetParent(cam.transform, false);
        flGo.transform.localPosition = new Vector3(0.15f, -0.1f, 0.2f);
        flGo.transform.localRotation = Quaternion.identity;

        var fl = flGo.AddComponent<BR_FlashlightController>();
        TryWireFlashlight(fl);

        Selection.activeGameObject = flGo;
        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log($"[BR_UISetup] Flashlight SpotLight created under '{cam.gameObject.name}'.\n" +
                  "BR_Inventory auto-finds it at runtime — no extra wiring needed.");
    }

    // Wire to BR_Inventory only if it already exists in the scene (edit-time placed).
    // If BR_Inventory is runtime-created, the auto-find in Start() handles it instead.
    static void TryWireFlashlight(BR_FlashlightController fl)
    {
        var inv = Object.FindAnyObjectByType<BR_Inventory>();
        if (inv != null)
        {
            Undo.RecordObject(inv, "Wire Flashlight");
            inv.flashlight = fl;
            EditorUtility.SetDirty(inv.gameObject);
            Debug.Log("[BR_UISetup] Flashlight wired to BR_Inventory.");
        }
        else
        {
            Debug.Log("[BR_UISetup] BR_Inventory not in scene yet — it will auto-find the flashlight at runtime.");
        }
    }
}
