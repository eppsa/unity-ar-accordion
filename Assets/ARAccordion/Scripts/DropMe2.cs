using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMe2 : MonoBehaviour, IDropHandler
{
	public Image containerImage;
	public Image receivingImage;
	private Color normalColor;
	public Color highlightColor = Color.yellow;
	
	public void OnEnable ()
	{
		if (containerImage != null)
			normalColor = containerImage.color;
	}
	
	public void OnDrop(PointerEventData data)
	{
		GameObject dropObject = data.pointerDrag;
		containerImage.color = normalColor;
		
		dropObject.transform.position = this.transform.position;
		
	}
}
