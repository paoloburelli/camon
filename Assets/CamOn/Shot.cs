using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class Shot : ScriptableObject{
	public int NumberOfSubjects = 0;
	public List<Vector3> SubjectCenters = new List<Vector3>();
	public List<Vector3> SubjectScales = new List<Vector3>();
	public List<PrimitiveType> SubjectBounds = new List<PrimitiveType>();
	public List<Property> Properties = new List<Property>();
	
	[MenuItem("Assets/Create/Shot")]
	public static void Crate(){
        Shot asset = new Shot();  //scriptable object 
        AssetDatabase.CreateAsset(asset, "Assets/Shot.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
	}
	
	public void FixPropertyTypes(){
		for (int i=0;i<Properties.Count;i++)
			switch(Properties[i].Type){
			case Property.PropertyType.Visibility:
				if (!(Properties[i] is Visibility))
					Properties[i] = new Visibility(Properties[i]);
				break;
			case Property.PropertyType.ProjectionSize:
				if (!(Properties[i] is ProjectionSize))
					Properties[i] = new ProjectionSize(Properties[i]);
				break;
			case Property.PropertyType.PositionOnScreen:
				if (!(Properties[i] is PositionOnScreen))
					Properties[i] = new PositionOnScreen(Properties[i]);
				break;
			case Property.PropertyType.VantageAngle:
				if (!(Properties[i] is VantageAngle))
					Properties[i] = new VantageAngle(Properties[i]);
				break;
			}
		
	}
		
	private Subject[] subjects;
	public void UpdateSubjects(Subject[] subjects,Camera camera){
		this. subjects = subjects;
		if (subjects != null && camera != null)
			foreach (Subject s in subjects)
				if (s != null)
					s.Update(camera);
	}
	public float Evaluate(){
		float value = 0;
		float weight = 0;
		
		if (subjects != null)
			foreach (Property p in Properties){
					value += p.Evaluate(subjects)*p.Weight;
					weight+= p.Weight;
			}
		
		return float.IsNaN(value/weight) ? 0:value/weight;
	}
	
	public float Evaluate(Property.PropertyType[] pTypes){
		float value = 0;
		float weight = 0;
		
		if (subjects != null)
			foreach (Property.PropertyType pt in pTypes)
				foreach (Property p in Properties)
					if (p.Type == pt){
						value += p.Evaluate(subjects)*p.Weight;
						weight+=p.Weight;
					}
		
		return float.IsNaN(value/weight) ? 0:value/weight;
	}
}
