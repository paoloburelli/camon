using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor {

	public override void OnInspectorGUI ()
	{
		Actor actor = (Actor)target;
		actor.Reevaluate(Camera.main);
		actor.Shape = (PrimitiveType)EditorGUILayout.EnumPopup ("Shape ", actor.Shape);
		actor.Offset = EditorGUILayout.Vector3Field ("Offset", actor.Offset);
		actor.Scale = EditorGUILayout.Vector3Field ("Scale", actor.Scale);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Visibility: "+actor.Visibility);
		EditorGUILayout.LabelField("Occlusion: "+actor.Occlusion);
		EditorGUILayout.LabelField("Projection Size: "+actor.ProjectionSize);
		Vector2 vantageAngle = actor.CalculateRelativeCameraAngle(0,0);
		EditorGUILayout.LabelField("Vantage Angle: "+vantageAngle.x+","+vantageAngle.y);
	}
}
