using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//https://answers.unity.com/questions/1417541/is-it-possible-to-create-sound-with-scripting.html
public class AudioGen1 : MonoBehaviour
{
	public double bpm = 140.0F;
	public float gain = 0.5F;
	public int signatureHi = 4;
	public int signatureLo = 4;

	private double nextTick = 0.0F;
	private float amp = 0.0F;
	private float phase = 0.0F;
	private double sampleRate = 0.0F;
	private int accent;
	private bool running = false;

	float[] f = new float[0];

	void Start()
	{
		gameObject.AddComponent<AudioSource>();
		accent = signatureHi;
		double startTick = AudioSettings.dspTime;
		sampleRate = AudioSettings.outputSampleRate;
		nextTick = startTick * sampleRate;
		running = true;
	}
	private void Update()
	{
		string s = "";
		for (int i = 0; i < f.Length; i++) s += f[i]+"\n";
		Debug.Log(s);
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!running)
			return;
		double samplesPerTick = sampleRate * 60.0F / bpm * 4.0F / signatureLo;
		double sample = AudioSettings.dspTime * sampleRate;
		int dataLen = data.Length / channels;
		f = new float[data.Length];
		float t = 0;
		int n = 0;
		while (n < dataLen)
		{
			float x = gain * amp * Mathf.Sin(phase);
			f[n]= x;
			int i = 0;
			while (i < channels)
			{
				data[n * channels + i] += x;
				i++;
			}
			while (sample + n >= nextTick)
			{
				nextTick += samplesPerTick;
				amp = 1.0F;
				if (++accent > signatureHi)
				{
					accent = 1;
					amp *= 2.0F;
				}
				Debug.Log("Tick: " + accent + "/" + signatureHi);
			}
			phase += amp * 0.3F;
			amp *= 0.993F;
			n++;
		}
	}

	float Sine(int i, int frequency, int sampleFreq) { return Mathf.Sin(Mathf.PI * 2 * i * frequency / sampleFreq); }
	float Rect(int i, int frequency, int sampleFreq) { return (Mathf.Repeat(i * frequency / sampleFreq, 1) > 0.5f) ? 1f : -1f; }
	float Sawt(int i, int frequency, int sampleFreq) { return Mathf.Repeat(i * frequency / sampleFreq, 1) * 2f - 1f; }
	float Tria(int i, int frequency, int sampleFreq) { return Mathf.PingPong(i * 2f * frequency / sampleFreq, 1) * 2f - 1f; }
}
