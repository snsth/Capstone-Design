using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public OxygenSystem oxygenSystem;
    public InteractionSystem interactionSystem;
    public InventorySystem inventorySystem;

    // GUI 스타일 캐시
    GUIStyle styleCenter;
    GUIStyle styleItemName;
    GUIStyle styleDesc;
    GUIStyle styleTitle;
    GUIStyle styleSmall;
    bool stylesReady;

    // 인벤토리 UI 상태
    int inventoryHoverIndex = -1;

    // 색상 팔레트
    static readonly Color ColPanelBg        = new Color(0.05f, 0.05f, 0.10f, 0.88f);
    static readonly Color ColSlotNormal      = new Color(0.10f, 0.10f, 0.16f, 0.88f);
    static readonly Color ColSlotSelected    = new Color(0.25f, 0.50f, 0.85f, 0.92f);
    static readonly Color ColSlotHover       = new Color(0.18f, 0.28f, 0.46f, 0.92f);
    static readonly Color ColBorderNormal    = new Color(0.30f, 0.30f, 0.42f, 1.00f);
    static readonly Color ColBorderSelected  = new Color(0.40f, 0.78f, 1.00f, 1.00f);
    static readonly Color ColOxyFull         = new Color(0.20f, 0.82f, 1.00f, 1.00f);
    static readonly Color ColOxyLow          = new Color(1.00f, 0.22f, 0.22f, 1.00f);
    static readonly Color ColInteractPrompt  = new Color(1.00f, 0.90f, 0.30f, 1.00f);
    static readonly Color ColDepthText       = new Color(0.50f, 0.82f, 1.00f, 1.00f);
    static readonly Color ColModeUnderwater  = new Color(0.30f, 0.50f, 1.00f, 1.00f);
    static readonly Color ColModeSurface     = new Color(0.20f, 0.90f, 0.90f, 1.00f);

    void Awake()
    {
        if (playerController == null)    playerController    = GetComponent<PlayerController>();
        if (oxygenSystem == null)        oxygenSystem        = GetComponent<OxygenSystem>();
        if (interactionSystem == null)   interactionSystem   = GetComponent<InteractionSystem>();
        if (inventorySystem == null)     inventorySystem     = GetComponent<InventorySystem>();
    }

    public void OnModeChanged(GameMode mode) { }

    void BuildStyles()
    {
        if (stylesReady) return;
        stylesReady = true;

        styleCenter = new GUIStyle(GUI.skin.label)
        { alignment = TextAnchor.MiddleCenter, fontSize = 14 };

        styleItemName = new GUIStyle(GUI.skin.label)
        { fontSize = 13, fontStyle = FontStyle.Bold };
        styleItemName.normal.textColor = Color.white;

        styleDesc = new GUIStyle(GUI.skin.label)
        { fontSize = 11, wordWrap = true };
        styleDesc.normal.textColor = new Color(0.80f, 0.80f, 0.80f);

        styleTitle = new GUIStyle(GUI.skin.label)
        { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        styleTitle.normal.textColor = Color.white;

        styleSmall = new GUIStyle(GUI.skin.label)
        { fontSize = 10 };
        styleSmall.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
    }

    void OnGUI()
    {
        BuildStyles();

        // ── 인벤토리 전체 화면 ──────────────────────────────
        if (inventorySystem != null && inventorySystem.IsOpen)
        {
            DrawFullInventory();
            return;
        }

        // ── 일반 HUD ────────────────────────────────────────
        DrawCrosshair();
        DrawInteractionPrompt();
        DrawQuickBar();

        bool inWater = playerController != null &&
                       (playerController.CurrentMode == GameMode.WaterSurface ||
                        playerController.CurrentMode == GameMode.Underwater);

        if (inWater)
        {
            DrawDepthMeter();
            DrawOxygenBar();
            DrawModeLabel();
        }
    }

    // ══════════════════════════════════════════════════════
    //  크로스헤어
    // ══════════════════════════════════════════════════════
    void DrawCrosshair()
    {
        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;
        const float s = 8f;

        GUI.color = new Color(1f, 1f, 1f, 0.80f);
        GUI.DrawTexture(new Rect(cx - s, cy - 1f, s * 2f, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1f, cy - s, 2f, s * 2f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // ══════════════════════════════════════════════════════
    //  상호작용 프롬프트
    // ══════════════════════════════════════════════════════
    void DrawInteractionPrompt()
    {
        if (interactionSystem == null) return;
        string prompt = interactionSystem.GetCurrentPrompt();
        if (string.IsNullOrEmpty(prompt)) return;

        const float w = 300f, h = 38f;
        float x = (Screen.width  - w) * 0.5f;
        float y =  Screen.height * 0.65f;

        DrawPanel(new Rect(x, y, w, h), new Color(0f, 0f, 0f, 0.65f));

        GUIStyle s = new GUIStyle(GUI.skin.label)
        { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
        s.normal.textColor = ColInteractPrompt;
        GUI.Label(new Rect(x, y, w, h), prompt, s);
    }

    // ══════════════════════════════════════════════════════
    //  하단 퀵바 (8슬롯)
    // ══════════════════════════════════════════════════════
    void DrawQuickBar()
    {
        if (inventorySystem == null) return;

        const float slotSize = 52f;
        const float pad      =  4f;
        int   total          = inventorySystem.MaxSlots;

        float totalW  = total * (slotSize + pad) - pad;
        float startX  = (Screen.width - totalW) * 0.5f;
        float startY  = Screen.height - slotSize - 10f;

        for (int i = 0; i < total; i++)
        {
            float x = startX + i * (slotSize + pad);
            var   r = new Rect(x, startY, slotSize, slotSize);

            bool hasItem   = i < inventorySystem.Items.Count;
            bool selected  = inventorySystem.SelectedIndex == i;

            DrawPanel(r, ColSlotNormal);

            if (hasItem)
                DrawItemIcon(r, inventorySystem.Items[i], 10f, 14f, slotSize - 20f, slotSize - 22f);

            // 슬롯 번호
            styleSmall.normal.textColor = new Color(0.45f, 0.45f, 0.45f);
            GUI.Label(new Rect(x + 3f, startY + 2f, 16f, 14f), (i + 1).ToString(), styleSmall);

            // 아이템명 (짧게)
            if (hasItem)
            {
                GUIStyle ns = new GUIStyle(GUI.skin.label)
                { fontSize = 8, alignment = TextAnchor.LowerCenter, wordWrap = false };
                ns.normal.textColor = Color.white;
                GUI.Label(new Rect(x, startY + slotSize - 14f, slotSize, 14f),
                    TruncateName(inventorySystem.Items[i].itemName, 7), ns);
            }

            DrawBorder(r, selected ? ColBorderSelected : ColBorderNormal, selected ? 2f : 1f);
        }
    }

    // ══════════════════════════════════════════════════════
    //  수심계
    // ══════════════════════════════════════════════════════
    void DrawDepthMeter()
    {
        if (playerController == null) return;
        float depth = playerController.GetDepth();

        const float w = 120f, h = 46f;
        float x = Screen.width - w - 14f;
        float y = 14f;

        DrawPanel(new Rect(x, y, w, h), new Color(0f, 0.08f, 0.22f, 0.85f));
        DrawBorder(new Rect(x, y, w, h), ColDepthText, 1f);

        GUIStyle labelS = new GUIStyle(GUI.skin.label)
        { fontSize = 11, alignment = TextAnchor.UpperCenter };
        labelS.normal.textColor = ColDepthText;
        GUI.Label(new Rect(x, y + 5f, w, 16f), "▼ 수심", labelS);

        GUIStyle depthS = new GUIStyle(GUI.skin.label)
        { fontSize = 19, fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerCenter };
        depthS.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 18f, w, 24f), $"{depth:F1} m", depthS);
    }

    // ══════════════════════════════════════════════════════
    //  산소 바
    // ══════════════════════════════════════════════════════
    void DrawOxygenBar()
    {
        if (oxygenSystem == null) return;

        float pct = oxygenSystem.OxygenPercent;
        const float w = 220f;
        const float barH = 18f;
        float x = (Screen.width - w) * 0.5f;
        float y = Screen.height - 100f;

        DrawPanel(new Rect(x - 6f, y - 6f, w + 12f, barH + 30f), new Color(0f, 0f, 0f, 0.62f));

        // 라벨
        GUIStyle ls = new GUIStyle(GUI.skin.label)
        { fontSize = 11, alignment = TextAnchor.MiddleCenter };
        ls.normal.textColor = ColOxyFull;
        GUI.Label(new Rect(x, y, w, 16f),
            $"산소  {oxygenSystem.CurrentOxygen:F0} / {oxygenSystem.MaxOxygen}", ls);

        // 바 배경
        GUI.color = new Color(0.08f, 0.08f, 0.18f);
        GUI.DrawTexture(new Rect(x, y + 18f, w, barH), Texture2D.whiteTexture);

        // 바 채움
        Color barColor = Color.Lerp(ColOxyLow, ColOxyFull, pct);
        GUI.color = barColor;
        GUI.DrawTexture(new Rect(x, y + 18f, w * pct, barH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        DrawBorder(new Rect(x, y + 18f, w, barH), new Color(0.3f, 0.4f, 0.6f), 1f);

        // 산소 부족 경고 화면 플래시
        if (pct < 0.25f)
        {
            float pulse = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
            GUI.color = new Color(1f, 0f, 0f, pulse * 0.28f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    // ══════════════════════════════════════════════════════
    //  수중 모드 라벨
    // ══════════════════════════════════════════════════════
    void DrawModeLabel()
    {
        if (playerController == null) return;

        bool diving = playerController.CurrentMode == GameMode.Underwater;
        string text  = diving ? "● 잠수 중" : "~ 수영 중";
        Color  color = diving ? ColModeUnderwater : ColModeSurface;

        GUIStyle s = new GUIStyle(GUI.skin.label)
        { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
        s.normal.textColor = color;
        GUI.Label(new Rect(Screen.width - 150f, 66f, 132f, 22f), text, s);
    }

    // ══════════════════════════════════════════════════════
    //  인벤토리 전체 화면
    // ══════════════════════════════════════════════════════
    void DrawFullInventory()
    {
        // 어두운 오버레이
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        const float panW = 640f, panH = 420f;
        float panX = (Screen.width  - panW) * 0.5f;
        float panY = (Screen.height - panH) * 0.5f;

        DrawPanel(new Rect(panX, panY, panW, panH), ColPanelBg);
        DrawBorder(new Rect(panX, panY, panW, panH), new Color(0.30f, 0.50f, 0.78f), 2f);

        // 제목
        GUI.Label(new Rect(panX, panY + 10f, panW, 30f), "인벤토리", styleTitle);

        // 닫기 힌트
        GUIStyle hint = new GUIStyle(GUI.skin.label)
        { fontSize = 11, alignment = TextAnchor.MiddleRight };
        hint.normal.textColor = new Color(0.45f, 0.45f, 0.45f);
        GUI.Label(new Rect(panX, panY + 10f, panW - 14f, 22f), "[Tab] 닫기", hint);

        // 아이템 조작 힌트
        GUIStyle useHint = new GUIStyle(GUI.skin.label)
        { fontSize = 11, alignment = TextAnchor.MiddleCenter };
        useHint.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
        GUI.Label(new Rect(panX, panY + panH - 26f, panW * 0.55f, 22f),
            "클릭: 선택   [E]: 사용", useHint);

        DrawInventoryGrid(panX + 14f, panY + 50f, panW * 0.55f - 14f);
        DrawItemDetailPanel(panX + panW * 0.56f, panY + 50f, panW * 0.42f, panH - 60f);

        // Tab 키 닫기
        if (Event.current.type == EventType.KeyDown &&
            (Event.current.keyCode == KeyCode.Tab || Event.current.keyCode == KeyCode.I))
        {
            inventorySystem.CloseInventory();
            Event.current.Use();
        }
    }

    void DrawInventoryGrid(float startX, float startY, float availableWidth)
    {
        if (inventorySystem == null) return;

        const int cols      = 4;
        const float slotSz  = 78f;
        const float slotPad = 6f;

        inventoryHoverIndex = -1;

        for (int i = 0; i < inventorySystem.MaxSlots; i++)
        {
            int   col = i % cols;
            int   row = i / cols;
            float sx  = startX + col * (slotSz + slotPad);
            float sy  = startY + row * (slotSz + slotPad);
            var   r   = new Rect(sx, sy, slotSz, slotSz);

            bool hasItem   = i < inventorySystem.Items.Count;
            bool isSelected = inventorySystem.SelectedIndex == i;
            bool isHover   = r.Contains(Event.current.mousePosition) && hasItem;

            if (isHover) inventoryHoverIndex = i;

            Color bg = isSelected ? ColSlotSelected : (isHover ? ColSlotHover : ColSlotNormal);
            DrawPanel(r, bg);

            if (hasItem)
                DrawItemIcon(r, inventorySystem.Items[i], 10f, 14f, slotSz - 20f, slotSz - 28f);

            // 슬롯 번호
            styleSmall.normal.textColor = new Color(0.38f, 0.38f, 0.38f);
            GUI.Label(new Rect(sx + 3f, sy + 2f, 16f, 14f), (i + 1).ToString(), styleSmall);

            // 아이템명
            if (hasItem)
            {
                GUIStyle ns = new GUIStyle(GUI.skin.label)
                { fontSize = 8, alignment = TextAnchor.LowerCenter, wordWrap = true };
                ns.normal.textColor = Color.white;
                GUI.Label(new Rect(sx, sy + slotSz - 18f, slotSz, 18f),
                    inventorySystem.Items[i].itemName, ns);
            }

            DrawBorder(r, isSelected ? ColBorderSelected : ColBorderNormal, isSelected ? 2f : 1f);

            // 마우스 클릭으로 선택
            if (isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                inventorySystem.SelectItem(i);
                Event.current.Use();
            }
        }
    }

    void DrawItemDetailPanel(float x, float y, float w, float h)
    {
        DrawPanel(new Rect(x, y, w, h), new Color(0.03f, 0.03f, 0.09f, 0.92f));
        DrawBorder(new Rect(x, y, w, h), new Color(0.22f, 0.30f, 0.50f), 1f);

        int dispIdx = inventoryHoverIndex >= 0 ? inventoryHoverIndex : inventorySystem.SelectedIndex;

        if (dispIdx < 0 || dispIdx >= inventorySystem.Items.Count)
        {
            GUIStyle empty = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            empty.normal.textColor = new Color(0.38f, 0.38f, 0.38f);
            GUI.Label(new Rect(x, y, w, h), "아이템을 선택하세요", empty);
            return;
        }

        ItemData item = inventorySystem.Items[dispIdx];
        const float pd = 12f;

        // 아이콘
        const float iconSz = 64f;
        float iconX = x + (w - iconSz) * 0.5f;
        float iconY = y + pd;

        if (item.icon != null)
        {
            GUI.DrawTexture(new Rect(iconX, iconY, iconSz, iconSz), item.icon.texture, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.color = GetItemTypeColor(item.itemType);
            GUI.DrawTexture(new Rect(iconX + 4f, iconY + 4f, iconSz - 8f, iconSz - 8f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        float cy = iconY + iconSz + 10f;

        // 아이템 이름
        GUI.Label(new Rect(x + pd, cy, w - pd * 2f, 22f), item.itemName, styleItemName);
        cy += 22f;

        // 타입 태그
        GUIStyle typeS = new GUIStyle(GUI.skin.label) { fontSize = 10 };
        typeS.normal.textColor = GetItemTypeColor(item.itemType);
        GUI.Label(new Rect(x + pd, cy, w - pd * 2f, 16f), $"[{item.itemType.ToUpper()}]", typeS);
        cy += 20f;

        // 구분선
        GUI.color = new Color(0.28f, 0.28f, 0.44f);
        GUI.DrawTexture(new Rect(x + pd, cy, w - pd * 2f, 1f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        cy += 8f;

        // 설명
        float descH = h - (cy - y) - 50f;
        GUI.Label(new Rect(x + pd, cy, w - pd * 2f, descH), item.description, styleDesc);

        // 사용 버튼 (선택된 슬롯만)
        if (inventorySystem.SelectedIndex == dispIdx)
        {
            const float btnH = 30f;
            float btnX = x + pd;
            float btnY = y + h - btnH - pd;
            float btnW = w - pd * 2f;

            DrawPanel(new Rect(btnX, btnY, btnW, btnH), new Color(0.16f, 0.38f, 0.16f, 0.92f));
            DrawBorder(new Rect(btnX, btnY, btnW, btnH), new Color(0.30f, 0.80f, 0.30f), 1f);

            GUIStyle bs = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold };
            bs.normal.textColor = new Color(0.40f, 1.00f, 0.40f);
            GUI.Label(new Rect(btnX, btnY, btnW, btnH), "[E]  사용하기", bs);

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "", GUIStyle.none))
                inventorySystem.UseItem(dispIdx);
        }
    }

    // ══════════════════════════════════════════════════════
    //  유틸리티
    // ══════════════════════════════════════════════════════
    void DrawItemIcon(Rect slotRect, ItemData item, float padX, float padY, float iconW, float iconH)
    {
        if (item.icon != null)
        {
            GUI.DrawTexture(
                new Rect(slotRect.x + padX, slotRect.y + padY, iconW, iconH),
                item.icon.texture, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.color = GetItemTypeColor(item.itemType);
            GUI.DrawTexture(
                new Rect(slotRect.x + padX + 4f, slotRect.y + padY + 2f, iconW - 8f, iconH - 4f),
                Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    void DrawPanel(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawBorder(Rect r, Color color, float t)
    {
        GUI.color = color;
        GUI.DrawTexture(new Rect(r.x,          r.y,            r.width, t),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,          r.yMax - t,     r.width, t),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,          r.y,            t,       r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - t,   r.y,            t,       r.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    Color GetItemTypeColor(string itemType)
    {
        if (itemType == null) return new Color(0.60f, 0.80f, 0.60f);
        switch (itemType.ToLower())
        {
            case "clue":     return new Color(1.00f, 0.90f, 0.30f);
            case "key":      return new Color(0.90f, 0.55f, 0.20f);
            case "note":     return new Color(0.70f, 0.70f, 1.00f);
            case "artifact": return new Color(0.85f, 0.30f, 1.00f);
            default:         return new Color(0.60f, 0.80f, 0.60f);
        }
    }

    static string TruncateName(string name, int maxLen)
    {
        if (name == null) return "";
        return name.Length > maxLen ? name.Substring(0, maxLen) + ".." : name;
    }
}
