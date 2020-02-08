using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(UnityEngine.EventSystems.StandaloneSinputModule))]
[CanEditMultipleObjects]
public class StandaloneSinputModuleEditor : Editor {

	private UnityEngine.EventSystems.StandaloneSinputModule ssm;

	public override void OnInspectorGUI() {
		ssm = this.target as UnityEngine.EventSystems.StandaloneSinputModule;

		ssm.m_SinputUpButton = EditorGUILayout.TextField("Up Control", ssm.m_SinputUpButton);
		ssm.m_SinputDownButton = EditorGUILayout.TextField("Down Control", ssm.m_SinputDownButton);
		ssm.m_SinputLeftButton = EditorGUILayout.TextField("Left Control", ssm.m_SinputLeftButton);
		ssm.m_SinputRightButton = EditorGUILayout.TextField("Right Control", ssm.m_SinputRightButton);
		ssm.m_SinputSubmitButton = EditorGUILayout.TextField("Submit Control", ssm.m_SinputSubmitButton);
		ssm.m_SinputCancelButton = EditorGUILayout.TextField("Cancel Control", ssm.m_SinputCancelButton);

		
		//ssm.inputActionsPerSecond = EditorGUILayout.FloatField("Input Actions Per Second", ssm.inputActionsPerSecond);
		//ssm.repeatDelay = EditorGUILayout.FloatField("Repeat Delay", ssm.repeatDelay);
		ssm.forceModuleActive = EditorGUILayout.Toggle("Force Module Active", ssm.forceModuleActive);
		//EditorGUILayout.LabelField("hi");
	}

}

