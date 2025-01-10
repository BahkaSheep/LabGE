using UnityEngine;
using System.Collections.Generic;

public class PopulationManager : MonoBehaviour
{
	public List<Individual> individuals = new List<Individual>();

	public float averageSpeed;
	public float averageSize;
	public float averageVisionRange;

	public float updateInterval = 1f; // Time interval for updating averages
	private float timer;

	public List<float> speedHistory = new List<float>();
	public List<float> sizeHistory = new List<float>();
	public List<float> visionHistory = new List<float>();

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= updateInterval)
		{
			UpdateAverages();
			timer = 0f;
		}
	}

	private void UpdateAverages()
	{
		if (individuals.Count == 0)
		{
			averageSpeed = 0;
			averageSize = 0;
			averageVisionRange = 0;
			return;
		}

		float totalSpeed = 0;
		float totalSize = 0;
		float totalVisionRange = 0;

		foreach (var individual in individuals)
		{
			totalSpeed += individual.speed;
			totalSize += individual.size;
			totalVisionRange += individual.visionRange;
		}

		averageSpeed = totalSpeed / individuals.Count;
		averageSize = totalSize / individuals.Count;
		averageVisionRange = totalVisionRange / individuals.Count;

		// Save to history for graphing
		speedHistory.Add(averageSpeed);
		sizeHistory.Add(averageSize);
		visionHistory.Add(averageVisionRange);
	}

	public void AddIndividual(Individual newIndividual)
	{
		if (!individuals.Contains(newIndividual))
		{
			individuals.Add(newIndividual);
		}
	}

	public void RemoveIndividual(Individual individual)
	{
		if (individuals.Contains(individual))
		{
			individuals.Remove(individual);
		}
	}
}