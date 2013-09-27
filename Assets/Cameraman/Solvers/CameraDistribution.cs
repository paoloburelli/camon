using UnityEngine;
using System.Collections.Generic;

public class CameraDistribution
{
	private static Pair<Vector3> rval = new Pair<Vector3>(Vector3.zero,Vector3.zero);
	
	public static Pair<Vector3> GetRandomDirection(Subject[] subjects, List<Property> properties, Transform camera){
		
		rval.a = Random.onUnitSphere;
		rval.b = Random.onUnitSphere;
		float bestProbability = PDF (subjects,properties,camera,rval.a,rval.b);
		
		for (int i=0;i<10;i++){
			Vector3 pd = Random.onUnitSphere;
			Vector3 ld = Random.onUnitSphere;
			float prob = PDF (subjects,properties,camera,pd,ld);
			
			if (prob > bestProbability){
				bestProbability = prob;
				rval.a = pd;
				rval.b = ld;
			}
		}
			
		return rval;
	}
	
	public static float PDF(Subject[] subjects, List<Property> properties, Transform camera, Vector3 positionDirection, Vector3 orientationDirection){
		return (ProjectionSizePDF(subjects, properties, camera, positionDirection)+VantageAnglePDF(subjects, properties, camera, positionDirection))/2;
	}
	
	public static float ProjectionSizePDF(Subject[] subjects, List<Property> properties, Transform camera, Vector3 positionDirection) {
		Vector3 ideadlDirection = Vector3.zero;
		foreach (Property p in properties){
			if (p.Type == Property.PropertyType.ProjectionSize) {
				
				if (subjects[p.Subject].ProjectionSize > p.DesiredValue)
					ideadlDirection += (camera.position-subjects[p.Subject].Position).normalized;
				
				if (subjects[p.Subject].ProjectionSize < p.DesiredValue)
					ideadlDirection += -(camera.position-subjects[p.Subject].Position).normalized;
			}
		}
		ideadlDirection.Normalize();
		return Mathf.Clamp01(Vector3.Dot(positionDirection,ideadlDirection));
	}
	
	public static float VantageAnglePDF(Subject[] subjects, List<Property> properties, Transform camera, Vector3 positionDirection) {
		Vector3 idealPosition = Vector3.zero;
		int angleCount = 0;
		
		foreach (Property p in properties){
			if (p.Type == Property.PropertyType.VantageAngle) {
				float distance = (subjects[p.Subject].Position-camera.position).magnitude;
				idealPosition += distance*(Quaternion.Euler(((VantageAngle)p).DesiredVerticalAngle,((VantageAngle)p).DesiredHorizontalAngle,0)*subjects[p.Subject].Forward);
				angleCount++;
			}
		}
		idealPosition *= 1.0f/angleCount;
		Vector3 idealDirection = (idealPosition-camera.position).normalized;
			
		return Mathf.Clamp01(Vector3.Dot(positionDirection,idealDirection));
	}

	public static float VisibilityPDF(Subject[] subjects, List<Property> properties, Transform camera, Vector3 positionDirection, Vector3 orientationDirection) {
		return 1;
	}
	
	public static float PositionOnScreenPDF(Subject[] subjects, List<Property> properties, Transform camera, Vector3 orientationDirection) {
		return 1;
	}	

}

