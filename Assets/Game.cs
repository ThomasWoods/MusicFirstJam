using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();
	public List<FrequencyGeneratorV2> userGenerators = new List<FrequencyGeneratorV2>();
	public OscillioscopeCheat oscillioscope;
	public FrquencyCombiner targetCombiner, playerCombiner;

	IEnumerator Start()
	{
		generators.Add(gameObject.AddComponent<FrequencyGeneratorV2>());
		generators.Add(gameObject.AddComponent<FrequencyGeneratorV2>());

		foreach (FrequencyGeneratorV2 g in generators)
		{
			oscillioscope.AddGenerator(g);
			targetCombiner.generators.Add(g);
		}
		targetCombiner.GetComponent<AudioSource>().mute = true;

		yield return null;
		Randomize();
	}

	public void CheckWinCondition()
	{
		foreach (FrequencyGeneratorV2 g in generators)
		{

		}
	}
	public void ToggleOutput()
	{
		targetCombiner.GetComponent<AudioSource>().mute=!targetCombiner.GetComponent<AudioSource>().mute;
		playerCombiner.GetComponent<AudioSource>().mute=!playerCombiner.GetComponent<AudioSource>().mute;
	}

	public void Randomize()
	{
		foreach (FrequencyGeneratorV2 g in generators)
		{
			g.TargetFreq = Random.Range(50, 401);
			g.SetWaveType(Random.Range(0, 4));
		}
	}
}
