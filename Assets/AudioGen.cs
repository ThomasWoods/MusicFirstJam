using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//https://answers.unity.com/questions/1417541/is-it-possible-to-create-sound-with-scripting.html
public class AudioGen : MonoBehaviour
{
	[System.Serializable]
	public enum WaveType { Sine, Rect, Sawtooth, Triangle }
	public WaveType _waveType;
	public WaveType waveType { get { return _waveType; }set { _waveType = value; Refresh(); } }
	public int waveTypeInt { get { return (int)_waveType; }set { _waveType = (WaveType)value; FreqChange.Invoke(frequency); } }

	public int position = 0;
	public int _sampleFreq = 44100;
	public int sampleFreq { get { return _sampleFreq; } set { _sampleFreq = value; } }
	public float FsampleFreq { get { return _sampleFreq; } set { _sampleFreq = (int)value; FreqChange.Invoke(frequency); } }
	public float _frequency = 440;
	public float frequency { get { return _frequency*_frequencyScale; } set { _frequency = value; FreqChange.Invoke(frequency); } }
	public float baseFrequency { get { return _frequency; } set { _frequency = value; _frequencyScale = 1; FreqChange.Invoke(frequency); } }
	public float _frequencyScale = 1;
	public float scaleFrequency { get { return _frequencyScale; } set { _frequencyScale = value; FreqChange.Invoke(frequency); } }

	float[] samples = new float[44100];

	AudioSource lastsource = default;
	AudioClip clip;

	[System.Serializable]
	public class FloatEvent : UnityEvent<float> { }

	public FloatEvent FreqChange = new FloatEvent();
	
    // Start is called before the first frame update
    void Start()
	{
		FreqChange.AddListener(RefreshF);
		setup();
	}
	
	void OnAudioRead(float[] data)
	{/*
		int count = 0;
		while (count < data.Length)
		{
			//data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / sampleFreq);
			switch (waveType) {
				case WaveType.Sine: data[count] = Sine(position); break;
				case WaveType.Rect: data[count] = Rect(position); break;
				case WaveType.Sawtooth: data[count] = Sawt(position); break;
				case WaveType.Triangle: data[count] = Tria(position); break;
			}
			position++;
			count++;
		}
		Debug.Log("Reading Data: "+data.Length);
		*/
	}
	
	void OnAudioSetPosition(int newPosition)
	{
		/*
		Debug.Log("Setting Position: " + position+"->"+newPosition);
		position = newPosition;
		*/
	}

	public void RefreshF(float a=0) { Refresh(); }
	public void Refresh()
	{
		if (!enabled) return;
		/*
		samples = new float[sampleFreq];
		for (int i = 0; i < samples.Length; i++)
		{
			samples[i] = Mathf.Sin(Mathf.PI * 2 * i * frequency / sampleFreq);
		}
		AudioClip ac = AudioClip.Create("Test", samples.Length, 1, sampleFreq, false);
		ac.SetData(samples, 0);

		AudioSource audiosource = GetComponent<AudioSource>();
		if (audiosource != null)
		{
			audiosource.clip = ac;
			Debug.Log(ac+" " +sampleFreq + " "+ frequency);
			audiosource.Play();
		}
		*/
		Debug.Log(frequency);
		if (lastsource != null)
		{
			lastsource.volume = 0;
			StartCoroutine(DelayedDestruction(lastsource));
		}
		lastsource = gameObject.AddComponent<AudioSource>();
		lastsource.volume = 0.05f;
		lastsource.loop = true;
		clip = AudioClip.Create("Test", samples.Length, 1, sampleFreq, false);
		samples = new float[sampleFreq];
		int count = 0;
		while (count < samples.Length)
		{
			//data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / sampleFreq);
			switch (waveType)
			{
				case WaveType.Sine: samples[count] = Sine((int)(Time.time * 1000) + count); break;
				case WaveType.Rect: samples[count] = Rect((int)(Time.time * 1000) + count); break;
				case WaveType.Sawtooth: samples[count] = Sawt((int)(Time.time * 1000) + count); break;
				case WaveType.Triangle: samples[count] = Tria((int)(Time.time * 1000) + count); break;
			}
			position++;
			if (position >= sampleFreq) position = 0;
			count++;
		}
		clip.SetData(samples, 0);
		lastsource.clip = clip;
		lastsource.Play();
	}

	IEnumerator DelayedDestruction(AudioSource c)
	{
		c.volume *= 0.75f;
		yield return null;
		c.volume *= 0.5f;
		yield return null;
		c.volume *= 0.25f;
		yield return null;
		c.volume = 0.0f;
		yield return null;
		Destroy(c);
	}

	public void setup()
	{
		//clip = AudioClip.Create("MySinusoid", sampleFreq * 2, 1, sampleFreq, true, OnAudioRead, OnAudioSetPosition);
		//clip=AudioClip.Create("Test", samples.Length, 1, sampleFreq, false);
		//sourceA.clip = clip;
		if(enabled)
			Refresh();
	}

	float Sine(int i) { return Mathf.Sin(Mathf.PI * 2 * i * frequency / sampleFreq); }
	float Rect(int i) { return (Mathf.Repeat(i * frequency / sampleFreq, 1) > 0.5f) ? 1f : -1f; }
	float Sawt(int i) { return Mathf.Repeat(i * frequency / sampleFreq, 1) * 2f - 1f; }
	float Tria(int i) { return Mathf.PingPong(i * 2f * frequency / sampleFreq, 1) * 2f - 1f; }
}
