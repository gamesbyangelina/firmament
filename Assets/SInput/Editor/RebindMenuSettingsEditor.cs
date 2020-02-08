using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SinputSystems.Rebinding;

[CustomEditor(typeof(RebindMenuSettings))]
public class RebindMenuSettingsEditor : Editor {

	public override void OnInspectorGUI() {
		RebindMenuSettings menuSettings = (RebindMenuSettings)target;
		EditorGUI.BeginChangeCheck();

		//show settings
		menuSettings.showSettings = EditorGUILayout.Toggle("Show settings:", menuSettings.showSettings);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space();

		//mouse sensitivity
		menuSettings.showMouseSensitivity = EditorGUILayout.Toggle("Mouse Sensitivity:", menuSettings.showMouseSensitivity);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Min/Max:", GUILayout.Width(100));
		menuSettings.minMouseSens = EditorGUILayout.FloatField(menuSettings.minMouseSens);
		menuSettings.maxMouseSens = EditorGUILayout.FloatField(menuSettings.maxMouseSens);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space();

		//toggleable controls
		EditorGUILayout.LabelField("Toggleable Controls:");
		for (int i=0; i<menuSettings.toggleableControls.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			menuSettings.toggleableControls[i] = EditorGUILayout.TextField(menuSettings.toggleableControls[i]);
			menuSettings.toggleableControls[i] = SinputSystems.SinputFileIO.SanitiseStringForSaving(menuSettings.toggleableControls[i]);
			if (GUILayout.Button("x")) {
				menuSettings.toggleableControls.RemoveAt(i);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("+")) {
			menuSettings.toggleableControls.Add("Control Name Here");
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space();

		//invertable smart controls
		EditorGUILayout.LabelField("Invertable Smart Controls:");
		for (int i = 0; i < menuSettings.invertableSmartControls.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			menuSettings.invertableSmartControls[i] = EditorGUILayout.TextField(menuSettings.invertableSmartControls[i]);
			menuSettings.invertableSmartControls[i] = SinputSystems.SinputFileIO.SanitiseStringForSaving(menuSettings.invertableSmartControls[i]);
			if (GUILayout.Button("x")) {
				menuSettings.invertableSmartControls.RemoveAt(i);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("+")) {
			menuSettings.invertableSmartControls.Add("Smart Control Name Here");
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Space();

		//scalable smart controls
		EditorGUILayout.LabelField("Scalable Smart Controls:");
		bool deleteScalable = false;
		for (int i = 0; i < menuSettings.scalables.Count; i++) {
			deleteScalable = false;
			RebindMenuSettings.scalable activeScalable = menuSettings.scalables[i];

			EditorGUILayout.BeginHorizontal();
			activeScalable.scalableName = EditorGUILayout.TextField(activeScalable.scalableName);
			activeScalable.scalableName = SinputSystems.SinputFileIO.SanitiseStringForSaving(activeScalable.scalableName);
			if (GUILayout.Button("x")) {
				deleteScalable = true;
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(20));
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Min/Max:", GUILayout.Width(100));
			activeScalable.minScale = EditorGUILayout.FloatField(activeScalable.minScale);
			activeScalable.maxScale = EditorGUILayout.FloatField(activeScalable.maxScale);
			EditorGUILayout.EndHorizontal();

			for (int k=0; k<activeScalable.scalableSmartControls.Count; k++) {
				EditorGUILayout.BeginHorizontal();
				activeScalable.scalableSmartControls[k] = EditorGUILayout.TextField(activeScalable.scalableSmartControls[k]);

				activeScalable.scalableSmartControls[k] = SinputSystems.SinputFileIO.SanitiseStringForSaving(activeScalable.scalableSmartControls[k]);
				if (GUILayout.Button("x")) {
					activeScalable.scalableSmartControls.RemoveAt(k);
					k--;
				}
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20))) {
				activeScalable.scalableSmartControls.Add("Smart Control Name Here");
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			menuSettings.scalables[i] = activeScalable;
			if (deleteScalable) {
				menuSettings.scalables.RemoveAt(i);
				i--;
			}
		}

		if (GUILayout.Button("+")) {
			RebindMenuSettings.scalable newScalable = new RebindMenuSettings.scalable();
			newScalable.scalableName = "Setting Name";
			newScalable.minScale = 0.01f;
			newScalable.maxScale = 4f;
			newScalable.scalableSmartControls = new List<string>();
			menuSettings.scalables.Add(newScalable);
		}

		if (EditorGUI.EndChangeCheck()) {
			//something was changed
			EditorUtility.SetDirty(menuSettings);
		}
	}
}
