using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
	[Header("Food Settings")]
	public GameObject foodPrefab; // Prefab for the food object

	public Vector2 worldBounds = new Vector2(10f, 10f); // Size of the spawning area
	public float spawnInterval = 5f; // Time in seconds between spawns
	public int maxFoodCount = 50; // Maximum number of food objects allowed at once
	public int initialAmount = 20;
	public int clusterAmount = 2;

	private float spawnTimer;

	private void Start()
	{
		spawnTimer = spawnInterval;
	}

	private void OnEnable()
	{
		for (int i = 0; i <= initialAmount; i++)
		{
			SpawnFood();
		}
	}

	private void Update()
	{
		spawnTimer -= Time.deltaTime;

		if (spawnTimer <= 0f)
		{
			SpawnFood();
			spawnTimer = spawnInterval; // Reset timer
		}
	}

	private void SpawnFood()
	{
		// Check current food count in the scene
		int currentFoodCount = GameObject.FindGameObjectsWithTag("Food").Length;

		if (currentFoodCount < maxFoodCount)
		{
			Vector3 randomPosition = new Vector3(
				Random.Range(-worldBounds.x, worldBounds.x),
				0,
				Random.Range(-worldBounds.y, worldBounds.y)
			);
			for (int i = 0; ++i < clusterAmount; i++)
			{
				Vector3 clusterOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
				Instantiate(foodPrefab, randomPosition + clusterOffset, Quaternion.identity);
			}
		}
	}
}