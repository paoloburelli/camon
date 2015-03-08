using UnityEngine;
using System.Collections;

public class ScriptedCamera : MonoBehaviour {

	public Transform a,b;

	// Use this for initialization
	void Start () {
		CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Cut, new Transform[] {a});
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount == 100)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LongShot"), CameraOperator.Transition.Smooth, new Transform[] {a});

		if (Time.frameCount == 200)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LeftSide"), CameraOperator.Transition.Cut, new Transform[] {a});

		if (Time.frameCount == 300)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Smooth, new Transform[] {a});
	}
}
