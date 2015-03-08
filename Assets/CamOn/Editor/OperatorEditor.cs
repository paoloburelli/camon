using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(CameraOperator))]
public class OperatorEditor : Editor
{
	CameraOperator controller;

	public override void OnInspectorGUI ()
	{
		controller = (CameraOperator)target;
		Shot prevShot = controller.Shot;
		controller.Shot = (Shot)EditorGUILayout.ObjectField ("Shot", controller.Shot, typeof(Shot), false);

		
		if (controller.Shot != null) {
			if (controller.Shot != prevShot)
				EditorUtility.SetDirty (controller.Shot);
			
			for (int i=0; i<controller.SubjectsCount; i++){
				Transform prevT = controller.GetSubjectTransform(i);
				controller.AssignSubjectTransform(i,(Transform)EditorGUILayout.ObjectField ("Actor " + i, controller.GetSubjectTransform(i), typeof(Transform), true));

				controller.ModifySubjectOffest(i,EditorGUILayout.Vector3Field("Actor "+i+" offset",controller.GetSubjectOffset(i)));
				controller.ModifySubjectScale(i,EditorGUILayout.Vector3Field("Actor "+i+" scale",controller.GetSubjectScale(i)));

				if (controller.GetSubjectTransform(i) != prevT)
					EditorUtility.SetDirty (controller.Shot);
			}
					
			if (controller.ReadyForEvaluation){
				
				foreach (Property p in controller.Shot.Properties){
					string sbj = p.MainSubjectIndex.ToString();
					if (p.PropertyType == Property.Type.RelativePosition)
						sbj += " "+(((RelativePosition)p).DesiredPosition).ToString()+" "+((RelativePosition)p).SecondaryActorIndex;

					EditorGUILayout.LabelField(p.PropertyType+" on "+sbj+" = "+p.Evaluate(controller.Subjects));
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