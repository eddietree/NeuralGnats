using UnityEngine;
using System.Collections;

#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(Transform))]
public class TransformInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;

        if (GUILayout.Button("Reset Transforms"))
        {
            Undo.RegisterCompleteObjectUndo(t, "Reset Transforms " + t.name);
            t.transform.localRotation = Quaternion.identity;
            t.transform.localScale = Vector3.one;
            t.transform.localPosition = Vector3.zero;
        }

        // Replicate the standard transform inspector gui
        //EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel = 0;

        // position
        GUILayout.BeginHorizontal();
        Vector3 position = EditorGUILayout.Vector3Field("Position", t.localPosition);
        if (GUILayout.Button("X", GUILayout.Width(20))) position = Vector3.zero;
        GUILayout.EndHorizontal();

        // rotation
        GUILayout.BeginHorizontal();
        Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
        if (GUILayout.Button("X", GUILayout.Width(20))) eulerAngles = Vector3.zero;
        GUILayout.EndHorizontal();

        // scale
        GUILayout.BeginHorizontal();
        Vector3 scale = EditorGUILayout.Vector3Field("Scale", t.localScale);
        if (GUILayout.Button("X", GUILayout.Width(20))) scale = Vector3.one;
        GUILayout.EndHorizontal();

        //EditorGUIUtility.LookLikeInspector();
        if (GUI.changed)
        {
            Undo.RegisterCompleteObjectUndo(t, "Transform Change");
            t.localPosition = FixIfNaN(position);
            t.localEulerAngles = FixIfNaN(eulerAngles);
            t.localScale = FixIfNaN(scale);
        }
    }
    private Vector3 FixIfNaN(Vector3 v)
    {
        if (float.IsNaN(v.x))
        {
            v.x = 0;
        }
        if (float.IsNaN(v.y))
        {
            v.y = 0;
        }
        if (float.IsNaN(v.z))
        {
            v.z = 0;
        }
        return v;
    }
}

#endif

public static class TransformExtension
{
    public static void ResetLocalPos(this Transform a)
    {
        a.localPosition = Vector3.zero;
    }

    public static void ResetLocalRot(this Transform a)
    {
        a.localRotation = Quaternion.identity;
    }

    public static void ResetLocalScale(this Transform a)
    {
        a.localScale = Vector3.one;
    }

    public static void ResetAll(this Transform a)
    {
        a.localPosition = Vector3.zero;
        a.localRotation = Quaternion.identity;
        a.localScale = Vector3.one;
    }
}