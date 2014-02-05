using System;
using UnityEngine;

[Serializable]
public class ProjectionSize : Property
{
	public ProjectionSize (Property p) : base(p){}
	
	public ProjectionSize (int subject, float desiredSize, float weight) : base(weight)
	{
		subjectReferences[0] = subject;
		type = Property.PropertyType.ProjectionSize;
		desiredValues = new float[1];
		desiredValues[0] = desiredSize;
	}
	
	#region implemented abstract members of Property
	public override float Evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];
		
		if (float.IsInfinity(mySubject.ProjectionSize))
			return 0;

		return mySubject.Visibility*(1-Mathf.Abs(mySubject.ProjectionSize - DesiredValue));
	}
	#endregion
}
