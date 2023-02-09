using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IDragHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler {

	public GameObject Board;
	public Vector2 Center;
	public Vector2 Output;
	private Transform CenterInd;
	private Canvas canvas;
	private bool setCenter = true;

	private void Start() {
		Board = GameObject.Find("Board");
		canvas = transform.GetComponentInParent<Canvas>();
		CenterInd = transform.Find("Center");
		Center = transform.position;
	}

	private void Update() {
		CenterInd.position = Center;
		if (setCenter)
			Center = transform.position;
	}

	public void OnDrag(PointerEventData eventData) {
		Vector2 touchPos = Camera.main.ScreenToWorldPoint(eventData.position);
		float dis = Vector2.Distance(touchPos, gameObject.transform.position);
		Output = touchPos - Center;//(Vector2)transform.position;
		Output *= 0.3f * canvas.scaleFactor / (Board.transform.localScale.x / 10);
		if (Output.magnitude > 1)
			Output *= 1/Output.magnitude;
	}

	public void OnPointerUp(PointerEventData eventData) {
		Output = Vector2.zero;
		Center = transform.position;
		setCenter = true;
	}
	public void OnPointerExit(PointerEventData eventData) {
		//Output = Vector2.zero;
		//Center = transform.position;
	}

	private float GetAngle(Vector2 pos) {
		var x0 = transform.position.x - pos.x;
		var y0 = transform.position.y - pos.y;
		float angle = (Mathf.Atan2(y0, x0) * Mathf.Rad2Deg) + 90;
		if (angle > 180) {
			angle -= 360;
		} else if (angle < -180) {
			angle += 360;
		}
		return angle;
	}

	public void OnPointerDown(PointerEventData eventData) {
		Center = Camera.main.ScreenToWorldPoint(eventData.position);
		setCenter = false;
	}
}