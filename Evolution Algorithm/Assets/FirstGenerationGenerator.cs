using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstGenerationGenerator : MonoBehaviour
{
	[Header("Individual Settings")]
	public GameObject individualPrefab; // Prefab for the individual

	public int initialPopulation = 30; // Number of individuals to spawn in the first generation
	public Vector2 worldBounds = new Vector2(10f, 5f); // Size of the world

	[Header("Attribute Ranges")]
	public Vector2 speedRange = new Vector2(0.5f, 5f); // Random speed range

	public Vector2 sizeRange = new Vector2(0.1f, 0.2f);
	public Vector2 visionRangeRange = new Vector2(1f, 10f); // Random vision range
	public Vector2 wanderTimeRange = new Vector2(0.5f, 5f);

	private void Start()
	{
		SpawnInitialPopulation();
	}

	private void SpawnInitialPopulation()
	{
		for (int i = 0; i < initialPopulation; i++)
		{
			// Generate random position within the world bounds
			Vector3 randomPosition = new Vector3(
				Random.Range(-worldBounds.x, worldBounds.x),
				0,
				Random.Range(-worldBounds.y, worldBounds.y)
			);

			// Instantiate a new individual at the random position
			GameObject individual = Instantiate(individualPrefab, randomPosition, Quaternion.identity);

			// Assign random attributes to the individual
			Individual individualScript = individual.GetComponent<Individual>();
			if (individualScript != null)
			{
				individualScript.speed = Random.Range(speedRange.x, speedRange.y);
				individualScript.visionRange = Random.Range(visionRangeRange.x, visionRangeRange.y);
				individualScript.size = Random.Range(sizeRange.x, sizeRange.y);
				individualScript.wanderTime = Random.Range(wanderTimeRange.x, wanderTimeRange.y);
			}
		}
	}
}