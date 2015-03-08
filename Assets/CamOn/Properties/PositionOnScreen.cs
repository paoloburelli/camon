using System;
using UnityEngine;

/// <summary>
/// This property can be used to define the position of a a subject on the screen
/// The position is defined in 2D coordinates.
/// </summary>
[Serializable]
public class PositionOnScreen : Property
{
	/// <summary>
	/// Copy constructor.
	/// This is used to deserialise properties from a shot file.
	/// </summary>
	/// <param name="p">P.</param>
	public PositionOnScreen (Property p) : base(p){}

	/// <summary>
	/// Initializes a new instance of the <see cref="PositionOnScreen"/> class.
	/// </summary>
	/// <param name="subjectIndex">Index of the subject in the shot.</param>
	/// <param name="x">[0,1] Desired horizontal coordinate, starting from left.</param>
	/// <param name="y">[0,1] Desired vertical coordinate, starting from bottom.</param>
	/// <param name="weight">[0,1] Importance of this property.</param>
	public PositionOnScreen (int subjectIndex, float x, float y, float weight) : base(weight)
	{
		subjectReferences[0] = subjectIndex;
		propertyType = Property.Type.PositionOnScreen;
		desiredValues = new float[2];
		desiredValues[0] = x;
		desiredValues[1] = y;
	}

	/// <summary>
	/// Gets or sets the desired horizontal position.
	/// </summary>
	/// <value>The desired horizontal position.</value>
	public float DesiredHorizontalPosition {
		set {desiredValues[0] = value;}
		get {return desiredValues[0];}
	}

	/// <summary>
	/// Gets or sets the desired vertical position.
	/// </summary>
	/// <value>The desired vertical position.</value>
	public float DesiredVerticalPosition {
		set {desiredValues[1] = value;}
		get {return desiredValues[1];}
	}
	
	#region implemented abstract members of Property
	protected override float evaluate (SubjectEvaluator[] subjectsList)
	{
		SubjectEvaluator mySubject = subjectsList[subjectReferences[0]];
		
		if (float.IsInfinity(mySubject.PositionOnScreen.x) || float.IsInfinity(mySubject.PositionOnScreen.y))
			return 0;
			
		float hSatisfaction = 1-Mathf.Abs(mySubject.PositionOnScreen.x - DesiredHorizontalPosition)/Mathf.Max(DesiredHorizontalPosition,1-DesiredHorizontalPosition);
		float vSatisfaction = 1-Mathf.Abs(mySubject.PositionOnScreen.y - DesiredVerticalPosition)/Mathf.Max(DesiredVerticalPosition,1-DesiredVerticalPosition);
		
		return mySubject.InFrustum*(hSatisfaction+vSatisfaction)/2;
	}
	#endregion
}
