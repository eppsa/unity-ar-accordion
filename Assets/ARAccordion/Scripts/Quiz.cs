using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{	
	private GameObject detailPopup;
	public Color32 heighlightCorrect;
	public Color32 heightlightWrong;

	private string correctAnswer = "Weisheit";
	private GameObject activeTile;
	private Vector3 tileStartPosition;


	private void Start()
	{
		detailPopup = GameObject.Find("InformationContainer");
		detailPopup.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (activeTile == null)
		{
			activeTile = eventData.pointerEnter;
			tileStartPosition = activeTile.transform.position;

			if (activeTile.tag == "AnswerContainer")
			{
				activeTile.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint;
        
		if (activeTile != null)
		{
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeTile.GetComponent<RectTransform>(),
																	eventData.position,
																	eventData.pressEventCamera,
																	out worldPoint))
			{
				if (activeTile.tag == "AnswerContainer")
				{
					activeTile.transform.localScale = new Vector3(1,1,1);
					activeTile.transform.position = worldPoint;
				}
			}
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerEnter.tag == "DropArea")
		{
			activeTile.transform.position = eventData.pointerEnter.transform.position;
			checkAnswer();		

		}
		else 
		{
			activeTile.transform.position = tileStartPosition;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (activeTile != null)
		{
			if (activeTile.tag == "AnswerContainer")
			{
				activeTile.transform.position = tileStartPosition;
				activeTile.GetComponent<Image>().color = new Color (100,100,100);
				activeTile.transform.localScale = new Vector3(1,1,1);
			}
			activeTile = null;
		}
	}

	private void checkAnswer() 
	{
		string currentAnswer = activeTile.GetComponentInChildren<Text>().text;

		if (currentAnswer == correctAnswer)
		{
			Debug.Log("Right");
			activeTile.GetComponent<Image>().color = new Color (0,200,0);
		}
		else
		{
			Debug.Log("Wrong");
			activeTile.GetComponent<Image>().color = new Color (200,0,0);
		}
	}
}
