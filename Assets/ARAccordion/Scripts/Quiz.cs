using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{	
	private GameObject detailPopup;
	private Color heighlightCorrect = new Color (0,200,0);
	private Color heightlightWrong = new Color (200,0,0);

	private Color normalTileColor = new Color (255,255,255);

	private string correctAnswer = "Weisheit";
	private GameObject activeTile;
	private Vector3 tileStartPosition;

	private Vector3 selectedScale = new Vector3 (1.5f,1.5f,1.5f);
	private Vector3 normalScale = new Vector3 (1.0f,1.0f,1.0f);


	bool answerGiven;


	private void Start()
	{
		detailPopup = GameObject.Find("InformationContainer");
		detailPopup.GetComponent<CanvasGroup>().alpha = 0;

	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (activeTile == null)
		{
			activeTile = eventData.pointerEnter;
			tileStartPosition = activeTile.transform.position;

			if (activeTile.tag == "AnswerContainer" && !answerGiven)
			{
				activeTile.transform.localScale = selectedScale;
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
				if (activeTile.tag == "AnswerContainer" && !answerGiven)
				{
					activeTile.transform.localScale = normalScale;
					activeTile.transform.position = worldPoint;
				}
			}
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerEnter.tag == "DropArea" && activeTile.tag == "AnswerContainer")
		{
			answerGiven = true;
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
		if (activeTile != null && !answerGiven)
		{
			if (activeTile.tag == "AnswerContainer" && !answerGiven)
			{
				activeTile.transform.position = tileStartPosition;
				activeTile.GetComponent<Image>().color = normalTileColor;
				activeTile.transform.localScale = normalScale;
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
			activeTile.GetComponent<Image>().color = heighlightCorrect;
			StartCoroutine(Fade(0.0f,1.0f,0.7f));
		}
		else
		{
			Debug.Log("Wrong");
			activeTile.GetComponent<Image>().color = heightlightWrong;
			StartCoroutine(ResetQuiz());
		}
	}

	private IEnumerator Fade(float fadeFrom, float fadeTo, float duration)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true)
        {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                detailPopup.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(fadeFrom, fadeTo, progress);
                yield return new WaitForEndOfFrame();
            } else {
                detailPopup.GetComponent<CanvasGroup>().alpha = fadeTo;
                yield break;
            }
        }
    }

	private IEnumerator ResetQuiz(){
		yield return new WaitForSeconds(2);
		activeTile.transform.position = tileStartPosition;
		activeTile.GetComponent<Image>().color = normalTileColor;
		answerGiven = false;
		activeTile = null;
	}
}
