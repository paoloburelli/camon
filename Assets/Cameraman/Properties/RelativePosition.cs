using System;
using UnityEngine;

[Serializable]
public class RelativePosition : Property
{
	public enum Position{Above,Below,LeftOf,RightOf,InFrontOf,Behind}
	
	public RelativePosition (Property p) : base(p){}
	
	public RelativePosition (int subjectA, Position  pos, int subjectB, float weight) : base(weight)
	{
		subjectReferences = new int[2];
		subjectReferences[0] = subjectA;
		subjectReferences[1] = subjectB;
		
		type = Property.PropertyType.RelativePosition;
		desiredValues[0] = (int)pos;
	}
	
	public int SecondarySubject{
		get {return subjectReferences[1];}
	}
	
	#region implemented abstract members of Property
	public override float Evaluate (Subject[] subjectsList)
	{
		Subject sA = subjectsList[subjectReferences[0]];
		Subject sB = subjectsList[subjectReferences[1]];
		
		float rVal = Mathf.Ceil(sA.Visibility)*Mathf.Ceil(sB.Visibility);
		
		if (DesiredValue == (int)Position.Above && sA.PositionOnScreen.y > sB.PositionOnScreen.y)
			return rVal;
		
		if (DesiredValue == (int)Position.Below && sA.PositionOnScreen.y < sB.PositionOnScreen.y)
			return rVal;
		
		if (DesiredValue == (int)Position.LeftOf && sA.PositionOnScreen.x < sB.PositionOnScreen.x)
			return rVal;
		
		if (DesiredValue == (int)Position.RightOf && sA.PositionOnScreen.x > sB.PositionOnScreen.x)
			return rVal;
		
		if (DesiredValue == (int)Position.InFrontOf && sA.PositionOnScreen.z < sB.PositionOnScreen.z)
			return rVal;
		
		if (DesiredValue == (int)Position.Behind && sA.PositionOnScreen.z > sB.PositionOnScreen.z)
			return rVal;
		
		return 0;
	}
	#endregion
}
