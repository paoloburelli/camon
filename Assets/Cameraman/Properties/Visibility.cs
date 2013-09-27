using System;
using UnityEngine;

[Serializable]
public class Visibility : Property
{	
	public Visibility (Property p) : base(p){}
	
	public Visibility (int subject, float desiredVesibility, float weight) : base(weight)
	{
		type = Property.PropertyType.Visibility;
		desiredValues = new float[1];
		desiredValues[0] = desiredVesibility;
		subjectReferences[0] = subject;
	}
		
	#region implemented abstract members of Property
	public override float Evaluate (Subject[] subjectsList)
	{
		Subject mySubject = subjectsList[subjectReferences[0]];
		return (1-Mathf.Abs(mySubject.Visibility - DesiredValue));
	}
	#endregion
}
