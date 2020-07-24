using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//https://answers.unity.com/questions/1417541/is-it-possible-to-create-sound-with-scripting.html
public class FrequencyGeneratorV2 : MonoBehaviour
{

	[System.Serializable]
	public enum WaveType { Sine, Rect, Sawtooth, Triangle, Custom }
	public WaveType waveType;
	public void SetWaveype(int i) { SetWaveType((WaveType)i); ; }
	public void SetWaveType(WaveType newWaveType){
		if (setTranstion) return;
		waveType = newWaveType;
		switch (waveType)
		{
			case WaveType.Sine: newWaveFunc = Sine; break;
			case WaveType.Rect: newWaveFunc = Rect; break;
			case WaveType.Sawtooth: newWaveFunc = Sawt; break;
			case WaveType.Triangle: newWaveFunc = Tria; break;
			case WaveType.Custom: newWaveFunc = Cust; break;
		}
		setTranstion = true;
		OnFreqChange.Invoke(_Freq);
	}


	private bool running = false;

	public float[] lastSamples=new float[2];
	static public int _position = 0;
	static public int position { get { return _position; } set { _position = value; } }
	public int usingPosition { get { return _position + _offset; } }
	public float _offsetPerc = 0;
	public float offsetPerc { get { return _offsetPerc; } set { _offsetPerc = value; offset = Mathf.FloorToInt((sampleRate/Freq) * offsetPerc); updateOffsetPercDisplay(0); } }
	public int _offset = 0;
	public int offset { get { return _offset; } set { _offset = value; OnOffsetChange.Invoke(offset); } }
	int sampleRate = 44100;
	public float _usingFreq = 440;
	public float _Freq = 440;
	public float Freq
	{
		get { return _Freq; }
		set { if (setTranstion) return; _Freq = value; setTranstion = true; OnFreqChange.Invoke(_Freq); }
	}
	public float _Volume = 1;
	public float Volume
	{
		get { return _Volume; }
		set { if (setTranstion) return; _Volume = value; setTranstion = true; OnFreqChange.Invoke(_Freq); }
	}

	public bool transition=false, setTranstion=false;
	public float stepsize = 0.0075f, transitionTime;

	delegate float WaveFunction(int i, float frequency, float sampleFreq);
	WaveFunction usingWaveFunc, newWaveFunc;

	public GameObject customFunc = default;

	public float mult = 1;

	[System.Serializable] public class FloatEvent : UnityEvent<float> { }
	[System.Serializable] public class StringEvent : UnityEvent<string> { }
	public FloatEvent OnFreqChange = new FloatEvent();
	public FloatEvent OnOffsetChange = new FloatEvent();
	public StringEvent refreshFreqStrings = new StringEvent();
	public StringEvent refreshOffsetStrings = new StringEvent();

	public UserWaveform customWaveform = default;

	public static int NumberOfGenerators = 0;
	public static int GeneratorUpdates = 0;

	IEnumerator Start()
	{
		var a = gameObject.GetComponent<AudioSource>();
		if(a==null) a = gameObject.AddComponent<AudioSource>();
		a.volume = 0.075f;
		running = true;
		waveType = WaveType.Sine;
		usingWaveFunc = Sine;
		newWaveFunc = Sine;
		_usingFreq = Freq;

		OnFreqChange.AddListener(updateHertzDisplay);
		OnOffsetChange.AddListener(updateOffsetPercDisplay);
		yield return null;
		OnFreqChange.Invoke(Freq);

	}


	private void OnEnable()
	{
		NumberOfGenerators++;
	}
	private void OnDisable()
	{
		NumberOfGenerators--;
	}


	void updateHertzDisplay(float f)
	{
		string s = "Hz";
		if (f > 1000) { Freq /= 1000; s = "KHz"; }
		refreshFreqStrings.Invoke(Freq.ToString("0.00") + s);
	}

	void updateOffsetPercDisplay(float f)
	{
		refreshOffsetStrings.Invoke(offsetPerc.ToString("0.00")+"%");
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!running)
			return;
		int dataLen = data.Length / channels;

		float target=0;
		if (setTranstion)
		{
			target = newWaveFunc(usingPosition+dataLen, Freq, sampleRate);
			transition = true;
		}
		int n = 0;
		while (n < dataLen)
		{
			int i = 0;
			while (i < channels)
			{
				if (transition)
				{
					data[n * channels + i] = Mathf.MoveTowards(lastSamples[i], target, stepsize);
					lastSamples[i] = data[n * channels + i];
					if (Mathf.Abs(lastSamples[0] - target) <= stepsize)
					{
						_usingFreq = Freq;
						usingWaveFunc = newWaveFunc;
						transition = false;
						setTranstion = false;
					}
				}
				else
				{
					data[n * channels + i] += usingWaveFunc(usingPosition + n, _usingFreq, sampleRate);
					lastSamples[i] = data[n * channels + i];
				}
				i++;
			}
			n++;
		}
		if (transition)
		{
			_usingFreq = Freq;
			usingWaveFunc = newWaveFunc;
			transition = false;
			setTranstion = false;
		}

		GeneratorUpdates++;
		if (GeneratorUpdates >= NumberOfGenerators)
		{
			position += n;
			GeneratorUpdates = 0;
		}
		
	}


	float Sine(int i, float frequency, float sampleRate) { return Mathf.Sin(Mathf.PI * 2 * i * frequency / sampleRate); }
	float Rect(int i, float frequency, float sampleRate) { return (Mathf.Repeat(i * frequency / sampleRate, 1) > 0.5f) ? 1f : -1f; }
	float Sawt(int i, float frequency, float sampleRate) { return Mathf.Repeat(i * frequency / sampleRate, 1) * 2f - 1f; }
	float Tria(int i, float frequency, float sampleRate) { return Mathf.PingPong(i * 2f * frequency / sampleRate, 1) * 2f - 1f; }
	float Cust(int i, float frequency, float sampleRate) { return customWaveform.samples[(int)Mathf.Repeat(i * frequency / customWaveform.samples.Length, customWaveform.samples.Length)]; }
}
