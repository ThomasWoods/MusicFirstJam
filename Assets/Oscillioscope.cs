using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillioscope : MonoBehaviour
{
	public int resolution = 1024;
	public Transform dataPointsParent = default;
	public GameObject dataPointPrefab = default;

	public float[] samples = new float[0];
	Queue<float[]> samplesStack = new Queue<float[]>();
	List<float[]> samplesHistory = new List<float[]>();

	public float heightMult = 1;

	void OnAudioFilterRead(float[] data, int channels)
	{
		int dataLen = data.Length / channels;
		samples = new float[dataLen];

		int n = 0;
		while (n < dataLen)
		{
			samples[n] = data[n * channels];
			n++;
		}
		samplesStack.Enqueue((float[])samples.Clone());
	}

	private void Start()
	{
		if (dataPointsParent == null) return;
		for (int i = 0; i < resolution; i++)
		{
			GameObject o = Instantiate(dataPointPrefab, dataPointsParent);
			o.name = "Data Point " + i.ToString("0000");
		}
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

	void updateOscilloscope()
	{
		if (dataPointsParent == null) return;
		Transform[] children = dataPointsParent.GetComponentsInChildren<Transform>(false);
		for (int i = 0; i < children.Length - 1; i++)
		{
			children[i + 1].localPosition = new Vector3(i, samplesHistory[0][i]*heightMult, 0);
		}
	}
	void updateOscilloscope(float f) { updateOscilloscope(); }//to use with FloatEvents
}
