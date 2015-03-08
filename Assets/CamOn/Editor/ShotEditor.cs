using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Shot))]
public class ShotEditor : Editor
{
	Shot shot;
	Property.Type propertyToAdd = Property.Type.ProjectionSize;
	int targetSubject = 0,secondaryTarget = 1;

	public override void OnInspectorGUI ()
	{
		shot = (Shot)target;
		
		shot.NumberOfSubjects = EditorGUILayout.IntSlider ("Number Of Actors", shot.NumberOfSubjects, 0, 4);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Lock");
		shot.LockX = EditorGUILayout.ToggleLeft("X",shot.LockX,GUILayout.MaxWidth(30));
		shot.LockY = EditorGUILayout.ToggleLeft("Y",shot.LockY,GUILayout.MaxWidth(30));
		shot.LockZ = EditorGUILayout.ToggleLeft("Z",shot.LockZ,GUILayout.MaxWidth(30));
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i<shot.NumberOfSubjects; i++) {
			shot.SubjectBounds [i] = (PrimitiveType)EditorGUILayout.EnumPopup ("Actor " + i, shot.SubjectBounds [i]);
			shot.SubjectCenters [i] = EditorGUILayout.Vector3Field ("  Offset", shot.SubjectCenters [i]);
			shot.SubjectScales [i] = EditorGUILayout.Vector3Field ("  Scale", shot.SubjectScales [i]);
		}
		
		if (shot.NumberOfSubjects > 0) {
			shot.FixPropertiesType();
			
			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Properties",EditorStyles.largeLabel);
		
			Property toRemove = null;
			foreach (Property p in shot.Properties) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (p.PropertyType.ToString ()+" (Actor "+p.MainSubjectIndex+")	",GUILayout.MinWidth(140));
				switch (p.PropertyType) {
				case Property.Type.ProjectionSize:
					EditorGUILayout.LabelField("Size",GUILayout.Width(30));
					((ProjectionSize)p).DesiredSize = EditorGUILayout.FloatField(((ProjectionSize)p).DesiredSize,GUILayout.MaxWidth(30));
					break;
				case Property.Type.VantageAngle:
					EditorGUILayout.LabelField("H",EditorStyles.wordWrappedLabel);
					((VantageAngle)p).DesiredHorizontalAngle = EditorGUILayout.FloatField(((VantageAngle)p).DesiredHorizontalAngle,GUILayout.MaxWidth(30));
					EditorGUILayout.LabelField("V",EditorStyles.wordWrappedLabel);
					((VantageAngle)p).DesiredVerticalAngle = EditorGUILayout.FloatField(((VantageAngle)p).DesiredVerticalAngle,GUILayout.MaxWidth(30));
					break;
				case Property.Type.PositionOnScreen:
					EditorGUILayout.LabelField("X",EditorStyles.wordWrappedLabel);
					((PositionOnScreen)p).DesiredHorizontalPosition = EditorGUILayout.FloatField(((PositionOnScreen)p).DesiredHorizontalPosition,GUILayout.MaxWidth(30));
					EditorGUILayout.LabelField("Y",EditorStyles.wordWrappedLabel);
					((PositionOnScreen)p).DesiredVerticalPosition = EditorGUILayout.FloatField(((PositionOnScreen)p).DesiredVerticalPosition,GUILayout.MaxWidth(30));
					break;
				case Property.Type.RelativePosition:
					((RelativePosition)p).DesiredPosition = (RelativePosition.Position)EditorGUILayout.EnumPopup(((RelativePosition)p).DesiredPosition);
					EditorGUILayout.LabelField(" (Actor "+((RelativePosition)p).SecondaryActorIndex+")",GUILayout.MinWidth(30));
					break;
				}
				EditorGUILayout.LabelField("Weight",GUILayout.Width(45));
				p.Weight = EditorGUILayout.FloatField(p.Weight,GUILayout.MaxWidth(30));
				if (GUILayout.Button ("-",GUILayout.Width(30)))
					toRemove = p;
				EditorGUILayout.EndHorizontal ();
			}
			if (toRemove != null)
				shot.Properties.Remove(toRemove);
		
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add")) {
			
				switch (propertyToAdd) {
				case Property.Type.PositionOnScreen:
					shot.Properties.Add (new PositionOnScreen (targetSubject, 0.5f, 0.5f, 1));
					break;
				case Property.Type.ProjectionSize:
					shot.Properties.Add (new ProjectionSize (targetSubject, 1, 1));
					break;
				case Property.Type.VantageAngle:
					shot.Properties.Add (new VantageAngle (targetSubject, 0, 0, 1));
					break;
				case Property.Type.RelativePosition:
					shot.Properties.Add (new RelativePosition (targetSubject, RelativePosition.Position.InFrontOf, secondaryTarget, 1));
					break;
				}
			}
		
			Property.Type tmp = (Property.Type)EditorGUILayout.EnumPopup (propertyToAdd);
			if (tmp != Property.Type.RelativePosition || shot.NumberOfSubjects > 1)
				propertyToAdd = tmp;
			
			string[] options = new string[shot.NumberOfSubjects];
			for (int i=0; i<options.Length; i++)
				options [i] = "Actor " + i.ToString ();
			if (targetSubject > options.Length - 1)
				targetSubject--;
			targetSubject = EditorGUILayout.Popup (targetSubject, options);
			
			
			if (propertyToAdd == Property.Type.RelativePosition)
				secondaryTarget = EditorGUILayout.Popup (secondaryTarget, options);
			
			EditorGUILayout.EndHorizontal ();
		

			
		}
		EditorUtility.SetDirty (shot);	
	}
}
  