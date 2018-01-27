using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

static public class EditorTool
{
	static public void CreateAsset <T> (string path) where T : ScriptableObject
	{
		var asset = ScriptableObject.CreateInstance <T> ();
		AssetDatabase.CreateAsset (asset, path);
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}

	static public bool DefaultHandlesHidden
	{
		get {
			Type type = typeof(Tools);
			FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			return ((bool)field.GetValue(null));
		}
		set {
			Type type = typeof(Tools);
			FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			field.SetValue(null, value);
		}
	}

	static public bool EnumToggleList<TEnum> (ref TEnum val) where TEnum : struct, IConvertible, IComparable
    {
        var oldVal = val;
        var names = Enum.GetNames(typeof(TEnum));
        var vals = (TEnum[])Enum.GetValues(typeof(TEnum));

        for (int i = 0; i < names.Length; ++i) {
            var mode = names[i];
            var toggled = EditorGUILayout.Toggle(mode, mode == val.ToString());
            if (toggled) {
                val = vals[i];
            }
        }

        return !val.Equals(oldVal);
    }
}
