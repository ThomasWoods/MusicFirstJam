using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrquencyCombiner : MonoBehaviour
{
	public List<FrequencyGeneratorV2> generators = new List<FrequencyGeneratorV2>();
	int position = 0;

	private void OnAudioFilterRead(float[] data, int channels)
	{
		int dataLen = data.Length / channels;
		int n = 0;
		while (n < dataLen)
		{
			int i = 0;
			while (i < channels) {
				float s = 0;
				foreach (FrequencyGeneratorV2 f in generators) {
					if (f == null) continue;
					s += f.getSample(position+n);
				}
				data[n * channels + i] = s / generators.Count;
				i++;
			}
			n++;
		}
		position += n;
	}
}
