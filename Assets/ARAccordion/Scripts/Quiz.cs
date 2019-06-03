using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler
{	
	public static GameObject dropObject;
	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint;
        
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out worldPoint))
		{
			this.transform.position = worldPoint;
			dropObject = this.gameObject;
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		Debug.Log("dropped");
	}
}
