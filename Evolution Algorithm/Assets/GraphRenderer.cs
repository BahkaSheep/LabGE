using System.Collections.Generic;
using UnityEngine;

public class GraphRenderer : MonoBehaviour
{
	public LineRenderer speedLine;
	public LineRenderer sizeLine;
	public LineRenderer visionLine;

	public PopulationManager populationManager;
	public float xScale = 1f; // Spacing between points on the x-axis
	public float yScale = 1f; // Scale for y-axis values

	private void Update()
	{
		UpdateGraph(speedLine, populationManager.speedHistory, Color.red);
		UpdateGraph(sizeLine, populationManager.sizeHistory, Color.green);
		UpdateGraph(visionLine, populationManager.visionHistory, Color.blue);
	}

	private void UpdateGraph(LineRenderer line, List<float> data, Color color)
	{
		line.positionCount = data.Count;
		line.startColor = color;
		line.endColor = color;

		for (int i = 0; i < data.Count; i++)
		{
			float x = i * xScale;
			float y = data[i] * yScale;
			line.SetPosition(i, new Vector3(x, y, 0) + transform.localPosition);
		}
	}
}