using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//https://answers.unity.com/questions/1417541/is-it-possible-to-create-sound-with-scripting.html
public class FrequencyGenerator : MonoBehaviour
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
	public string FreqS
	{
		get {
			string s = Freq.ToString("0.00") + "Hz";
			if (Mathf.Abs(Freq) > 1000) s = (Freq / 1000).ToString("0.000") +  "KHz";
			return s; }
		set {
			float newF = 0;
			bool b = float.TryParse(value, out newF);
			if (b) Freq = newF; 
		}
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


	public float[] samples = new float[0];
	Queue<float[]> samplesStack = new Queue<float[]>();
	List<float[]> samplesHistory = new List<float[]>();

	public GameObject line = default;
	public GameObject objs = default;

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
		if (a == null)
		{
			a = gameObject.AddComponent<AudioSource>();
			a.volume = 0.075f;
		}
		running = true;
		waveType = WaveType.Sine;
		usingWaveFunc = Sine;
		newWaveFunc = Sine;
		_usingFreq = Freq;
		samples = new float[1024];
		if (line != null)
		{
			for (int i = 0; i < 1024; i++)
			{
				GameObject o = Instantiate(objs, line.transform);
				o.name = "Obj " + i.ToString("0000");
			}
		}

		OnFreqChange.AddListener(updateOscilloscope);
		OnOffsetChange.AddListener(updateOscilloscope);
		if(customWaveform!=null)customWaveform.OnWaveFormUpdate.AddListener(updateOscilloscope);
		OnFreqChange.AddListener(updateHertz);
		OnOffsetChange.AddListener(updateOffset);
		yield return null;
		OnFreqChange.Invoke(Freq);

	}

	private void Update()
	{
		while (samplesStack.Count > 0)
		{
			samplesHistory.Add(samplesStack.Dequeue());
		}
		while (samplesHistory.Count > 10)
		{
			samplesHistory.RemoveAt(0);
		}
	}

	private void OnEnable()
	{
		NumberOfGenerators++;
	}
	private void OnDisable()
	{
		NumberOfGenerators--;
	}

	void updateOscilloscope(float f) { updateOscilloscope(); }
	void updateOscilloscope()
	{
		if (line != null)
		{
			Transform[] children = line.GetComponentsInChildren<Transform>(false);
			for (int i = 0; i < children.Length - 1; i++)
			{
				children[i + 1].localPosition = new Vector3(i, newWaveFunc(i+offset, Freq, sampleRate) * mult, 0);
			}
		}
	}

	void updateHertz(float f)
	{
		refreshFreqStrings.Invoke(FreqS);
	}

	void updateOffset(float f)
	{
		refreshOffsetStrings.Invoke(offset.ToString("0"));
	}

	public void TuneFreq(float f)
	{
		Freq += f;
	}
	public void TuneOffset(float f)
	{
		offset += (int)Mathf.Sign(f);
		updateOffset(f);
	}

	public float getSample(int n)
	{
		return usingWaveFunc(usingPosition + n, _usingFreq, sampleRate);
	}
	public float getSampleUniversal(int n)
	{
		return usingWaveFunc(n+offset, _usingFreq, sampleRate);
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!running)
			return;
		int dataLen = data.Length / channels;
		samplesStack.Enqueue((float[])samples.Clone());
		samples = new float[dataLen]; 

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
					data[n * channels + i] += getSample(n);
					lastSamples[i] = data[n * channels + i];
				}
				i++;
			}
			samples[n] = data[n * channels];
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
