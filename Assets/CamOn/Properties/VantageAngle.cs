using System;
using UnityEngine;

[Serializable]
public class VantageAngle : Property
{
	public VantageAngle (Property p) : base(p){}
	
	public VantageAngle (int subject,float h, float v, float weight) : base(weight)
	{
		subjectReferences[0] = subject;
		propertyType = Property.Type.VantageAngle;
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
	protected override float evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];

		float hAngleDifference = Mathf.Abs(mySubject.VantageAngle.x - DesiredHorizontalAngle);
		if (hAngleDifference > 180)
			hAngleDifference = 360-hAngleDifference;

		float hSatisfaction = 1-hAngleDifference/180;
		float vSatisfaction = 1-(Mathf.Abs(mySubject.VantageAngle.y - DesiredVerticalAngle))/180;
		
		return (hSatisfaction*vSatisfaction);
	}
	#endregion
}
