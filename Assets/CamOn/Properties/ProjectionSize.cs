using System;
using UnityEngine;

/// <summary>
/// This property can be used to define the size of a subject on the screen.
/// The size is defined as the largest dimension (i.e. height or width) of the character on the screen
/// </summary>
[Serializable]
public class ProjectionSize : Property
{
	/// <summary>
	/// Copy constructor.
	/// This is used to deserialise properties from a shot file.
	/// </summary>
	/// <param name="p">P.</param>
	public ProjectionSize (Property p) : base(p){}

	/// <summary>
	/// Initializes a new instance of the <see cref="ProjectionSize"/> class.
	/// </summary>
	/// <param name="subjectIndex">Index of the subject in the shot.</param>
	/// <param name="desiredSize">Desired size.</param>
	/// <param name="weight">Importance of the property.</param>
	public ProjectionSize (int subjectIndex, float desiredSize, float weight) : base(weight)
	{
		subjectReferences[0] = subjectIndex;
		propertyType = Property.Type.ProjectionSize;
		desiredValues = new float[1];
		desiredValues[0] = desiredSize;
	}
	
	#region implemented abstract members of Property
	protected override float evaluate (Actor[] subjectsList)
	{
		Actor mySubject = subjectsList[subjectReferences[0]];
		
		if (float.IsInfinity(mySubject.ProjectionSize))
			return 0;

		return mySubject.InFrustum*(1-Mathf.Abs(mySubject.ProjectionSize - DesiredSize));
	}
	#endregion

	/// <summary>
	/// Gets or sets the desired size on screen of an actor.
	/// </summary>
	/// <value>[0.1,3] The desired projection size value.</value>
	public float DesiredSize {
		set {desiredValues[0] = Mathf.Clamp(value,0.1f,3.0f);}
		get {return desiredValues[0];}
	}
}
