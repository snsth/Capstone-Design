using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class BR_UIManager : MonoBehaviour
{
    public static BR_UIManager Instance { get; private set; }
    public bool InventoryOpen { get; private set; }

    [Header("시작 자막")]
    [TextArea] public string startSubtitleText = "열쇠를 찾아 비밀문을 열고 탈출하세요.";
    public float subtitleDisplayTime = 4f;
    public float subtitleFadeTime    = 0.8f;

    // State read by OnGUI every frame
    string prompt = "";
    float airRatio = 1f;
    bool showAir;

    string subtitleText  = "";
    float  subtitleAlpha = 0f;

    // Inventory
    Vector2 scrollPos;
    bool showCtx;
    int ctxIndex = -1;
    Vector2 ctxPos;     // GUI-space (y-down from top)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (FindAnyObjectByType<BR_Inventory>() == null)
            new GameObject("BR_Inventory").AddComponent<BR_Inventory>();
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(startSubtitleText))
            StartCoroutine(SubtitleRoutine(startSubtitleText));
    }

    public void ShowSubtitle(string text)
    {
        StopCoroutine(nameof(SubtitleRoutine));
        StartCoroutine(SubtitleRoutine(text));
    }

    IEnumerator SubtitleRoutine(string text)
    {
        subtitleText  = text;
        subtitleAlpha = 0f;

        // Fade in
        for (float t = 0f; t < subtitleFadeTime; t += Time.deltaTime)
        {
            subtitleAlpha = t / subtitleFadeTime;
            yield return null;
        }
        subtitleAlpha = 1f;

        // Hold
        yield return new WaitForSeconds(subtitleDisplayTime);

        // Fade out
        for (float t = 0f; t < subtitleFadeTime; t += Time.deltaTime)
        {
            subtitleAlpha = 1f - t / subtitleFadeTime;
            yield return null;
        }

        subtitleAlpha = 0f;
        subtitleText  = "";
    }

    // ─── Input ───────────────────────────────────
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryOpen = !InventoryOpen;
            Cursor.lockState = InventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = InventoryOpen;
            if (!InventoryOpen) showCtx = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showCtx) { showCtx = false; return; }
            if (InventoryOpen)
            {
                InventoryOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
        }
    }

    // ─── Draw (IMGUI — always works, no Canvas needed) ───
    void OnGUI()
    {
        DrawCrosshair();
        if (!InventoryOpen) DrawPrompt();
        DrawSubtitle();
        if (InventoryOpen)
        {
            DrawInventory();
            if (showCtx) DrawContextMenu();
        }
    }

    void DrawSubtitle()
    {
        if (string.IsNullOrEmpty(subtitleText) || subtitleAlpha <= 0f) return;

        float w = 700f, h = 52f;
        float x = (Screen.width  - w) * 0.5f;
        float y =  Screen.height * 0.76f;

        // 반투명 배경
        GUI.color = new Color(0f, 0f, 0f, 0.62f * subtitleAlpha);
        GUI.DrawTexture(new Rect(x - 12f, y - 4f, w + 24f, h + 8f), Texture2D.whiteTexture);

        // 텍스트
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 22,
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true,
        };
        style.normal.textColor = new Color(1f, 1f, 1f, subtitleAlpha);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), subtitleText, style);

        GUI.color = Color.white;
    }

    void DrawCrosshair()
    {
        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        GUI.DrawTexture(new Rect(cx - 6f, cy - 1f, 12f, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1f, cy - 6f, 2f, 12f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawPrompt()
    {
        if (string.IsNullOrEmpty(prompt)) return;
        var style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = Color.white;
        float w = 460f, h = 46f;
        GUI.Box(new Rect((Screen.width - w) * 0.5f, Screen.height - 130f, w, h), prompt, style);
    }

    void DrawAirBar()
    {
        if (!showAir) return;
        float w = 380f, h = 26f;
        float x = (Screen.width - w) * 0.5f, y = Screen.height - 70f;
        GUI.color = new Color(0.05f, 0.12f, 0.28f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = new Color(0.3f, 0.78f, 1f, 1f);
        GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(airRatio), h), Texture2D.whiteTexture);
        GUI.color = Color.white;
        var s = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        s.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y, w, h), $"O2  {Mathf.CeilToInt(airRatio * 100)}%", s);
    }

    void DrawInventory()
    {
        float pw = 420f, ph = 540f;
        float px = (Screen.width  - pw) * 0.5f;
        float py = (Screen.height - ph) * 0.5f;

        // Panel background
        GUI.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        var ts = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        ts.normal.textColor = new Color(1f, 0.9f, 0.4f);
        GUI.Label(new Rect(px, py + 8f, pw, 36f), "INVENTORY  [TAB]", ts);

        // Divider
        GUI.color = new Color(0.55f, 0.45f, 0.2f, 0.7f);
        GUI.DrawTexture(new Rect(px + 10f, py + 50f, pw - 20f, 1f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Hint
        var hs = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
        hs.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
        GUI.Label(new Rect(px, py + ph - 22f, pw, 18f), "Right-click an item for options", hs);

        // Item list
        var items = BR_Inventory.Instance?.Items;
        Rect listRect = new Rect(px + 6f, py + 56f, pw - 12f, ph - 84f);

        if (items == null || items.Count == 0)
        {
            var es = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            es.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(listRect, "(Empty)", es);
            return;
        }

        string equipped = BR_Inventory.Instance?.EquippedItemId;
        const float rowH = 42f;
        Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, items.Count * rowH);
        scrollPos = GUI.BeginScrollView(listRect, scrollPos, viewRect);

        for (int i = 0; i < items.Count; i++)
        {
            var entry = items[i];
            bool on = entry.id == equipped;
            Rect row = new Rect(0f, i * rowH, viewRect.width, rowH - 2f);

            // Row background
            GUI.color = on
                ? new Color(0.30f, 0.25f, 0.05f, 0.9f)
                : new Color(0.14f, 0.14f, 0.14f, 0.7f);
            GUI.DrawTexture(row, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Label
            var rs = new GUIStyle(GUI.skin.label) { fontSize = 17 };
            rs.normal.textColor = on ? new Color(1f, 0.9f, 0.4f) : Color.white;
            string lbl = on ? $"  {entry.displayName}  [ON]" : $"  {entry.displayName}";
            GUI.Label(new Rect(row.x + 4f, row.y + 3f, row.width - 8f, row.height - 6f), lbl, rs);

            // Right-click → open context menu
            if (Event.current.type == EventType.MouseDown
                && Event.current.button == 1
                && row.Contains(Event.current.mousePosition))
            {
                ctxIndex = i;
                // Convert to GUI space (y flipped from Input.mousePosition)
                ctxPos = new Vector2(
                    Input.mousePosition.x,
                    Screen.height - Input.mousePosition.y);
                showCtx = true;
                Event.current.Use();
            }
        }
        GUI.EndScrollView();
    }

    void DrawContextMenu()
    {
        var items = BR_Inventory.Instance?.Items;
        if (items == null || ctxIndex < 0 || ctxIndex >= items.Count)
        {
            showCtx = false;
            return;
        }
        var entry = items[ctxIndex];

        bool canEquip = entry.itemType == BR_ItemType.Flashlight;
        const float rowH = 32f, pad = 3f;
        int rows = (canEquip ? 1 : 0) + 2; // equip? + drop + cancel
        float mw = 165f, mh = rows * (rowH + pad) + 10f;

        float mx = Mathf.Clamp(ctxPos.x, 2f, Screen.width  - mw - 4f);
        float my = Mathf.Clamp(ctxPos.y, 2f, Screen.height - mh - 4f);

        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.97f);
        GUI.DrawTexture(new Rect(mx, my, mw, mh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float bx = mx + 5f, bw = mw - 10f, by = my + 5f;

        if (canEquip)
        {
            bool isOn = BR_Inventory.Instance?.EquippedItemId == entry.id;
            if (GUI.Button(new Rect(bx, by, bw, rowH), isOn ? "Unequip" : "Equip"))
            {
                BR_Inventory.Instance?.Equip(entry);
                showCtx = false;
            }
            by += rowH + pad;
        }

        if (GUI.Button(new Rect(bx, by, bw, rowH), "Drop"))
        {
            BR_Inventory.Instance?.Drop(entry);
            showCtx = false;
            InventoryOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        by += rowH + pad;

        if (GUI.Button(new Rect(bx, by, bw, rowH), "Cancel"))
            showCtx = false;
    }

    // ─── Public API ──────────────────────────────
    public void SetPrompt(string text)               => prompt  = text ?? "";
    public void SetAir(float ratio, bool show)       { airRatio = ratio; showAir = show; }
    public void RefreshInventory(IReadOnlyList<InventoryEntry> items) { } // IMGUI reads live
    public void ShowContextMenu(InventoryEntry e, Vector2 p) { }
    public void HideContextMenu()                    => showCtx = false;
}
