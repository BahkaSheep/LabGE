using UnityEngine;
using TMPro;

public class Individual : MonoBehaviour
{
	[Header("Genes")]
	public float speed = 2f;

	public float size = 1f;
	public float visionRange = 1f;
	public float fleeDistance = 0.5f;
	public float wanderTime = 4f;
	public float mutationStrength = 0.1f;

	[Header("Energy")]
	public float energy = 2f;

	public float energyThreshold = 1.5f; // Minimum energy required to reproduce
	public float energyStopMatingThreshold = 1f; // Stop mating when below this threshold

	[Header("Energy Consumption")]
	public float baseEnergyUsage = 0.1f;

	public float sizeMultiplier = 0.3f;
	public float visionMultiplier = 0.02f;
	public float speedMultiplier = 0.1f;

	[Header("Behavior")]
	public Vector3 wanderDirection;

	public float wanderTimer;
	public float worldScale = 10f;

	[Header("FoodPrefab")]
	public GameObject foodPrefab;

	[Header("Material")]
	public Material agentMaterial;

	public GameObject spawnEffectPrefab;

	private bool isLookingForMate = false;
	private bool isHunting = false;
	private GameObject currentTarget = null;

	[Header("UI")]
	public TMP_Text generationText;

	public int generation = 1;

	private void Start()
	{
		FindObjectOfType<PopulationManager>().AddIndividual(this);
		wanderTimer = wanderTime;
		transform.localScale = Vector3.one * size; // Adjust size visually
		Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);

		agentMaterial = new Material(agentMaterial);
		GetComponent<Renderer>().material = agentMaterial;

		UpdateGenerationText();
	}

	private void OnDestroy()
	{
		// Unregister this individual when it's destroyed
		FindObjectOfType<PopulationManager>().RemoveIndividual(this);
	}

	private void Update()
	{
		UpdateMatingState();

		GameObject closestPredator = FindClosestPredator();
		GameObject closestPrey = FindClosestPrey();
		isHunting = false;
		// Action priority: flee  > mate > hunt > food > wander
		if (closestPredator != null)
		{
			FleeFromPredator(closestPredator.transform.position);
			ClearCurrentTarget();
		}
		else if (closestPrey != null && energy > energyThreshold * 2)
		{
			HuntPrey(closestPrey);
			isHunting = true;
		}
		else if (isLookingForMate)
		{
			HandleMating();
		}
		else
		{
			HandleFoodSearch();
		}

		KeepWithinBounds();
		ConsumeEnergy();
		UpdateColor();
	}

	private void UpdateMatingState()
	{
		if (!isLookingForMate && energy >= energyThreshold)
		{
			isLookingForMate = true; // Start looking for a mate
		}
		else if (isLookingForMate && energy < energyStopMatingThreshold)
		{
			isLookingForMate = false; // Stop looking for a mate
		}
	}

	private void UpdateGenerationText()
	{
		if (generationText != null)
		{
			generationText.text = $"{generation}";
		}
	}

	private void HuntPrey(GameObject prey)
	{
		currentTarget = prey;

		if (prey != null)
		{
			MoveToTarget(prey.transform.position, speed * 1.2f);

			// Check if prey is close enough
			float distance = Vector3.Distance(transform.position, prey.transform.position);
			if (distance < 0.5f)
			{
				ConsumePrey(prey.GetComponent<Individual>());
				ClearCurrentTarget();
			}
		}
	}

	private void ConsumePrey(Individual prey)
	{
		energy += prey.energy; //I might regret this part
		energy += prey.size * 2;
		Destroy(prey.gameObject); // Remove the prey
	}

	private void HandleMating()
	{
		if (currentTarget == null || !currentTarget.CompareTag("Individual"))
		{
			currentTarget = FindClosestMate();
		}

		if (currentTarget != null)
		{
			MoveToTarget(currentTarget.transform.position, speed);

			// Check if mate is close enough
			float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
			if (distance < 0.5f)
			{
				Reproduce(currentTarget.GetComponent<Individual>());
				ClearCurrentTarget();
			}
		}
		else
		{
			HandleFoodSearch(); // No mate found
		}
	}

	private void HandleFoodSearch()
	{
		if (currentTarget == null || !currentTarget.CompareTag("Food"))
		{
			currentTarget = FindClosestFood();
		}

		if (currentTarget != null)
		{
			if (energy > 5)
				MaintainDistanceFromOthers();
			MoveToTarget(currentTarget.transform.position, speed);
			// Check if food is close enough
			float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
			if (distance < 0.5f)
			{
				energy += 1f; // Gain energy
				Destroy(currentTarget);
				ClearCurrentTarget();
			}
		}
		else
		{
			WanderAround(); // No food found, wander
		}
	}

	private void ClearCurrentTarget()
	{
		currentTarget = null;
	}

	private void MoveToTarget(Vector3 targetPosition, float speed)
	{
		Vector3 direction = (targetPosition - transform.position).normalized;
		transform.position += direction * speed * Time.deltaTime;
	}

	private void FleeFromPredator(Vector3 predatorPosition)
	{
		Vector3 fleeDirection = (transform.position - predatorPosition).normalized;
		transform.position += fleeDirection * speed * 1.2f * Time.deltaTime;
	}

	private void WanderAround()
	{
		transform.position += wanderDirection * speed * Time.deltaTime;

		wanderTimer -= Time.deltaTime;
		if (wanderTimer <= 0f)
		{
			ChooseNewWanderDirection();
			wanderTimer = wanderTime;
		}
	}

	private GameObject FindClosestFood()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange, LayerMask.GetMask("Food"));
		GameObject closestFood = null;
		float closestDistance = Mathf.Infinity;

		foreach (Collider collider in colliders)
		{
			float distance = Vector3.Distance(transform.position, collider.transform.position);
			if (distance < closestDistance)
			{
				closestFood = collider.gameObject;
				closestDistance = distance;
			}
		}
		return closestFood;
	}

	private GameObject FindClosestMate()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange * 2, LayerMask.GetMask("Individual"));
		GameObject closestMate = null;
		float closestDistance = Mathf.Infinity;

		foreach (Collider collider in colliders)
		{
			if (collider.gameObject != this.gameObject)
			{
				Individual otherIndividual = collider.GetComponent<Individual>();
				if (otherIndividual != null && Mathf.Abs(otherIndividual.size - size) < 0.5f && otherIndividual.energy >= energyThreshold)
				{
					float distance = Vector3.Distance(transform.position, collider.transform.position);
					if (distance < closestDistance)
					{
						closestMate = collider.gameObject;
						closestDistance = distance;
					}
				}
			}
		}
		return closestMate;
	}

	private GameObject FindClosestPrey()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange, LayerMask.GetMask("Individual"));
		GameObject closestPrey = null;
		float closestDistance = Mathf.Infinity;

		foreach (Collider collider in colliders)
		{
			if (collider.gameObject != this.gameObject)
			{
				Individual otherIndividual = collider.GetComponent<Individual>();
				if (otherIndividual != null && otherIndividual.size <= size / 1.5f)
				{
					float distance = Vector3.Distance(transform.position, collider.transform.position);
					if (distance < closestDistance)
					{
						closestPrey = collider.gameObject;
						closestDistance = distance;
					}
				}
			}
		}
		return closestPrey;
	}

	private GameObject FindClosestPredator()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange / 3, LayerMask.GetMask("Individual"));
		GameObject closestPredator = null;
		float closestDistance = Mathf.Infinity;

		foreach (Collider collider in colliders)
		{
			Individual otherIndividual = collider.GetComponent<Individual>();
			if (otherIndividual != null && otherIndividual.size > size * 1.5f)
			{
				float distance = Vector3.Distance(transform.position, collider.transform.position);
				if (distance < closestDistance)
				{
					closestPredator = collider.gameObject;
					closestDistance = distance;
				}
			}
		}
		return closestPredator;
	}

	private void MaintainDistanceFromOthers()
	{
		Collider[] nearbyIndividuals = Physics.OverlapSphere(transform.position, 0.1f, LayerMask.GetMask("Individual")); // Adjust range as needed
		Vector3 separationForce = Vector3.zero;

		foreach (Collider collider in nearbyIndividuals)
		{
			if (collider.gameObject != this.gameObject)
			{
				Vector3 awayDirection = (transform.position - collider.transform.position).normalized;
				separationForce += awayDirection;
			}
		}

		// Apply the separation force
		if (separationForce != Vector3.zero)
		{
			transform.position += separationForce.normalized * speed * Time.deltaTime;
		}
	}

	private void ConsumeEnergy()
	{
		float energyUsage = baseEnergyUsage + (visionRange * visionMultiplier) + (speed * speedMultiplier) + (size * sizeMultiplier);
		energy -= energyUsage * Time.deltaTime;

		if (energy <= 0)
		{
			SpawnFood();
			Destroy(gameObject); // kaboom
		}
	}

	private void ChooseNewWanderDirection()
	{
		float randomAngle = Random.Range(0f, 360f);
		wanderDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)).normalized;
	}

	private void KeepWithinBounds()
	{
		Vector3 pos = transform.position;

		if (pos.x < -worldScale || pos.x > worldScale)
		{
			wanderDirection.x = -wanderDirection.x;
			pos.x = Mathf.Clamp(pos.x, -worldScale, worldScale);
		}
		if (pos.z < -worldScale || pos.z > worldScale)
		{
			wanderDirection.z = -wanderDirection.z;
			pos.z = Mathf.Clamp(pos.z, -worldScale, worldScale);
		}

		transform.position = pos;
	}

	private void SpawnFood()
	{
		int foodAmount = Mathf.CeilToInt(size * 2);
		for (int i = 0; i < foodAmount; i++)
		{
			Vector3 foodPosition = transform.position + new Vector3(
				Random.Range(-0.5f, 0.5f),
				0,
				Random.Range(-0.5f, 0.5f));
			Instantiate(foodPrefab, foodPosition, Quaternion.identity);
		}
	}

	private void UpdateColor()
	{
		if (agentMaterial != null)
		{
			// Define colors for low and high energy
			Color lowEnergyColor = Color.red;
			Color highEnergyColor = Color.green;
			Color lookingForMateColor = Color.blue;
			Color huntingColor = Color.magenta;

			// Normalize energy to a 0-1 range
			float normalizedEnergy = Mathf.Clamp01(energy / energyThreshold);

			// Interpolate between lowEnergyColor and highEnergyColor
			Color currentColor = Color.Lerp(lowEnergyColor, highEnergyColor, normalizedEnergy);
			if (isHunting)
			{
				agentMaterial.color = huntingColor;
			}
			else if (isLookingForMate)
			{
				agentMaterial.color = lookingForMateColor;
			}
			else
				agentMaterial.color = currentColor;
		}
	}

	private void Reproduce(Individual mate)
	{
		Individual child = Instantiate(gameObject).GetComponent<Individual>();
		child.speed = Mathf.Clamp(speed + Random.Range(-mutationStrength, mutationStrength), 0.1f, 60f);
		child.size = Mathf.Clamp(size + Random.Range(-mutationStrength, mutationStrength), 0.5f, 6f);
		child.visionRange = Mathf.Clamp(visionRange + Random.Range(-mutationStrength, mutationStrength), 1f, 60f);
		child.energy = energy / 2; // Split energy between parent and child
		energy /= 2;

		child.generation = Mathf.Max(generation, mate.generation) + 1; // Set the child's generation
		child.UpdateGenerationText(); // Update text for the child

		Instantiate(spawnEffectPrefab, child.transform.position, Quaternion.identity);
	}
}