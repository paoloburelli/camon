using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[System.Serializable]
public class Shot : ScriptableObject
{
	public bool LockX=false,LockY=false,LockZ=false;
	public List<Vector3> SubjectCenters = new List<Vector3> ();
	public List<Vector3> SubjectScales = new List<Vector3> ();
	public List<PrimitiveType> SubjectBounds = new List<PrimitiveType> ();
	public List<Property> Properties = new List<Property> ();
	[SerializeField]
	int numberOfActors = 0;

	public int NumberOfActors {
		get {
			return numberOfActors;
		}
		set {
			numberOfActors = value;
			while (SubjectBounds.Count < value) {
				SubjectCenters.Add (Vector3.zero);
				SubjectScales.Add (Vector3.one);
				SubjectBounds.Add (PrimitiveType.Capsule);
			}
			while (SubjectBounds.Count > value) {
				SubjectBounds.RemoveAt (SubjectBounds.Count - 1);
				SubjectCenters.RemoveAt (SubjectCenters.Count - 1);
				SubjectScales.RemoveAt (SubjectScales.Count - 1);
			}
		}
	}

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
		
	public float GetQuality (Actor[] actors, Camera camera=null)
	{
		float value = 0;
		float weight = 0;
		
		if (actors != null){

			if (camera != null)
				Actor.ReevaluateAll (actors, camera);
			
			bool eval = true;
			foreach (Actor s in actors)
				if (s == null)
					eval = false;
			
			if (eval) {
				foreach (Property p in Properties) {
					value += p.Evaluate (actors) * p.Weight;
					weight += p.Weight;
				}
				for (int i=0;i<actors.Length;i++){
	
					float f = (1-Mathf.Pow(1-actors[i].Visibility,4));
					float w =  Mathf.Lerp(PropertiesCount(i),1,f);

					value += f * w;
					weight += w;
				}
			}
		}
		
		return float.IsNaN (value / weight) ? 0 : value / weight;
	}
	
	public float GetQuality (Property.Type[] pTypes, Actor[] actors, Camera camera=null)
	{
		float value = 0;
		float weight = 0;

		if (camera != null)
			Actor.ReevaluateAll (actors, camera);
		
		if (actors != null)
			foreach (Property.Type pt in pTypes)
				foreach (Property p in Properties)
					if (p.PropertyType == pt) {
						value += p.Evaluate (actors) * p.Weight;
						weight += p.Weight;
					}
		
		return float.IsNaN (value / weight) ? 1 : value / weight;
	}
	
	public float Visibility(Actor[] actors) {
			float visbility = 0;
			foreach (Actor s in actors)
				visbility += s.Visibility / actors.Length;
			return visbility;
	}

	public float InFrustum(Actor[] actors) {
			float visbility = 0;
			foreach (Actor s in actors)
				visbility += s.InFrustum / actors.Length;
			return visbility;
	}

	public T GetProperty<T>(int actorIndex=0) where T : Property{
		foreach (Property p in Properties)
			if (p is T && p.Subject == actorIndex)
				return (T)p;
		return null;
	}

	private int PropertiesCount(int actorIndex) {
		int n = 0;
		foreach (Property p in Properties)
			if (p.Subject == actorIndex)
				n++;
		return n;
	}
}
