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
			
			for (int i=0; i<controller.SubjectsCount; i++){
				Transform prevT = controller.GetSubjectTransform(i);
				controller.SetSubjectTransform(i,(Transform)EditorGUILayout.ObjectField ("Subject " + i, controller.GetSubjectTransform(i), typeof(Transform), true));

				controller.SetSubjectCenter(i,EditorGUILayout.Vector3Field("Subject "+i+" center",controller.GetSubjectCenter(i)));
				controller.SetSubjectScale(i,EditorGUILayout.Vector3Field("Subject "+i+" scale",controller.GetSubjectScale(i)));

				if (controller.GetSubjectTransform(i) != prevT)
					EditorUtility.SetDirty (controller.Shot);
			}
					
			if (controller.ReadyForEvaluation){
				
				foreach (Property p in controller.Shot.Properties){
					string sbj = p.Subject.ToString();
					if (p.Type == Property.PropertyType.RelativePosition)
						sbj += " "+((RelativePosition.Position)p.DesiredValue).ToString()+" "+((RelativePosition)p).SecondarySubject;

					EditorGUILayout.LabelField(p.Type+" on "+sbj+" = "+p.Evaluate(controller.Subjects));
				}
			

				for (int i=0;i<controller.Subjects.Length;i++)
					EditorGUILayout.LabelField("Visibility on "+i+" = "+controller.Subjects[i].Visibility);
			}
			
			EditorGUILayout.Separator();
			controller.MovementResponsiveness = EditorGUILayout.Slider("Movement Responsiveness",controller.MovementResponsiveness,0,1);
			controller.RotationResponsiveness = EditorGUILayout.Slider("Rotation Responsiveness",controller.RotationResponsiveness,0,1);
		}	
	}
}