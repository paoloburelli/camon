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

			for (int i=0;i<controller.Actors.Length;i++){
				controller.Actors[i] = (Actor)EditorGUILayout.ObjectField("Actor "+i,controller.Actors[i],typeof(Actor),true);
			}
			

			controller.MovementResponsiveness = EditorGUILayout.Slider("Movement Responsiveness",controller.MovementResponsiveness,0,1);
			controller.RotationResponsiveness = EditorGUILayout.Slider("Rotation Responsiveness",controller.RotationResponsiveness,0,1);

			EditorGUILayout.Separator();

			if (controller.ReadyForEvaluation){
				controller.Shot.GetQuality(controller.Actors,controller.transform.GetComponent<Camera>());

				foreach (Property p in controller.Shot.Properties){
					string sbj = p.MainSubjectIndex.ToString();
					if (p.PropertyType == Property.Type.RelativePosition)
						sbj += " "+(((RelativePosition)p).DesiredPosition).ToString()+" "+((RelativePosition)p).SecondaryActorIndex;

					EditorGUILayout.LabelField(p.PropertyType+" on "+sbj+" = "+p.Evaluate(controller.Actors));
				}
			

				for (int i=0;i<controller.Actors.Length;i++)
					EditorGUILayout.LabelField("Visibility on "+i+" = "+controller.Actors[i].Visibility);
			}


		}	
	}
}