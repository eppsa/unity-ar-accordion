using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using jsonObject;
using System;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject questionContainer;
    [SerializeField] private GameObject dropArea;

    private Color heighlightCorrect = new Color(0, 200, 0);
    private Color heightlightWrong = new Color(200, 0, 0);

    private Color normalTileColor = new Color(255, 255, 255);

    private string correctAnswer;

    private int countCorrectAnswers = 0;

    private int maxQuestions = 5;

    private List<int> randomSelectedQuestions = new List<int>();
    private GameObject activeDraggable;
    private Vector3 dragStartPosition;

    private float scaleFactor = 1.5f;
    private Vector3 normalScale = new Vector3(1.0f, 1.0f, 1.0f);

    private jsonObject.Quiz quiz;

    private int currentQuestion = 1;


    bool answerChosen;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeDraggable == null)
        {
            activeDraggable = eventData.pointerEnter;
            if (activeDraggable.tag != "AnswerContainer")
            {
                return;
            }

            dragStartPosition = activeDraggable.transform.position;

            if (!answerChosen) // todo
            {
                activeDraggable.transform.localScale = activeDraggable.transform.localScale * scaleFactor;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeDraggable != null)
        {
            Vector3 worldPoint;

            bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                activeDraggable.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint
            );

            if (hit && !answerChosen)
            {
                activeDraggable.transform.localScale = normalScale;
                activeDraggable.transform.position = worldPoint;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerEnter.gameObject == dropArea)
        {
            activeDraggable.transform.position = eventData.pointerEnter.transform.position;
            answerChosen = true;
            checkAnswer();
        }
        else
        {
            activeDraggable.transform.position = dragStartPosition;
            answerChosen = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeDraggable != null && !answerChosen)
        {
            activeDraggable.transform.position = dragStartPosition;
            activeDraggable.GetComponent<Image>().color = normalTileColor;
            activeDraggable.transform.localScale = normalScale;
            activeDraggable = null;
        }
    }

    private void checkAnswer()
    {
        string currentAnswer = activeDraggable.GetComponentInChildren<Text>().text;

        int answerIndex = Array.IndexOf(answerContainers, currentAnswer) + 1;
        int correctAnswerIndex = this.quiz.questions[currentQuestion].correctAnswerId;

        if (answerIndex == correctAnswerIndex)
        {
            Debug.Log("Right");
            countCorrectAnswers++;
            activeDraggable.GetComponent<Image>().color = heighlightCorrect;
            Invoke("ResetQuizTiles", 1.0f);
        }
        else
        {
            Debug.Log("Wrong");
            activeDraggable.GetComponent<Image>().color = heightlightWrong;
            Invoke("ResetQuizTiles", 1.0f);
        }
    }

    private void ResetQuizTiles()
    {
        activeDraggable.transform.position = dragStartPosition;
        activeDraggable.GetComponent<Image>().color = normalTileColor;
        answerChosen = false;
        activeDraggable = null;
        currentQuestion++;
        UpdateQuizContent();
    }

    public void SetContent(jsonObject.Quiz quiz)
    {
        this.quiz = quiz;
        UpdateQuizContent();
    }

    private void UpdateQuizContent()
    {
        if (currentQuestion <= maxQuestions)
        {
            int totalQuestions = quiz.questions.Count;
            int questionNumber = UnityEngine.Random.Range(0, totalQuestions);

            if (randomSelectedQuestions.Contains(questionNumber))
            {
                UpdateQuizContent();
            }
            else
            {
                randomSelectedQuestions.Add(questionNumber);
                int correctAnswerId = this.quiz.questions[questionNumber].correctAnswerId;
                correctAnswer = this.quiz.questions[questionNumber].answers[correctAnswerId - 1];

                questionContainer.GetComponentInChildren<Text>().text = this.quiz.questions[questionNumber].questionText;

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
            questionContainer.GetComponentInChildren<Text>().text = resultText;
            dropArea.SetActive(false);

            foreach (GameObject answerContainer in answerContainers)
            {
                answerContainer.SetActive(false);
            }
        }
    }
}
