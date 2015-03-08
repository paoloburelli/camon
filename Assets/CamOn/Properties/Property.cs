using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for the shot properties
/// </summary>
[Serializable]
public class Property
{
	/// <summary>
	/// Type of the property.
	/// This is used for correct deserialisation and fast type checking
	/// </summary>
	public enum Type {ProjectionSize, VantageAngle, PositionOnScreen, RelativePosition};

	/// <summary>
	/// Gets the type of the property.
	/// </summary>
	/// <value>The type of the property.</value>
	public Type PropertyType{
		get {return propertyType;}
	}


	/// <summary>
	///  Fixes the property after a deserialisation
	/// </summary>
	/// <param name="property">A reference to the property to be fixed</param>
	public static void FixType(ref Property property){
		switch (property.PropertyType) {
		case Property.Type.ProjectionSize:
			if (!(property is ProjectionSize))
				property = new ProjectionSize (property);
			break;

		case Property.Type.PositionOnScreen:
			if (!(property is PositionOnScreen))
				property = new PositionOnScreen (property);
			break;

		case Property.Type.VantageAngle:
			if (!(property is VantageAngle))
				property = new VantageAngle (property);
			break;

		case Property.Type.RelativePosition:
			if (!(property is RelativePosition))
				property = new RelativePosition (property);
			break;
		}
	}

	/// <summary>
	/// Gets or sets the importance of the property.
	/// </summary>
	/// <value>[0,1] The weight.</value>
	public float Weight{
		get {return weight;}
		set {weight = Mathf.Clamp01(value);}
	}
	
	[SerializeField]
	protected Type propertyType;
	
	[SerializeField]
	protected float[] desiredValues;
	
	[SerializeField]
	protected int[] subjectReferences;
	
	[SerializeField]
	private float weight;

	private float satisfaction;

	/// <summary>
	/// Gets the level of satisfaction of this property since its last evluation.
	/// </summary>
	/// <value>[0,1] The satisfaction.</value>
	public float Satisfaction {
		get {
			return satisfaction;
			}
	}

	/// <summary>
	/// A copy constructor used only in sub classes
	/// </summary>
	/// <param name="p">The original property</param>
	protected Property(Property p){
		Weight = p.Weight;
		propertyType = p.PropertyType;
		desiredValues = p.desiredValues;
		subjectReferences = p.subjectReferences;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Property"/> class.
	/// </summary>
	/// <param name="weight">Weight.</param>
	protected Property (float weight)
	{
		Weight = weight;
		subjectReferences = new int[1];
		desiredValues = new float[1];
	}

	/// <summary>
	/// Evaluates the satisfacton of this property, given a set of subjects
	/// </summary>
	/// <param name="subjectsList">Subjects list.</param>
	public float Evaluate(SubjectEvaluator[] subjectsList){
		satisfaction = evaluate(subjectsList);
		return satisfaction;
	}

	/// <summary>
	/// This method is implemented by each property.
	/// It is no abstract but virtual for serialisation issues.
	/// </summary>
	/// <param name="subjectsList">Subjects list.</param>
	protected virtual float evaluate(SubjectEvaluator[] subjectsList){
		return 1;
	}

	/// <summary>
	/// Gets or sets the index of the main subject, which this property is referring to.
	/// </summary>
	/// <value>The subject.</value>
	public int MainSubjectIndex {
		get {return subjectReferences[0];}
	}
}

