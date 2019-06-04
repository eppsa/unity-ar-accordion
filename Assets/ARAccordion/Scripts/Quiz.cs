using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{	

	public static GameObject activeTile;

	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log(eventData.pointerEnter);
		if (activeTile == null)
		{
			activeTile = eventData.pointerEnter;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint;
        
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeTile.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out worldPoint))
		{
			if (activeTile.tag == "AnswerContainer")
			{
				activeTile.transform.position = worldPoint;
			}
		}
	}

	public void OnDrop(PointerEventData data)
	{
		if (data.pointerEnter.tag == "DropArea")
		{
			activeTile.transform.position = data.pointerEnter.transform.position;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		activeTile = null;
	}
}
