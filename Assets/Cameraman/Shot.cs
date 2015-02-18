using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Shot : ScriptableObject
{
	public int NumberOfSubjects = 0;
	public List<Vector3> SubjectCenters = new List<Vector3> ();
	public List<Vector3> SubjectScales = new List<Vector3> ();
	public List<PrimitiveType> SubjectBounds = new List<PrimitiveType> ();
	public List<Property> Properties = new List<Property> ();
		
	public void FixPropertyTypes ()
	{
		for (int i=0; i<Properties.Count; i++)
			switch (Properties [i].PropertyType) {
			case Property.Type.ProjectionSize:
				if (!(Properties [i] is ProjectionSize))
					Properties [i] = new ProjectionSize (Properties [i]);
				break;
			case Property.Type.PositionOnScreen:
				if (!(Properties [i] is PositionOnScreen))
					Properties [i] = new PositionOnScreen (Properties [i]);
				break;
			case Property.Type.VantageAngle:
				if (!(Properties [i] is VantageAngle))
					Properties [i] = new VantageAngle (Properties [i]);
				break;
			case Property.Type.RelativePosition:
				if (!(Properties [i] is RelativePosition))
					Properties [i] = new RelativePosition (Properties [i]);
				break;
			}
		
	}
		
	private Subject[] subjects;

	public void UpdateSubjects (Subject[] subjects, Camera camera)
	{
		this. subjects = subjects;
		if (subjects != null && camera != null)
			foreach (Subject s in subjects)
				if (s != null)
					s.Update (camera);
	}
	
	public float Evaluate ()
	{
		float value = 0;
		float weight = 0;
		
		if (subjects != null){
			
			bool eval = true;
			foreach (Subject s in subjects)
				if (s == null)
					eval = false;
			
			if (eval) {
				foreach (Property p in Properties) {
					value += p.Evaluate (subjects) * p.Weight;
					weight += p.Weight;
				}
				foreach (Subject s in subjects){
					value += s.Visibility;
					weight += 1;
				}
			}
		}
		
		return float.IsNaN (value / weight) ? 0 : value / weight;
	}
	
	public float Evaluate (Property.Type[] pTypes)
	{
		float value = 0;
		float weight = 0;
		
		if (subjects != null)
			foreach (Property.Type pt in pTypes)
				foreach (Property p in Properties)
					if (p.PropertyType == pt) {
						value += p.Evaluate (subjects) * p.Weight;
						weight += p.Weight;
					}
		
		return float.IsNaN (value / weight) ? 0 : value / weight;
	}
	
	public float Visibility {
		get {
			float visbility = 0;
			foreach (Subject s in subjects)
				visbility += s.Visibility / subjects.Length;
			return visbility;
		}
	}
	
	public float ProjectionSize {
		get {
		float value = 0;
		float weight = 0;
		
		if (subjects != null)
			foreach (Property p in Properties)
				if (p.PropertyType == Property.Type.ProjectionSize) {
					value += p.Evaluate (subjects) * p.Weight;
					weight += p.Weight;
				}
		
			return float.IsNaN (value / weight) ? 0 : value / weight;
		}
	}
}
