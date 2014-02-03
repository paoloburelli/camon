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
				EditorUtility.SetDirty (controller.Shot);
			
			for (int i=0; i<controller.SubjectTransformsCount; i++){
				Transform prevT = controller.GetSubjectTransform(i);
				controller.SetSubjectTransform(i,(Transform)EditorGUILayout.ObjectField ("Subject " + i, controller.GetSubjectTransform(i), typeof(Transform), true));
				if (controller.GetSubjectTransform(i) != prevT && Application.isPlaying){
					EditorUtility.SetDirty (controller.Shot);
					controller.Reset();
				}
			}
		
			bool eval = controller.Subjects != null;
			if (eval)
				foreach (Subject s in controller.Subjects)
					if (s == null)
						eval = false;
			
			if (controller.Shot != null && controller.Shot.Properties != null && controller.Subjects != null && eval)
				foreach (Property p in controller.Shot.Properties){
					string sbj = p.Subject.ToString();
					if (p.Type == Property.PropertyType.RelativePosition)
						sbj += " "+((RelativePosition.Position)p.DesiredValue).ToString()+" "+((RelativePosition)p).SecondarySubject;

					EditorGUILayout.LabelField(p.Type+" on "+sbj+" = "+p.Evaluate(controller.Subjects));
				}
			
			if (controller.Shot != null && controller.Shot.Properties != null && controller.Subjects != null && eval)
				for (int i=0;i<controller.Subjects.Length;i++)
					EditorGUILayout.LabelField("Visibility on "+i+" = "+controller.Subjects[i].Visibility);
			
			EditorGUILayout.Separator();
			controller.MovementSpeed = EditorGUILayout.Slider("Movement Speed",controller.MovementSpeed,0,100);
			controller.RotationSpeed = EditorGUILayout.Slider("Rotation Speed",controller.RotationSpeed,0,10);
			
			if (Application.isPlaying) {
				EditorGUILayout.Separator();
				EditorGUILayout.IntField("EPS",controller.EvaluationsPerSecond);
			}
		}	
	}
}