using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Property
{
	public enum PropertyType {ProjectionSize, VantageAngle, PositionOnScreen};
	
	public PropertyType Type{
		get {return type;}
	}

	public float Weight{
		get {return weight;}
		set {weight = (value <= 1) ? (value >= 0) ? value : 0 : 1;}
	}
	
	[SerializeField]
	protected PropertyType type;
	
	[SerializeField]
	protected float[] desiredValues;
	
	[SerializeField]
	protected int[] subjectReferences;
	
	[SerializeField]
	private float weight;
	
	public Property(Property p){
		weight = p.Weight;
		type = p.Type;
		desiredValues = p.desiredValues;
		subjectReferences = p.subjectReferences;
	}
	
	public Property (float weight)
	{
		this.Weight = weight;
		subjectReferences = new int[1];
	}
	
	public virtual float Evaluate(Subject[] subjectsList){
		return 1;
	}
	
	public int Subject {
		get {return subjectReferences[0];}
		set {subjectReferences[0] = value;}
	}
	
	public float DesiredValue {
		set {desiredValues[0] = value;}
		get {return desiredValues[0];}
	}
	
}

