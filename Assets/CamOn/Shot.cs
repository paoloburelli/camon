using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


/// <summary>
/// A shot describes the way the camera should frame a scene.
/// A shot is composed by a number of properties and actors.
/// Shots can be created and edited in the unity editor an ia code.
/// Shots created in the editor should be stored in the "Resources" folder so they can be loaded in the game.
/// </summary>
[System.Serializable]
public class Shot : ScriptableObject
{
	/// <summary>
	/// Defines whether the camera should moove or not along the X axis.
	/// If true, the camera should not move.
	/// </summary>
	public bool LockX = false;
	/// <summary>
	/// Defines whether the camera should moove or not along the Y axis.
	/// If true, the camera should not move.
	/// </summary>
	public bool LockY = false;
	/// <summary>
	/// Defines whether the camera should moove or not along the X axis.
	/// If true, the camera should not move.
	/// </summary>
	public bool LockZ=false;

	/// <summary>
	/// Offset of each subject evaluator.
	/// </summary>
	public List<Vector3> VolumesOfInterestPosition = new List<Vector3> ();

	/// <summary>
	/// Scale modifier of each subject evaluator.
	/// </summary>
	public List<Vector3> VolumesOfInterestSize = new List<Vector3> ();

	/// <summary>
	/// Properties composing the shot.
	/// </summary>
	public List<Property> Properties = new List<Property> ();

	[SerializeField]
	int numberOfActors = 0;

	/// <summary>
	/// Gets or sets the number of subjects in the shot.
	/// </summary>
	/// <value>The number of subjects.</value>
	public int NumberOfActors {
		get {
			return numberOfActors;
		}
		set {
			numberOfActors = Mathf.Max(0,value);
			while (VolumesOfInterestPosition.Count < value) {
				VolumesOfInterestPosition.Add (Vector3.zero);
				VolumesOfInterestSize.Add (Vector3.one);
			}
			while (VolumesOfInterestPosition.Count > value) {
				VolumesOfInterestPosition.RemoveAt (VolumesOfInterestPosition.Count - 1);
				VolumesOfInterestSize.RemoveAt (VolumesOfInterestSize.Count - 1);
			}
		}
	}

	/// <summary>
	/// Fixs the properties type afte deserialisation;
	/// </summary>
	public void FixPropertiesType ()
	{
		for (int i = 0; i < Properties.Count; i++) {
			Property p = Properties [i];
			Property.FixType (ref p);
			Properties [i] = p;
		}
	}

	/// <summary>
	/// Return the quality of a specific camera setting with respect to the curren shot and the given subject evaluators.
	/// The quality is calculated as a wighted sum of the properties level of satisfaction.
	/// Visibility of each subject is also considered in the total quality.
	/// </summary>
	/// <returns>[0,1] The quality value; the higher the better.</returns>
	/// <param name="subjects">A list of subject evaluators.</param>
	/// <param name="camera">The camera to be evaluated. If the camera is not passed as a parameter, no evaluation is performed and the last recorded quality is passed.</param>
	public float GetQuality (Actor[] subjects, Camera camera=null)
	{
		float value = 0;
		float weight = 0;
		
		if (subjects != null){

			if (camera != null)
				Actor.ReevaluateAll (subjects, camera);
			
			bool eval = true;
			foreach (Actor s in subjects)
				if (s == null)
					eval = false;
			
			if (eval) {
				foreach (Property p in Properties) {
					value += p.Evaluate (subjects) * p.Weight;
					weight += p.Weight;
				}
				for (int i=0;i<subjects.Length;i++){
	
					float f = (1-Mathf.Pow(1-subjects[i].Visibility,4));
					float w =  Mathf.Lerp(PropertiesCount(i),1,f);

					value += f * w;
					weight += w;
				}
			}
		}
		
		return float.IsNaN (value / weight) ? 0 : value / weight;
	}

	/// <summary>
	/// Return the quality of a specific camera setting with respect to the specified properties and the given subject evaluators
	/// </summary>
	/// <returns>[0,1] The quality value; the higher the better.</returns>
	/// <param name="pTypes">A list of properties.</param>
	/// <param name="actors">A list of subject evaluators.</param>
	/// <param name="camera">The camera to be evaluated. If the camera is not passed as a parameter, no evaluation is performed and the last recorded quality is passed.</param>
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

	/// <summary>
	/// Returns the level of visibility of the subject given a specific camera camera.
	/// Visibility is intended as the fraction of the subjects which is included in the frame ans is not occluded by any obstacle.
	/// </summary>
	/// <param name="subjects">List of subjects.</param>
	/// <param name="camera">The camera to be used for evaluation. If the camera is not passed as a parameter, no evaluation is performed and the last recorded visibility is passed.</param>
	public float Visibility(Actor[] subjects, Camera camera=null) {

		if (camera != null)
			Actor.ReevaluateAll (subjects, camera);

			float visbility = 0;
			foreach (Actor s in subjects)
				visbility += s.Visibility / subjects.Length;
			return visbility;
	}

	/// <summary>
	/// Returns the fraction of the subjects that are included in the view frustum given a specific camera camera.
	/// </summary>
	/// <param name="subjects">List of subjects.</param>
	/// <param name="camera">The camera to be used for evaluation. If the camera is not passed as a parameter, no evaluation is performed and the last recorded fraction is passed.</param>
	public float InFrustum(Actor[] actors, Camera camera=null) {

		if (camera != null)
			Actor.ReevaluateAll (actors, camera);

			float visbility = 0;
			foreach (Actor s in actors)
				visbility += s.InFrustum / actors.Length;
			return visbility;
	}

	/// <summary>
	/// Returns a property defined on a spcific subject
	/// </summary>
	/// <returns>The property.</returns>
	/// <param name="subjectIndex">Index of the subject; the default value is 0.</param>
	/// <typeparam name="T">The type of the property.</typeparam>
	public T GetProperty<T>(int subjectIndex=0) where T : Property{
		foreach (Property p in Properties)
			if (p is T && p.MainSubjectIndex == subjectIndex)
				return (T)p;
		return null;
	}

	/// <summary>
	/// Returns the number of properties set on a specific subject
	/// </summary>
	/// <returns>The count.</returns>
	/// <param name="subjectIndex">Index of the subject</param>
	private int PropertiesCount(int subjectIndex) {
		int n = 0;
		foreach (Property p in Properties)
			if (p.MainSubjectIndex == subjectIndex)
				n++;
		return n;
	}
}
