using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserWaveform : MonoBehaviour
{
	Camera cam;
	RectTransform rTransfrom = default;
	float width, height;
	Vector2 halfSize;
	public bool isHovering = false;
	public bool isDragging = false;

	public int maxSamples = 1024;
	public float[] samples;

	public GameObject obj = default;
	public GameObject list = default;
	GameObject[] sampleObjs;

	public float mult = 1;

	public UnityEvent OnWaveFormUpdate = new UnityEvent();

	private void Start()
	{
		rTransfrom = GetComponent<RectTransform>();
		cam = Camera.main;
		width = rTransfrom.rect.width;
		height = rTransfrom.rect.height;
		halfSize = new Vector2(width / 2, height / 2);
		maxSamples = (int)width;
		samples = new float[maxSamples];
		sampleObjs = new GameObject[maxSamples];
		list.transform.localPosition = new Vector2(width*-0.5f, 0);
		mult = height/2;
		for (int i = 0; i < maxSamples; i++)
		{
			GameObject o=Instantiate(obj, list.transform);
			o.name = "Obj " + i;
			o.SetActive(true);
			sampleObjs[i] = o;
		}

		refresh();
	}

	public void Enter()
	{
		//Debug.Log("OnMouseEnter");
		isHovering = true;
	}
	public void Exit()
	{
		//Debug.Log("OnMouseExit");
		isHovering = false;
	}
	public void DragStart()
	{
		//Debug.Log("OnDragStart");
		isDragging = true;
	}
	public void DragEnd()
	{
		//Debug.Log("OnDragEnd");
		isDragging = false;
	}
	void Update()
	{
		if (isHovering && isDragging) UpdateWaveForm();
	}

	void UpdateWaveForm()
	{
		Vector2 o;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rTransfrom, Input.mousePosition, cam, out o);
		//Debug.Log(o);
		Vector2 pos = new Vector2(o.x + width / 2, o.y / (height*0.5f));
		int position = (int)pos.x;
		if (position < 0 || position > 1024) Debug.Log("OutOfRange");
		else
		{
			samples[position] = pos.y;
			sampleObjs[position].transform.localPosition = new Vector3(position, samples[position] * mult, 0);
			OnWaveFormUpdate.Invoke();
		}
	}

	void refresh()
	{
		for (int i = 0; i < samples.Length; i++)
		{
			sampleObjs[i].transform.localPosition = new Vector3(i, samples[i] * mult, 0);
		}
	}
}
