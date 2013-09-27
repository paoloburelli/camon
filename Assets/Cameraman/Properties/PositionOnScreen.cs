using System;
using UnityEngine;

[Serializable]
public class PositionOnScreen : Property
{
	public PositionOnScreen (Property p) : base(p){}
	
	public PositionOnScreen (int subject, float x, float y, float weight) : base(weight)
	{
		subjectReferences[0] = subject;
		type = Property.PropertyType.PositionOnScreen;
		desiredValues = new float[2];
		desiredValues[0] = x;
		desiredValues[1] = y;
	}
	
	public float DesiredHorizontalPosition {
		set {desiredValues[0] = value;}
		get {return desiredValues[0];}
	}
	
	public float DesiredVerticalPosition {
		set {desiredValues[1] = value;}
		get {return desiredValues[1];}
	}
	
	new private float DesiredValue;
	
	#region implemented abstract members of Property
	public override float Evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];
		
		if (float.IsInfinity(mySubject.PositionOnScreen.x) || float.IsInfinity(mySubject.PositionOnScreen.y))
			return 0;
		
		float hSatisfaction = 1-Mathf.Abs(mySubject.PositionOnScreen.x - DesiredHorizontalPosition);
		float vSatisfaction = 1-Mathf.Abs(mySubject.PositionOnScreen.y - DesiredVerticalPosition);
		
		return (hSatisfaction+vSatisfaction)/2;
	}
	#endregion
}
