using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Knob : MonoBehaviour
{
	[System.Serializable] public class FloatEvent : UnityEvent<float> { }
	public float multiplier=1f;
	public FloatEvent Rotate = new FloatEvent();

	float lastAngle;
	RectTransform rectT;
	Canvas canvas;

	private void Start()
	{
		rectT = GetComponent<RectTransform>();
		canvas = GetComponentInParent<Canvas>();
	}


	public void OnBeginDrag(BaseEventData data)
	{
		PointerEventData d = (PointerEventData)data;
		if (d == null) return;
		Vector2 mPos = getRelativePos(d.position);
		lastAngle = Mathf.Atan2(mPos.y, mPos.x) * Mathf.Rad2Deg + 180;
	}
	public void OnDrag(BaseEventData data)
	{
		PointerEventData d = (PointerEventData)data;
		if (d == null) return;
		Vector2 mPos = getRelativePos(d.position);
		float angle=Mathf.Atan2(mPos.y,mPos.x)*Mathf.Rad2Deg+180;
		float delta = lastAngle - angle;

		if (delta > 300) delta -= 360;
		if (delta < -300) delta += 360;

		Rotate.Invoke(delta*multiplier);
		RotateMe(delta);
		lastAngle = angle;
	}
	void RotateMe(float f) {
		rectT.Rotate(new Vector3(0, 0, f*-1));
	}

	Vector2 getRelativePos(Vector2 d)
	{
		Vector2 lPos=default;// = rectT.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if(canvas.renderMode==RenderMode.WorldSpace)
			lPos = rectT.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			lPos = rectT.position - Input.mousePosition;
		else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
			lPos = rectT.position - canvas.worldCamera.ScreenToWorldPoint(Input.mousePosition);
		//Debug.Log(lPos);
		return lPos;
	}
}
