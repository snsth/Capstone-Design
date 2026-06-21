using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HorrorEventManager))]
public class HorrorEventManagerEditor : Editor
{
    SerializedProperty eventsProp;
    SerializedProperty overlayProp;
    List<bool> foldouts = new List<bool>();

    void OnEnable()
    {
        eventsProp = serializedObject.FindProperty("events");
        overlayProp = serializedObject.FindProperty("screenOverlay");
        SyncFoldouts();
    }

    void SyncFoldouts()
    {
        while (foldouts.Count < eventsProp.arraySize) foldouts.Add(true);
        while (foldouts.Count > eventsProp.arraySize) foldouts.RemoveAt(foldouts.Count - 1);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SyncFoldouts();

        // ── 화면 연출 ──────────────────────────────────────
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("화면 연출", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(overlayProp, new GUIContent("Screen Overlay"));

        // ── 이벤트 목록 ────────────────────────────────────
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("공포 이벤트 목록", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            SerializedProperty elem     = eventsProp.GetArrayElementAtIndex(i);
            SerializedProperty labelProp = elem.FindPropertyRelative("label");
            SerializedProperty typeProp  = elem.FindPropertyRelative("type");
            SerializedProperty timeProp  = elem.FindPropertyRelative("triggerTime");

            var eventType = (HorrorEventType)typeProp.enumValueIndex;
            string header = $"  [{i}]  {timeProp.floatValue:0}s  —  {labelProp.stringValue}  ({eventType})";

            // ── 이벤트 박스 ────────────────────────────────
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 헤더 (접기/펼치기)
            EditorGUILayout.BeginHorizontal();
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], header, true, EditorStyles.foldoutHeader);

            // 위/아래 이동 버튼
            GUI.enabled = i > 0;
            if (GUILayout.Button("▲", GUILayout.Width(24))) { eventsProp.MoveArrayElement(i, i - 1); foldouts.Insert(i - 1, foldouts[i]); foldouts.RemoveAt(i + 1); }
            GUI.enabled = i < eventsProp.arraySize - 1;
            if (GUILayout.Button("▼", GUILayout.Width(24))) { eventsProp.MoveArrayElement(i, i + 1); foldouts.Insert(i + 2, foldouts[i]); foldouts.RemoveAt(i); }
            GUI.enabled = true;

            // 삭제 버튼
            GUI.color = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("✕", GUILayout.Width(24))) { eventsProp.DeleteArrayElementAtIndex(i); foldouts.RemoveAt(i); serializedObject.ApplyModifiedProperties(); return; }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();

            // 펼쳐진 경우 필드 표시
            if (foldouts[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(2);

                // 공통 필드
                EditorGUILayout.PropertyField(labelProp,  new GUIContent("이름"));
                EditorGUILayout.PropertyField(typeProp,   new GUIContent("타입"));
                EditorGUILayout.PropertyField(timeProp,   new GUIContent("발동 시각 (초)"));

                EditorGUILayout.Space(4);

                // 타입별 필드
                switch (eventType)
                {
                    case HorrorEventType.PhantomSpawn:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("phantomCount"),  new GUIContent("스폰 수"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("phantomRadius"), new GUIContent("소환 반경"));
                        break;

                    case HorrorEventType.StaticNoise:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("staticRepeatCount"),    new GUIContent("반복 횟수"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("staticRepeatInterval"), new GUIContent("반복 간격 (초)"));
                        break;

                    case HorrorEventType.EnemyFreeze:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("freezeDuration"), new GUIContent("동결 시간 (초)"));
                        break;

                    case HorrorEventType.PatternBreak:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("patternBreakCount"), new GUIContent("이상행동 적 수"));
                        break;

                    case HorrorEventType.Jumpscare:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("jumpscareColor"), new GUIContent("번쩍임 색상"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("damagePercent"),  new GUIContent("피해 비율 (0~0.5)"));
                        break;

                    case HorrorEventType.MassSpawn:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("massSpawnCount"),       new GUIContent("스폰 수"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("massSpawnSpeed"),       new GUIContent("적 속도"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("speedBoostMultiplier"), new GUIContent("부스트 배율"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("speedBoostDuration"),   new GUIContent("부스트 시간 (초)"));
                        break;

                    case HorrorEventType.NoiseAndFreeze:
                        EditorGUILayout.LabelField("— 정적 노이즈", EditorStyles.miniLabel);
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("staticRepeatCount"),    new GUIContent("반복 횟수"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("staticRepeatInterval"), new GUIContent("반복 간격 (초)"));
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("— 동결", EditorStyles.miniLabel);
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("freezeDuration"),       new GUIContent("동결 시간 (초)"));
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("— 대규모 스폰", EditorStyles.miniLabel);
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("massSpawnCount"),       new GUIContent("스폰 수"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("massSpawnSpeed"),       new GUIContent("적 속도"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("speedBoostMultiplier"), new GUIContent("부스트 배율"));
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("speedBoostDuration"),   new GUIContent("부스트 시간 (초)"));
                        break;

                    case HorrorEventType.SuddenDeath:
                        EditorGUILayout.HelpBox("추가 설정 없음 — 무기정지 + 암전 후 게임오버", MessageType.None);
                        break;

                    case HorrorEventType.InputReverse:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("inputDuration"), new GUIContent("지속 시간 (초)"));
                        EditorGUILayout.HelpBox("WASD 방향이 뒤집힙니다.", MessageType.None);
                        break;

                    case HorrorEventType.InputBlock:
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("inputDuration"), new GUIContent("지속 시간 (초)"));
                        EditorGUILayout.HelpBox("플레이어가 움직이지 못합니다.", MessageType.None);
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // ── 추가 버튼 ──────────────────────────────────────
        EditorGUILayout.Space(4);
        GUI.color = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("＋  이벤트 추가", GUILayout.Height(28)))
        {
            eventsProp.arraySize++;
            foldouts.Add(true);
        }
        GUI.color = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}
