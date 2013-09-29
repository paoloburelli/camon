using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Cameraman))]
public class CameramanEditor : Editor
{
	Cameraman controller;

	public override void OnInspectorGUI ()
	{
		controller = (Cameraman)target;
		Shot prevShot = controller.Shot;
		controller.Shot = (Shot)EditorGUILayout.ObjectField ("Shot", controller.Shot, typeof(Shot), false);

		
		if (controller.Shot != null) {
			if (controller.Shot != prevShot)
				controller.SubjectsTransform = new Transform[controller.Shot.NumberOfSubjects];
			
			for (int i=0; i<controller.SubjectsTransform.Length; i++)
				controller.SubjectsTransform [i] = (Transform)EditorGUILayout.ObjectField ("Subject " + i, controller.SubjectsTransform [i], typeof(Transform), true);
		
			if (controller.Shot != null && controller.Shot.Properties != null && controller.Subjects != null)
				foreach (Property p in controller.Shot.Properties){
					string sbj = p.Subject.ToString();
					if (p.Type == Property.PropertyType.RelativePosition)
						sbj += " and "+((RelativePosition)p).SecondarySubject;

					EditorGUILayout.LabelField(p.Type+" on "+sbj+" = "+p.Evaluate(controller.Subjects));
				}
			
			if (controller.Shot != null && controller.Shot.Properties != null && controller.Subjects != null)
				for (int i=0;i<controller.Subjects.Length;i++)
					EditorGUILayout.LabelField("Visibility on "+i+" = "+controller.Subjects[i].Visibility);
			
			EditorGUILayout.Separator();
			controller.MovementSpeed = EditorGUILayout.Slider("Movement Speed",controller.MovementSpeed,0,100);
			controller.RotationSpeed = EditorGUILayout.Slider("Rotation Speed",controller.RotationSpeed,0,10);
			
			if (Application.isPlaying) {
				EditorGUILayout.Separator();
				EditorGUILayout.IntField("EPS",controller.EvaluationsPerSecond);
			}
			
			EditorUtility.SetDirty (controller.Shot);
		}	
	}
}