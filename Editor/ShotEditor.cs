using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Shot))]
public class ShotEditor : Editor
{
	Shot shot;
	Property.PropertyType propertyToAdd = Property.PropertyType.ProjectionSize;
	int targetSubject = 0,secondaryTarget = 1;

	public override void OnInspectorGUI ()
	{
		shot = (Shot)target;
		
		shot.NumberOfSubjects = EditorGUILayout.IntSlider ("Number Of Subjects", shot.NumberOfSubjects, 0, 4);
		
		while (shot.SubjectBounds.Count < shot.NumberOfSubjects) {
			shot.SubjectCenters.Add (0.5f * Vector3.one);
			shot.SubjectScales.Add (Vector3.one);
			shot.SubjectBounds.Add (PrimitiveType.Capsule);
		}
		while (shot.SubjectBounds.Count > shot.NumberOfSubjects) {
			shot.SubjectBounds.RemoveAt (shot.SubjectBounds.Count - 1);
			shot.SubjectCenters.RemoveAt (shot.SubjectCenters.Count - 1);
			shot.SubjectScales.RemoveAt (shot.SubjectScales.Count - 1);
		}
		
		
		for (int i = 0; i<shot.NumberOfSubjects; i++) {
			shot.SubjectBounds [i] = (PrimitiveType)EditorGUILayout.EnumPopup ("Subject " + i, shot.SubjectBounds [i]);
			shot.SubjectCenters [i] = EditorGUILayout.Vector3Field ("  Center", shot.SubjectCenters [i]);
			shot.SubjectScales [i] = EditorGUILayout.Vector3Field ("  Scale", shot.SubjectScales [i]);
		}
		
		if (shot.NumberOfSubjects > 0) {
			shot.FixPropertyTypes();
			
			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Properties",EditorStyles.largeLabel);
		
			Property toRemove = null;
			foreach (Property p in shot.Properties) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (p.Type.ToString ()+" (Subject "+p.Subject+")	",GUILayout.MinWidth(140));
				switch (p.Type) {
				case Property.PropertyType.ProjectionSize:
					EditorGUILayout.LabelField("Size",GUILayout.Width(30));
					p.DesiredValue = EditorGUILayout.FloatField(p.DesiredValue,GUILayout.MaxWidth(30));
					break;
				case Property.PropertyType.VantageAngle:
					EditorGUILayout.LabelField("H",EditorStyles.wordWrappedLabel);
					((VantageAngle)p).DesiredHorizontalAngle = EditorGUILayout.FloatField(((VantageAngle)p).DesiredHorizontalAngle,GUILayout.MaxWidth(30));
					EditorGUILayout.LabelField("V",EditorStyles.wordWrappedLabel);
					((VantageAngle)p).DesiredVerticalAngle = EditorGUILayout.FloatField(((VantageAngle)p).DesiredVerticalAngle,GUILayout.MaxWidth(30));
					break;
				case Property.PropertyType.PositionOnScreen:
					EditorGUILayout.LabelField("X",EditorStyles.wordWrappedLabel);
					((PositionOnScreen)p).DesiredHorizontalPosition = EditorGUILayout.FloatField(((PositionOnScreen)p).DesiredHorizontalPosition,GUILayout.MaxWidth(30));
					EditorGUILayout.LabelField("Y",EditorStyles.wordWrappedLabel);
					((PositionOnScreen)p).DesiredVerticalPosition = EditorGUILayout.FloatField(((PositionOnScreen)p).DesiredVerticalPosition,GUILayout.MaxWidth(30));
					break;
				case Property.PropertyType.RelativePosition:
					p.DesiredValue = (int)((RelativePosition.Position)EditorGUILayout.EnumPopup((RelativePosition.Position)p.DesiredValue));
					EditorGUILayout.LabelField(" (Subject "+((RelativePosition)p).SecondarySubject+")",GUILayout.MinWidth(30));
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
				case Property.PropertyType.PositionOnScreen:
					shot.Properties.Add (new PositionOnScreen (targetSubject, 0.5f, 0.5f, 1));
					break;
				case Property.PropertyType.ProjectionSize:
					shot.Properties.Add (new ProjectionSize (targetSubject, 1, 1));
					break;
				case Property.PropertyType.VantageAngle:
					shot.Properties.Add (new VantageAngle (targetSubject, 0, 0, 1));
					break;
				case Property.PropertyType.RelativePosition:
					shot.Properties.Add (new RelativePosition (targetSubject, RelativePosition.Position.InFrontOf, secondaryTarget, 1));
					break;
				}
			}
		
			Property.PropertyType tmp = (Property.PropertyType)EditorGUILayout.EnumPopup (propertyToAdd);
			if (tmp != Property.PropertyType.RelativePosition || shot.NumberOfSubjects > 1)
				propertyToAdd = tmp;
			
			string[] options = new string[shot.NumberOfSubjects];
			for (int i=0; i<options.Length; i++)
				options [i] = "Subject " + i.ToString ();
			if (targetSubject > options.Length - 1)
				targetSubject--;
			targetSubject = EditorGUILayout.Popup (targetSubject, options);
			
			
			if (propertyToAdd == Property.PropertyType.RelativePosition)
				secondaryTarget = EditorGUILayout.Popup (secondaryTarget, options);
			
			EditorGUILayout.EndHorizontal ();
		

			
		}
		EditorUtility.SetDirty (shot);	
	}
}
  