using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable] public class FloatEvent : UnityEvent<float> { }
[System.Serializable] public class StringEvent : UnityEvent<string> { }

//https://answers.unity.com/questions/1417541/is-it-possible-to-create-sound-with-scripting.html
public class FrequencyGeneratorV2 : MonoBehaviour
{
	[System.Serializable]
	public enum WaveType { Sine, Rect, Sawtooth, Triangle, Custom }
	public WaveType waveType;
	public void SetWaveType(int i) { SetWaveType((WaveType)i); ; }
	public void SetWaveType(WaveType newWaveType)
	{
		waveType = newWaveType;
		switch (waveType)
		{
			case WaveType.Sine: function = Sine; break;
			case WaveType.Rect: function = Rect; break;
			case WaveType.Sawtooth: function = Sawt; break;
			case WaveType.Triangle: function = Tria; break;
			case WaveType.Custom: function = Cust; break;
		}
		OnFreqChange.Invoke(Freq);
	}

	public int _offset = 0;
	public int offset { get { return _offset; } set { _offset = value; OnOffsetChange.Invoke(offset); } }
	public int sampleRate = 44100;
	public float _Freq = 440;
	public float Freq
	{
		get { return _Freq; }
		set { _Freq = value; OnFreqChange.Invoke(_Freq); }
	}
	public string FreqS
	{
		get {
			string s = Freq.ToString("0.00") + "Hz";
			if (Mathf.Abs(Freq) > 1000) s = (Freq / 1000).ToString("0.000") + "KHz";
			return s;
		}
		set {
			float newF = 0;
			bool b = float.TryParse(value, out newF);
			if (b) TargetFreq = newF;
		}
	}

	public float _targetFreq = 440;
	public float TargetFreq
	{
		get { return _targetFreq; }
		set { _targetFreq = value; }
	}
	public bool useSmoothing { get; set; }
	public float smoothDampVel=0;
	public float SmoothDampTime = 0.1f;

	public float _Volume = 1;
	public float Volume
	{
		get { return _Volume; }
		set { _Volume = value;}
	}

	public delegate float WaveFunction(int i, float frequency, float sampleFreq);
	public WaveFunction function;
	public UserWaveform customWaveform = default;

	public FloatEvent OnFreqChange = new FloatEvent();
	public FloatEvent OnOffsetChange = new FloatEvent();
	public StringEvent refreshFreqStrings = new StringEvent();
	public StringEvent refreshOffsetStrings = new StringEvent();


	IEnumerator Start()
	{
		waveType = WaveType.Sine;
		function = Sine;

		OnFreqChange.AddListener(updateHertz);
		OnOffsetChange.AddListener(updateOffset);
		if (customWaveform != null) customWaveform.OnWaveFormUpdate.AddListener(OnCustomWaveformChanged);
		yield return null;
		OnFreqChange.Invoke(Freq);

	}
	private void Update()
	{
		if (Freq != TargetFreq)
		{
			if (useSmoothing) Freq = Mathf.SmoothDamp(Freq, TargetFreq, ref smoothDampVel, SmoothDampTime);
			else Freq = TargetFreq;
		}
	}


	void updateHertz(float f)
	{
		refreshFreqStrings.Invoke(FreqS);
	}

	void updateOffset(float f)
	{
		refreshOffsetStrings.Invoke(offset.ToString());
		OnFreqChange.Invoke(Freq);
	}
	void OnCustomWaveformChanged()
	{
		OnFreqChange.Invoke(Freq);
	}

	public void TuneFreq(float f)
	{
		TargetFreq += f;
	}
	public void TuneOffset(float f)
	{
		offset += (int)Mathf.Sign(f);
		updateOffset(f);
	}

	public float getSample(int n)
	{
		return function(n + offset, Freq, sampleRate);
	}



	float Sine(int i, float frequency, float sampleRate) { return Mathf.Sin(Mathf.PI * 2 * i * frequency / sampleRate); }
	float Rect(int i, float frequency, float sampleRate) { return (Mathf.Repeat(i * frequency / sampleRate, 1) > 0.5f) ? 1f : -1f; }
	float Sawt(int i, float frequency, float sampleRate) { return Mathf.Repeat(i * frequency / sampleRate, 1) * 2f - 1f; }
	float Tria(int i, float frequency, float sampleRate) { return Mathf.PingPong(i * 2f * frequency / sampleRate, 1) * 2f - 1f; }
	float Cust(int i, float frequency, float sampleRate) { return customWaveform.samples[(int)Mathf.Repeat(i * frequency / customWaveform.samples.Length, customWaveform.samples.Length)]; }
}
