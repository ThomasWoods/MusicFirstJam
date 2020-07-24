using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillioscopeCheat : MonoBehaviour
{
	public int resolution = 1024;
	public Transform dataPointsParent = default;
	public GameObject dataPointPrefab = default;

	public List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();

	public float heightMult = 1;

	private void Start()
	{
		if (dataPointsParent == null) return;
		for (int i = 0; i < resolution; i++)
		{
			GameObject o = Instantiate(dataPointPrefab, dataPointsParent);
			o.name = "Data Point " + i.ToString("0000");
			o.SetActive(true);
		}
		foreach (FrequencyGeneratorV2 g in generators)
		{
			g.OnFreqChange.AddListener(updateOscilloscope);
			g.OnOffsetChange.AddListener(updateOscilloscope);
		}
	}

	public void AddGenerator(FrequencyGeneratorV2 g)
	{
		generators.Add(g);
		g.OnFreqChange.AddListener(updateOscilloscope);
		g.OnOffsetChange.AddListener(updateOscilloscope);
	}
	public void RemoveGenerator(FrequencyGeneratorV2 g)
	{
		generators.Remove(g);
		g.OnFreqChange.RemoveListener(updateOscilloscope);
		g.OnOffsetChange.RemoveListener(updateOscilloscope);
	}

	public void updateOscilloscope()
	{
		if (dataPointsParent == null) return;
		float[] samples = new float[resolution];
		for (int n = 0; n < resolution; n++)
		{
			float s = 0;
			foreach (FrequencyGeneratorV2 g in generators)
			{
				if (g == null) continue;
				s += g.getSample(n);
			}
			samples[n] = s / generators.Count;
		}

		Transform[] children = dataPointsParent.GetComponentsInChildren<Transform>(false);
		for (int i = 0; i < children.Length - 1; i++)
		{
			children[i + 1].localPosition = new Vector3(i, samples[i]*heightMult, 0);
		}
	}
	void updateOscilloscope(float f) { updateOscilloscope(); }//to use with FloatEvents
}
