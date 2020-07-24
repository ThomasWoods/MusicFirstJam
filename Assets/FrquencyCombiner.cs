using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrquencyCombiner : MonoBehaviour
{
	public struct WaveSnapshot
	{
		public FrequencyGeneratorV2 g;
		public FrequencyGeneratorV2.WaveFunction function;
		public int offset;
		public float Freq;
		public float sampleRate;
		public WaveSnapshot(FrequencyGeneratorV2 g)
		{
			this.g = g;
			function = g.function;
			offset = g.offset;
			Freq = g.Freq;
			sampleRate = g.sampleRate;
		}
		public float getSample(int n)
		{
			return function(n + offset, Freq, sampleRate);
		}
	}

	public List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();
	int position = 0;

	private void OnAudioFilterRead(float[] data, int channels)
	{
		List<WaveSnapshot> snaps = new List<WaveSnapshot>();
		foreach (FrequencyGeneratorV2 g in generators)
		{
			if(g!=null) snaps.Add(new WaveSnapshot(g));
		}
		//function(n + offset, Freq, sampleRate);
		int dataLen = data.Length / channels;
		int n = 0;
		while (n < dataLen)
		{
			int i = 0;
			while (i < channels) {
				float s = 0;
				foreach (WaveSnapshot snap in snaps) {
					s += snap.getSample(position+n);
				}
				data[n * channels + i] = s / snaps.Count;
				i++;
			}
			n++;
		}
		position += n;
		snaps.Clear();
	}
}
