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
			
			for (int i=0; i<controller.ActorsCount; i++){
				Transform prevT = controller.GetActorTransform(i);
				controller.AssignActorTransform(i,(Transform)EditorGUILayout.ObjectField ("Subject " + i, controller.GetActorTransform(i), typeof(Transform), true));

				controller.ModifyActorOffest(i,EditorGUILayout.Vector3Field("Subject "+i+" offset",controller.GetActorOffset(i)));
				controller.ModifyActorScale(i,EditorGUILayout.Vector3Field("Subject "+i+" scale",controller.GetActorScale(i)));

				if (controller.GetActorTransform(i) != prevT)
					EditorUtility.SetDirty (controller.Shot);
			}
					
			if (controller.ReadyForEvaluation){
				
				foreach (Property p in controller.Shot.Properties){
					string sbj = p.Subject.ToString();
					if (p.PropertyType == Property.Type.RelativePosition)
						sbj += " "+((RelativePosition.Position)p.DesiredValue).ToString()+" "+((RelativePosition)p).SecondarySubject;

					EditorGUILayout.LabelField(p.PropertyType+" on "+sbj+" = "+p.Evaluate(controller.Actors));
				}
			

				for (int i=0;i<controller.Actors.Length;i++)
					EditorGUILayout.LabelField("Visibility on "+i+" = "+controller.Actors[i].Visibility);
			}

			EditorGUILayout.Separator();
			controller.MovementResponsiveness = EditorGUILayout.Slider("Movement Responsiveness",controller.MovementResponsiveness,0,1);
			controller.RotationResponsiveness = EditorGUILayout.Slider("Rotation Responsiveness",controller.RotationResponsiveness,0,1);
		}	
	}
}