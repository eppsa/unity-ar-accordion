using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using jsonObject;
using System;
using System.Linq;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject dropArea;

    [SerializeField] private Text questionText;
    
    [SerializeField] private Color heighlightCorrect = new Color(0, 200, 0);
    [SerializeField] private Color heightlightWrong = new Color(200, 0, 0);
    [SerializeField] private Color normalTileColor = new Color(255, 255, 255);

    [SerializeField] private float scaleFactor = 1.5f;
    [SerializeField] private float defaultScaleFactor = 1.0f;


    private int correctAnswers = 0;
    private int maxQuestions = 5;

    private jsonObject.Quiz quiz;
    private List<Question> randomQuestions = new List<Question>();
    private int currentQuestionIndex = 0;

    private GameObject activeDraggable;
    private Vector3 activeDraggableStartPosition;
    bool questionAnswered;

    public void SetContent(jsonObject.Quiz quiz)
    {
        this.quiz = quiz;
        InitQuiz();
    }
    
    private void InitQuiz() {
        randomQuestions = GetRandomQuestions(maxQuestions);
        UpdateQuiz();
    }

    private List<Question> GetRandomQuestions(int count) {
        var random = new System.Random();
        List<Question> randomQuestions = this.quiz.questions.OrderBy(question => random.Next()).ToList();

        return randomQuestions.GetRange(0, count);
    }

    private void UpdateQuiz() {
        if (currentQuestionIndex < maxQuestions) {
            UpdateQuizContent();
        } else {
            ShowResult();
        }
    }

    private void UpdateQuizContent() {
        Question question = randomQuestions[currentQuestionIndex];
        questionText.text = question.questionText;

        for (int i = 0; i < answerContainers.Length; i++)
        {
            answerContainers[i].GetComponentInChildren<Text>().text = question.answers[i];
        }
    }

    private void ShowResult() {
        string resultText = string.Format(this.quiz.resultText, correctAnswers, maxQuestions);
        questionText.text = resultText;

        dropArea.SetActive(false);
        foreach (GameObject answerContainer in answerContainers)
        {
            answerContainer.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeDraggable == null)
        {
            GameObject objectBelowPointer = eventData.pointerEnter;
            if (objectBelowPointer.tag == "AnswerContainer")
            {
                activeDraggable = objectBelowPointer;
                activeDraggableStartPosition = activeDraggable.transform.position;

                if (!questionAnswered)
                {
                    activeDraggable.transform.localScale = activeDraggable.transform.localScale * scaleFactor;
                }
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

            if (hit && !questionAnswered)
            {
                activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
                activeDraggable.transform.position = worldPoint;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerEnter.gameObject == dropArea)
        {
            activeDraggable.transform.position = eventData.pointerEnter.transform.position;
            questionAnswered = true;
            CheckAnswer();
        }
        else
        {
            activeDraggable.transform.position = activeDraggableStartPosition;
            questionAnswered = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeDraggable != null && !questionAnswered)
        {
            activeDraggable.transform.position = activeDraggableStartPosition;
            activeDraggable.GetComponent<Image>().color = normalTileColor;
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable = null;
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = randomQuestions[currentQuestionIndex].correctAnswerId;
        int draggableIndex = Array.IndexOf(answerContainers, activeDraggable);

        if (draggableIndex == correctAnswerId)
        {
            Debug.Log("Right");
            correctAnswers++;
            activeDraggable.GetComponent<Image>().color = heighlightCorrect;
            Invoke("Reset", 1.0f);
        }
        else
        {
            Debug.Log("Wrong");
            activeDraggable.GetComponent<Image>().color = heightlightWrong;
            Invoke("Reset", 1.0f);
        }
    }

    private void Reset()
    {
        activeDraggable.transform.position = activeDraggableStartPosition;
        activeDraggable.GetComponent<Image>().color = normalTileColor;
        questionAnswered = false;
        activeDraggable = null;

        currentQuestionIndex++;
        UpdateQuiz();
    }
}
