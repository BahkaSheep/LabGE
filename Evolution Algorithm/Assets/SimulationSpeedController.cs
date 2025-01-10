using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSpeedController : MonoBehaviour
{
	[Range(0.1f, 10f)] // Allows you to adjust speed in the inspector or via UI
	public float timeScale = 1f;

	private void Update()
	{
		// Set the time scale to the selected value
		Time.timeScale = timeScale;

		// Optional: Set Time.fixedDeltaTime proportionally to keep physics in sync with the time scale
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
	}
}