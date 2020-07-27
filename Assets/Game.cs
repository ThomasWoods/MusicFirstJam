using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

public class Game : MonoBehaviour
{
	List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();
	public List<FrequencyGeneratorV2> playerGenerators = new List<FrequencyGeneratorV2>();
	public OscillioscopeCheat oscillioscope;
	public FrquencyCombiner targetCombiner, playerCombiner;
	public float FreqTolerace = 5.0f, OffsetTolerance = 1;
	public float ClearFailTime = 2.0f;
	float Difficulty_LoFreq = 50, Difficulty_HiFreq = 401;
	int Difficulty_LoOff = 0, Difficulty_HiOff = 0;

	public StringEvent FreqDiffChange = new StringEvent();
	public StringEvent OffDiffChange = new StringEvent();
	public StringEvent updateHint = new StringEvent();

	public UnityEvent MatchSuccess = new UnityEvent();
	public FloatEvent MatchFail = new FloatEvent();
	public StringEvent MatchesTextUpdate = new StringEvent();
	public UnityEvent ClearFail = new UnityEvent();
	public UnityEvent ClearAll = new UnityEvent();

	public AudioMixer mixer;
	public AudioMixerSnapshot ReverbOn, ReverbOff;

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
				if (!matched.Contains(g) && g.waveType == pg.waveType && withinTolerace(g.Freq, pg.Freq, FreqTolerace) && withinTolerace(g.offset, pg.offset, OffsetTolerance))
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
		targetCombiner.GetComponent<AudioSource>().mute = !targetCombiner.GetComponent<AudioSource>().mute;
		playerCombiner.GetComponent<AudioSource>().mute = !playerCombiner.GetComponent<AudioSource>().mute;
	}

	public void Randomize()
	{
		//Difficulty_LoFreq, Difficulty_HiFreq, Difficulty_LoOff, Difficulty_HiOff;
		string s = "";
		foreach (FrequencyGeneratorV2 g in generators)
		{
			g.TargetFreq = Random.Range(Difficulty_LoFreq, Difficulty_HiFreq);
			g.offset = Random.Range(Difficulty_LoOff, Difficulty_HiOff);
			g.SetWaveType(Random.Range(0, 4));
			string offsetstr = "";
			if (g.offset == 0) offsetstr = "";
			else offsetstr = (g.offset > 0 ? ("+" + g.offset.ToString()) : g.offset.ToString()) + " offset, ";
			s += g.TargetFreq.ToString("0.000") + "Hz, " + offsetstr + "Type:" + g.waveType + "\n";
		}
		updateHint.Invoke(s);
		ClearAll.Invoke();
	}
	bool withinTolerace(float a, float b, float tolerance)
	{
		if (Mathf.Abs(a - b) < Mathf.Abs(tolerance)) return true;
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

	public void SetLowFreqRange(string s)
	{
		if (float.TryParse(s, out Difficulty_LoFreq))
		{
			if (Difficulty_LoFreq > Difficulty_HiFreq)
			{
				Debug.Log("Say what?");
				Difficulty_HiFreq = Difficulty_LoFreq;
			}
			RefreshDifficultyDisplay();
		}
	}
	public void SetHighFreqRange(string s)
	{
		if (float.TryParse(s, out Difficulty_HiFreq))
		{
			if (Difficulty_LoFreq > Difficulty_HiFreq) Difficulty_LoFreq = Difficulty_HiFreq;
			RefreshDifficultyDisplay();
		}
	}
	public void SetLowOffsetRange(string s)
	{
		if (int.TryParse(s, out Difficulty_LoOff))
		{
			if (Difficulty_LoOff > Difficulty_HiOff) Difficulty_LoOff = Difficulty_HiOff;
			RefreshDifficultyDisplay();
		}
	}
	public void SetHighOffsetRange(string s)
	{
		if (int.TryParse(s, out Difficulty_HiOff))
		{
			if (Difficulty_LoOff > Difficulty_HiOff) Difficulty_HiOff = Difficulty_LoOff;
			RefreshDifficultyDisplay();
		}
	}
	void RefreshDifficultyDisplay()
	{
		FreqDiffChange.Invoke(Difficulty_LoFreq + " - " + Difficulty_HiFreq);
		OffDiffChange.Invoke(Difficulty_LoOff + " - " + Difficulty_HiOff);
	}

	public void toggleReverb(bool b)
	{
		if (b)
		{
			ReverbOn.TransitionTo(1.0f);
		}
		else
		{
			ReverbOff.TransitionTo(1.0f);
		}
	}
}
