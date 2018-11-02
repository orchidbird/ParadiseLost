using UnityEditor;

[CustomEditor(typeof(CustomUIText))]
public class CustomUITextInspector : UnityEditor.Editor{
	public override void OnInspectorGUI(){
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck()){
			CustomUIText customUiText = (CustomUIText)target;
			customUiText.RefreshOnInspector();
		}
	}
}