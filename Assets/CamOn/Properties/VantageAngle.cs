using System;
using UnityEngine;

[Serializable]
public class VantageAngle : Property
{

	/// <summary>
	/// Copy constructor.
	/// This is used to deserialise properties from a shot file.
	/// </summary>
	/// <param name="p">P.</param>
	public VantageAngle (Property p) : base(p){}

	/// <summary>
	/// Initializes a new instance of the <see cref="VantageAngle"/> class.
	/// </summary>
	/// <param name="subjectIndex">Index of the subject.</param>
	/// <param name="h">Horzontal vantage angle in degrees.</param>
	/// <param name="v">Vertical vantage angle in degrees.</param>
	/// <param name="weight">Importance of this property.</param>
	public VantageAngle (int subjectIndex,float h, float v, float weight) : base(weight)
	{
		subjectReferences[0] = subjectIndex;
		propertyType = Property.Type.VantageAngle;
		desiredValues = new float[2];
		desiredValues[0] = h;
		desiredValues[1] = v;
	}

	/// <summary>
	/// Gets or sets the desired horizontal angle.
	/// </summary>
	/// <value>[-180,180] The desired horizontal angle in degrees.</value>
	public float DesiredHorizontalAngle {
		set {desiredValues[0] = Mathf.Clamp(value,-180,180);}
		get {return desiredValues[0];}
	}

	/// <summary>
	/// Gets or sets the desired vertical angle.
	/// </summary>
	/// <value>[-90,90] The desired vertical angle in degrees.</value>
	public float DesiredVerticalAngle {
		set {desiredValues[1] = Mathf.Clamp(value,-90,90);;}
		get {return desiredValues[1];}
	}
	
	#region implemented abstract members of Property
	protected override float evaluate (Actor[] subjectsList)
	{
		Actor mySubject = subjectsList[subjectReferences[0]];

		Vector2 diff = mySubject.CalculateRelativeCameraAngle (DesiredHorizontalAngle, DesiredVerticalAngle);

		float hAngleDifference = Mathf.Abs(diff.x);
		if (hAngleDifference > 180)
			hAngleDifference = 360-hAngleDifference;

		float hSatisfaction = 1-hAngleDifference/180;
		float vSatisfaction = 1-(Mathf.Abs(diff.y))/180;
		
		return (hSatisfaction*vSatisfaction);
	}
	#endregion
}
