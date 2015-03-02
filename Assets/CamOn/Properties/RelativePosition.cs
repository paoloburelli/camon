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
		
		propertyType = Property.Type.RelativePosition;
		desiredValues[0] = (int)pos;
	}
	
	public int SecondarySubject{
		get {return subjectReferences[1];}
	}
	
	#region implemented abstract members of Property
	protected override float evaluate (Subject[] subjectsList)
	{
		Subject sA = subjectsList[subjectReferences[0]];
		Subject sB = subjectsList[subjectReferences[1]];
		
		float rVal = Mathf.Ceil(sA.InFrustum)*Mathf.Ceil(sB.InFrustum);
		
//		if (DesiredValue == (int)Position.Above && sA.PositionOnScreen.y > sB.PositionOnScreen.y)
//			return rVal;
//		
//		if (DesiredValue == (int)Position.Below && sA.PositionOnScreen.y < sB.PositionOnScreen.y)
//			return rVal;
//		
//		if (DesiredValue == (int)Position.LeftOf && sA.PositionOnScreen.x < sB.PositionOnScreen.x)
//			return rVal;
//		
//		if (DesiredValue == (int)Position.RightOf && sA.PositionOnScreen.x > sB.PositionOnScreen.x)
//			return rVal;
//		
//		if (DesiredValue == (int)Position.InFrontOf && sA.PositionOnScreen.z < sB.PositionOnScreen.z)
//			return rVal;
//		
//		if (DesiredValue == (int)Position.Behind && sA.PositionOnScreen.z > sB.PositionOnScreen.z)
//			return rVal;
		
		switch ((Position)DesiredValue) {
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
