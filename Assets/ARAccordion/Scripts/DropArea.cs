using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
	private Color normalColor;
	public Color highlightColor = Color.yellow;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Quiz.activeTile != null)
		{
			Debug.Log(Quiz.activeTile.name);
		}
	}
	
	public void OnDrop(PointerEventData data)
	{
		//Debug.Log(data.pointerDrag);
		GameObject dropObject = Quiz.activeTile;
		dropObject.transform.position = this.transform.position;
	}
}
