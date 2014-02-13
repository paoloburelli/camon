using UnityEngine;
using System.Collections;

public class ViewObjectiveFunction : MonoBehaviour {

	public int NumberOfSamples = 10;
	public Bounds bounds;
	
	float[][][] functionValues;
	Vector3 index;

	Cameraman cameraman;

	// Use this for initialization
	void Start () {
		functionValues = new float[NumberOfSamples][][];

		for (int i = 0; i<NumberOfSamples; i++){
			functionValues[i] = new float[NumberOfSamples][];

			for (int j = 0; j<NumberOfSamples; j++){
				functionValues[i][j] = new float[NumberOfSamples];

				for (int k = 0; k<NumberOfSamples; k++){
					functionValues[i][j][k] = 0;
				}
			}
		}
		cameraman = GetComponent<Cameraman>();
		cameraman.enabled = false;
		index = Vector3.zero;
	}


	void Update() {
		Vector3 step = (bounds.max - bounds.min)/NumberOfSamples;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < 17){

			cameraman.EvaluationCamera.position = bounds.min + Vector3.Scale(step,index);
			cameraman.EvaluationCamera.LookAt(bounds.center);
			cameraman.Shot.UpdateSubjects(cameraman.Subjects,cameraman.EvaluationCamera.camera);


			functionValues[(int)index.x][(int)index.y][(int)index.z] = cameraman.Shot.Evaluate();

			index.x += 1;

			if (index.x == NumberOfSamples){
				index.x = 0;
				index.y += 1;
			}

			if (index.y == NumberOfSamples){
				index.y = 0;
				index.z += 1;
			}

			if (index.z == NumberOfSamples){
				index.z = 0;
			}
		}
	}

	void OnDrawGizmos(){
		if (Application.isPlaying) {
		Vector3 step = (bounds.max - bounds.min)/NumberOfSamples;


		for (int i = 0; i<Mathf.Pow(NumberOfSamples,3); i++){

			int n2 = NumberOfSamples*NumberOfSamples;

			int x = i/n2;
			int y = (i%n2)/NumberOfSamples;
			int z = (i%n2)%NumberOfSamples;

				if (functionValues[x][y][z]>0.5) {
			Gizmos.color = new Color(Mathf.Pow(functionValues[x][y][z],13),0,0,Mathf.Pow(functionValues[x][y][z],13));
			Gizmos.DrawCube(bounds.min + Vector3.Scale(step,new Vector3(x,y,z)),step*0.9f);
				}
		}
		}
	}

}
