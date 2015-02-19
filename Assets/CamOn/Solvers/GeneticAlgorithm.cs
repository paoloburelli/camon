using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Individual
{
	Vector3 position,lookAtPosition;
	float fitness = 0;
	
	public float Fitness {
		get {
			return fitness;	
		}
	}
	
	public Vector3 Position {
		get {
			return position;
		}
	}
	
	public Vector3 LookAt {
		get {
			return lookAtPosition;
		}
	}
	
	public Individual (Vector3 position, Vector3 lookAt)
	{
		this.position = position;
		this.lookAtPosition = lookAt;
	}

	public Individual (Individual p) {
		this.position = p.position;
		this.lookAtPosition = p.lookAtPosition;
		this.fitness = p.fitness;
	}

	public float Evaluate (Camera camera, Shot shot, Subject[] subjects)
	{
		camera.transform.position = position;
		camera.transform.LookAt(lookAtPosition);
		shot.UpdateSubjects (subjects, camera);
		fitness = shot.Evaluate ();
		return fitness;
	}

	public Individual Mutate(float mutationProbability) {
		if (Random.value < mutationProbability) {
			position = position+Random.insideUnitSphere;
			lookAtPosition = lookAtPosition+Random.insideUnitSphere;
		}
		return this;
	}

	public static Individual Crossover(Individual a, Individual b, float crossoverProbability){
		if (Random.value < crossoverProbability)
			return new Individual(0.8f*a.position + 0.2f*b.position,0.8f*a.lookAtPosition + 0.2f*b.lookAtPosition);
		return a;
	}
}

public class GeneticAlgorithm : Solver
{
	public float crossoverProbability;
	public float mutationProbability;
	public int populationSize;
	public float selection;
	
	Individual globalOptimum;
	List<Individual> individuals;
	IEnumerator<Individual> enumerator=null;

	public GeneticAlgorithm(float selection, float mutationProbability, float crossoverProbability, int popSize){
		this.mutationProbability = mutationProbability;
		this.crossoverProbability = crossoverProbability;
		this.populationSize = popSize;
		this.selection = selection;
	}
	
	protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		globalOptimum.Evaluate(currentCamera.camera,shot,subjects);

		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
			if (!enumerator.MoveNext ()) {
				newGeneration(subjects);
				enumerator = individuals.GetEnumerator ();
				enumerator.MoveNext ();
			}
		
			enumerator.Current.Evaluate (currentCamera.camera,shot,subjects);

			if (enumerator.Current.Fitness > globalOptimum.Fitness) {
				globalOptimum = new Individual(enumerator.Current);
			}

			logTrace (currentCamera.position, currentCamera.forward, enumerator.Current.Fitness);
		}
		
		currentCamera.position = globalOptimum.Position;
		currentCamera.LookAt(globalOptimum.LookAt);

		return globalOptimum.Fitness;
	}

	protected void newGeneration(Subject[] subjects){
		individuals.Sort((x,y) => {return (int)((y.Fitness - x.Fitness)*10);});

		for (int i=0; i<selection*individuals.Count;i+=2){
			Individual a = individuals[i];
			Individual b = individuals[i+1];

			individuals[i] = Individual.Crossover(a,b,crossoverProbability).Mutate(mutationProbability);
			individuals[i+1] = Individual.Crossover(b,a,crossoverProbability).Mutate(mutationProbability);
		} 

		Vector3 c = SubjectsCenter(subjects);
		float r = SubjectsRadius(subjects);
		for (int i=(int)(selection*individuals.Count); i<individuals.Count;i++)
			individuals[i] = new Individual(c+r*Random.insideUnitSphere,c+Random.insideUnitSphere);
	}


	public override void Start (Transform camera, Subject[] subjects)
	{
		base.Start (camera, subjects);
		if (camera == null)
			throw new MissingReferenceException ("camera not initilised");

		individuals = new List<Individual> ();
		individuals.Add(new Individual(camera.position,camera.forward));

		Vector3 c = SubjectsCenter(subjects);
		float r = SubjectsRadius(subjects);

		for (int i=0;i<populationSize-1;i++){
			individuals.Add(new Individual(c+r*Random.insideUnitSphere,c+Random.insideUnitSphere));
		}

		enumerator = individuals.GetEnumerator ();
		globalOptimum = new Individual(individuals[0]);
	}

}