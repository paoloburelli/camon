using System;
using UnityEngine;

[Serializable]
public class VantageAngle : Property
{
	public VantageAngle (Property p) : base(p){}
	
	public VantageAngle (int subject,float h, float v, float weight) : base(weight)
	{
		subjectReferences[0] = subject;
		type = Property.PropertyType.VantageAngle;
		desiredValues = new float[2];
		desiredValues[0] = h;
		desiredValues[1] = v;
	}
	
	public float DesiredHorizontalAngle {
		set {desiredValues[0] = value;}
		get {return desiredValues[0];}
	}
	
	public float DesiredVerticalAngle {
		set {desiredValues[1] = value;}
		get {return desiredValues[1];}
	}
	
	new private float DesiredValue{
		set{}
	}
	
	#region implemented abstract members of Property
	public override float Evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];
		
		float hSatisfaction = 1-(Mathf.Abs(mySubject.VantageAngle.x - DesiredHorizontalAngle))/360;
		float vSatisfaction = 1-(Mathf.Abs(mySubject.VantageAngle.y - DesiredVerticalAngle))/180;
		
		return (hSatisfaction+vSatisfaction)/2;
	}
	#endregion
}
