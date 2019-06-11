using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragMe2 : MonoBehaviour, IDragHandler
{	
	GameObject DragObject;


	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out worldPoint))
		{
			this.transform.position = worldPoint;
		}
	}
}
