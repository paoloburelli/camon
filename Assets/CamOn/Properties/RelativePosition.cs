using System;
using UnityEngine;

[Serializable]
public class RelativePosition : Property
{
	/// <summary>
	/// Possible types of relative placement
	/// </summary>
	public enum Position{Above,Below,LeftOf,RightOf,InFrontOf,Behind}

	/// <summary>
	/// Copy constructor.
	/// This is used to deserialise properties from a shot file.
	/// </summary>
	/// <param name="p">P.</param>
	public RelativePosition (Property p) : base(p){}

	/// <summary>
	/// Initializes a new instance of the <see cref="RelativePosition"/> class.
	/// </summary>
	/// <param name="mainSubjectIndex">Index of the main actor.</param>
	/// <param name="pos">Relative position between the actors.</param>
	/// <param name="secondarySubjectIndex">Index of the secondary actor.</param>
	/// <param name="weight">Importance of this property.</param>
	public RelativePosition (int mainSubjectIndex, Position  pos, int secondarySubjectIndex, float weight) : base(weight)
	{
		subjectReferences = new int[2];
		subjectReferences[0] = mainSubjectIndex;
		subjectReferences[1] = secondarySubjectIndex;

		propertyType = Property.Type.RelativePosition;
		desiredValues[0] = (int)pos;
	}

	/// <summary>
	/// Gets the index of the secondary subject.
	/// </summary>
	/// <value>The index of the secondary subject.</value>
	public int SecondaryActorIndex{
		get {return subjectReferences[1];}
	}

	/// <summary>
	/// Gets or sets the desired relative position between the two subjects.
	/// </summary>
	/// <value>The desired position value.</value>
	public Position DesiredPosition {
		set {desiredValues [0] = (int)value; }
		get {return (RelativePosition.Position)Mathf.FloorToInt(desiredValues[0]);}
	}

	#region implemented abstract members of Property
	protected override float evaluate (Actor[] subjectsList)
	{
		Actor sA = subjectsList[subjectReferences[0]];
		Actor sB = subjectsList[subjectReferences[1]];

		float rVal = Mathf.Ceil(sA.InFrustum)*Mathf.Ceil(sB.InFrustum);

		switch (DesiredPosition) {
		case Position.Above:
			return rVal*isBeyond(sA.ScreenBounds.min.y,sA.ScreenBounds.max.y,sB.ScreenBounds.min.y,sB.ScreenBounds.max.y);
		case Position.Below:
			return rVal*isBeyond(sB.ScreenBounds.min.y,sB.ScreenBounds.max.y,sA.ScreenBounds.min.y,sA.ScreenBounds.max.y);
		case Position.RightOf:
			return rVal*isBeyond(sA.ScreenBounds.min.x,sA.ScreenBounds.max.x,sB.ScreenBounds.min.x,sB.ScreenBounds.max.x);
		case Position.LeftOf:
			return rVal*isBeyond(sB.ScreenBounds.min.x,sB.ScreenBounds.max.x,sA.ScreenBounds.min.x,sA.ScreenBounds.max.x);
		case Position.Behind:
			return rVal*isBeyond(sA.ScreenBounds.min.z,sA.ScreenBounds.max.z,sB.ScreenBounds.min.z,sB.ScreenBounds.max.z);
		case Position.InFrontOf:
			return rVal*isBeyond(sB.ScreenBounds.min.z,sB.ScreenBounds.max.z,sA.ScreenBounds.min.z,sA.ScreenBounds.max.z);
		}

		return 0;
	}
	#endregion

	private float isBeyond(float minA, float maxA, float minB, float maxB){
		if (minA > maxB)
			return 1;

		if (maxA < minB)
			return 0;

		bool bigA = (maxA-minA)>(maxB-minB);

		if (!bigA && minA < minB)
			return 0.5f-0.5f*(minB-minA)/(maxA-minA);

		if (!bigA && maxA > maxB)
			return 0.5f+0.5f*(maxA-maxB)/(maxA-minA);

		if (bigA && minB < minA)
			return 0.5f+0.5f*(minA-minB)/(maxB-minB);

		if (bigA && maxB > maxA)
			return 0.5f-0.5f*(maxB-maxA)/(maxB-minB);

		return 0.5f;
	}
}
