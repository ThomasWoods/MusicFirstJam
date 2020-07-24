using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrquencyCombiner : MonoBehaviour
{
	public List<FrequencyGenerator> generators = new List<FrequencyGenerator>();

	private void OnAudioFilterRead(float[] data, int channels)
	{
		int dataLen = data.Length / channels;
		int n = 0;
		while (n < dataLen)
		{
			int i = 0;
			while (i < channels) {
				foreach (FrequencyGenerator f in generators) {

				}
			}
		}
	}
}
