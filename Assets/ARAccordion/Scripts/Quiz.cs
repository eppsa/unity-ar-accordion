using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using jsonObject;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject[] answerContainers;
    private GameObject QuestionContainer;

    private Color heighlightCorrect = new Color(0, 200, 0);
    private Color heightlightWrong = new Color(200, 0, 0);

    private Color normalTileColor = new Color(255, 255, 255);

    private string correctAnswer = "BWeisheit";

	private int countCorrectAnswers = 0;

	private int maxQuestions = 5;

	public List<int> randomSelectedQuestions = new List<int>();
    private GameObject activeTile;
    private Vector3 tileStartPosition;

    private Vector3 selectedScale = new Vector3(1.5f, 1.5f, 1.5f);
    private Vector3 normalScale = new Vector3(1.0f, 1.0f, 1.0f);

    private jsonObject.Quiz quiz;

    private int currentQuestion = 1;


    bool answerChosen;


    private void Start()
    {
        QuestionContainer = GameObject.FindGameObjectWithTag("QuestionContainer");
        answerContainers = GameObject.FindGameObjectsWithTag("AnswerContainer");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (activeTile == null)
        {
            activeTile = eventData.pointerEnter;
            tileStartPosition = activeTile.transform.position;

            if (activeTile.tag == "AnswerContainer" && !answerChosen)
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
                if (activeTile.tag == "AnswerContainer" && !answerChosen)
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
            answerChosen = true;
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
        if (activeTile != null && !answerChosen)
        {
            if (activeTile.tag == "AnswerContainer" && !answerChosen)
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
			countCorrectAnswers++;
            activeTile.GetComponent<Image>().color = heighlightCorrect;
			Invoke("ResetQuizTiles", 1.0f);
        }
        else
        {
            Debug.Log("Wrong");
            activeTile.GetComponent<Image>().color = heightlightWrong;
			Invoke("ResetQuizTiles", 1.0f);
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

            if (progress <= 1.0f)
            {
                //detailPopup.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(fadeFrom, fadeTo, progress);
                yield return new WaitForEndOfFrame();
            }
            else
            {
                //detailPopup.GetComponent<CanvasGroup>().alpha = fadeTo;
                yield break;
            }
        }
    }

    private void ResetQuizTiles()
    {
		activeTile.transform.position = tileStartPosition;
        activeTile.GetComponent<Image>().color = normalTileColor;
        answerChosen = false;
        activeTile = null;
		currentQuestion++;
        UpdateQuizContent();

    }

    public void SetContent(jsonObject.Quiz quiz)
    {
        this.quiz = quiz;
    }

    private void UpdateQuizContent()
    {
        if (currentQuestion <= maxQuestions)
        {
            int totalQuestions = quiz.questions.Count;
            int questionNumber = Random.Range(0, totalQuestions);

			if (randomSelectedQuestions.Contains(questionNumber))
			{
				UpdateQuizContent();
			}
			else
			{
				randomSelectedQuestions.Add(questionNumber);
				int correctAnswerId = int.Parse(this.quiz.questions[questionNumber].correctAnswer);
				correctAnswer = this.quiz.questions[questionNumber].answers[correctAnswerId - 1];

				QuestionContainer.GetComponentInChildren<Text>().text = this.quiz.questions[questionNumber].questionText;

				int answerNumber = 0;
				foreach (GameObject answerContainer in answerContainers)
				{
					answerContainer.GetComponentInChildren<Text>().text = this.quiz.questions[questionNumber].answers[answerNumber];
					answerNumber++;
				}
			}
        }
        else
        {	
			string resultText = string.Format(this.quiz.resultText, countCorrectAnswers, maxQuestions);
			QuestionContainer.GetComponentInChildren<Text>().text = resultText;
			GameObject.FindGameObjectWithTag("DropArea").SetActive(false);

			foreach (GameObject answerContainer in answerContainers)
            {
                answerContainer.SetActive(false);
            }


        }

    }
}
