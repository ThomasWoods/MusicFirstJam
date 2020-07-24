using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
	List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();
	public List<FrequencyGeneratorV2> playerGenerators = new List<FrequencyGeneratorV2>();
	public OscillioscopeCheat oscillioscope;
	public FrquencyCombiner targetCombiner, playerCombiner;
	public float FreqTolerace = 1.0f;
	public float ClearFailTime = 2.0f;

	public UnityEvent MatchSuccess = new UnityEvent();
	public FloatEvent MatchFail = new FloatEvent();
	public StringEvent MatchesTextUpdate = new StringEvent();
	public UnityEvent ClearFail = new UnityEvent();
	public UnityEvent ClearAll = new UnityEvent();

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
		playerGenerators.AddRange(playerCombiner.generators);


		MatchFail.AddListener(UpdatesFailMessage);

		yield return null;
		Randomize();
	}

	public void CheckWinCondition(float f) { CheckWinCondition(); }
	public void CheckWinCondition()
	{
		List<FrequencyGeneratorV2> matched = new List<FrequencyGeneratorV2>();
		foreach (FrequencyGeneratorV2 g in generators)
		{
			foreach (FrequencyGeneratorV2 pg in playerGenerators)
			{
				if (!matched.Contains(g) && g.waveType == pg.waveType && withinTolerace(g.Freq, pg.Freq, FreqTolerace))
				{
					matched.Add(g);
				}
			}
		}
		if (matched.Count == generators.Count) MatchSuccess.Invoke();
		else
		{
			MatchFail.Invoke(matched.Count);
			StartCoroutine(WaitAndClear());
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
		ClearAll.Invoke();
	}
	bool withinTolerace(float a, float b, float tolerance)
	{
		if (Mathf.Abs(a-b)<Mathf.Abs(tolerance))return true ;
		return false;
	}

	IEnumerator WaitAndClear()
	{
		yield return new WaitForSeconds(ClearFailTime);
		ClearFail.Invoke();
	}

	void UpdatesFailMessage(float f)
	{
		MatchesTextUpdate.Invoke(f + " Matches!");
	}
}
