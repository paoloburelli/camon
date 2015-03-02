using System;
using UnityEngine;

[Serializable]
public class ProjectionSize : Property
{
	public ProjectionSize (Property p) : base(p){}
	
	public ProjectionSize (int subject, float desiredSize, float weight) : base(weight)
	{
		subjectReferences[0] = subject;
		propertyType = Property.Type.ProjectionSize;
		desiredValues = new float[1];
		desiredValues[0] = desiredSize;
	}
	
	#region implemented abstract members of Property
	protected override float evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];
		
		if (float.IsInfinity(mySubject.ProjectionSize))
			return 0;

		return mySubject.InFrustum*(1-Mathf.Abs(mySubject.ProjectionSize - DesiredValue));
	}
	#endregion
}
