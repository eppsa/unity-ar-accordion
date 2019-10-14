using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Model;
using System;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

enum QuizState
{
    START,
    NEXT_QUESTION,
    RESULT
}

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerUpHandler
{
    private const float ANSWER_MOVE_SPEED = 1.0f;

    [SerializeField] private GameObject questionContainer;
    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject resultContainer;
    [SerializeField] private GameObject dropArea;

    [SerializeField] private Color rightColor = new Color(0, 200, 0);
    [SerializeField] private Color wrongColor = new Color(200, 0, 0);
    [SerializeField] private Color defaultColor = new Color(255, 255, 255);

    [SerializeField] private float dragScaleFactor;
    [SerializeField] private float defaultScaleFactor = 1.0f;
    [SerializeField] private float nextQuestionDelay = 1.5f;

    [SerializeField] private int maxQuestions = 5;

    [SerializeField] private UnityEvent dragStartEvent;
    [SerializeField] private UnityEvent dropEvent;
    [SerializeField] private UnityEvent quizRightEvent;
    [SerializeField] private UnityEvent quizWrongEvent;

    private Accordion accordion;

    private Model.Accordion content;

    private List<KeyValuePair<int, Layer>> pickedLayers = new List<KeyValuePair<int, Layer>>();
    private int correctAnswerCount = 0;

    Question currentQuestion;
    private int currentQuestionIndex = 0;
    bool currentQuestionAnswered;

    private GameObject selectedAnswer;
    private Vector3 selectedAnswerStartPosition;

    private bool waiting = false;

    private QuizState state;
    private Vector2 pointerDownPosition;

    public void Awake()
    {
        accordion = this.transform.parent.GetComponent<Accordion>();
    }

    public void Reset()
    {
        InitQuiz();

        Show(false);
    }

    private void InitQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswerCount = 0;
        pickedLayers = GetRandomLayers(maxQuestions);

        resultContainer.SetActive(false);

        state = QuizState.START;
    }

    public void OnAccordionMovementFinsh()
    {
        switch (state) {
            case QuizState.START:
                this.state = QuizState.NEXT_QUESTION;
                accordion.MoveTo(pickedLayers[currentQuestionIndex].Key, nextQuestionDelay);
                break;
            case QuizState.NEXT_QUESTION:
                ShowNextQuestion();
                break;
            case QuizState.RESULT:
                ShowResult();
                break;
        }
    }

    private void ShowNextQuestion()
    {
        SetPositions();
        UpdateQuizContent();
        Show(true);
    }

    private List<KeyValuePair<int, Layer>> GetRandomLayers(int count)
    {
        System.Random random = new System.Random();

        return this.content.layers
            .Select((layer, index) => new KeyValuePair<int, Layer>(index, layer))
            .Where((item) => item.Value.questions != null)
            .OrderBy(entry => random.Next())
            .ToList()
            .GetRange(0, count)
            .OrderBy(entry => entry.Key)
            .ToList();
    }

    private void Show(bool show)
    {
        foreach (Transform child in transform) {
            if (child.gameObject.name != "ResultContainer") {
                child.gameObject.SetActive(show);
            }
        }
    }

    private void UpdateQuizContent()
    {
        List<Question> questions = pickedLayers[currentQuestionIndex].Value.questions;
        int questionIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[questionIndex];

        questionContainer.transform.GetChild(0).GetComponent<Text>().text = currentQuestion.question;

        for (int i = 0; i < answerContainers.Length; i++) {
            Text containerText = answerContainers[i].GetComponentInChildren<Text>();
            containerText.text = currentQuestion.answers[i];
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag()");

        if (waiting || currentQuestionAnswered) {
            return;
        }

        if (selectedAnswer) {
            selectedAnswer.transform.localScale = new Vector3(dragScaleFactor, dragScaleFactor, dragScaleFactor);

            Vector3 worldPoint;

            bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                selectedAnswer.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint
            );

            if (hit && !currentQuestionAnswered) {
                selectedAnswer.transform.position = worldPoint;
                selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.004f);
            }

            if (eventData.pointerEnter && eventData.pointerEnter.gameObject == dropArea) {
                dropArea.transform.localScale = new Vector3(dragScaleFactor, dragScaleFactor, dragScaleFactor);
            } else {
                dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            }
        } else {
            if (eventData.pointerEnter && eventData.pointerEnter.tag == "AnswerContainer") {
                dragStartEvent.Invoke();

                selectedAnswer = eventData.pointerEnter;
                selectedAnswerStartPosition = selectedAnswer.transform.position;

                Debug.Log("selectedAnswer " + selectedAnswer);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop()");

        if (selectedAnswer == null || waiting || currentQuestionAnswered) {
            return;
        }

        Debug.Log(eventData);
        Debug.Log(eventData.pointerEnter);
        Debug.Log(eventData.pointerEnter.gameObject);

        if (eventData.pointerEnter && eventData.pointerEnter.gameObject == dropArea) {
            dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            selectedAnswer.transform.position = eventData.pointerEnter.transform.position;
            selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.002f);
            selectedAnswer.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            currentQuestionAnswered = true;
            dropEvent.Invoke();

            CheckAnswer();
        } 
    }

    public void OnEndDrag(PointerEventData eventData) {
        Debug.Log("OnEndDrag()");

        if (currentQuestionAnswered || selectedAnswer == null) {
            return;
        }

        selectedAnswer.transform.position = selectedAnswerStartPosition;
        selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.001f);
        selectedAnswer.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
        selectedAnswer = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentQuestionAnswered || waiting) {
            return;
        }

        this.pointerDownPosition = eventData.position;
        Debug.Log(this.pointerDownPosition);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
        if (dragged(eventData.position) || currentQuestionAnswered || waiting) {
            return;   
        }

        if (eventData.pointerEnter && eventData.pointerEnter.tag == "AnswerContainer") {
            selectedAnswer = eventData.pointerEnter;
            selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.004f);

            currentQuestionAnswered = true;

            this.pointerDownPosition = Vector2.zero;

            StartCoroutine(MoveAnswerToDropArea());
        }
    }

    private bool dragged(Vector3 pointerUpPosition) {
        return Vector3.Distance(this.pointerDownPosition, pointerUpPosition) != 0;
    }

    private IEnumerator MoveAnswerToDropArea()
    {
        float speed = ANSWER_MOVE_SPEED;
        float step = speed * Time.deltaTime;

        while (true) {
            selectedAnswer.transform.position = Vector3.MoveTowards(selectedAnswer.transform.position, dropArea.transform.position, step);

            if (Vector3.Distance(selectedAnswer.transform.position, dropArea.transform.position) < 0.001f) {
                selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.002f);

                CheckAnswer();
                yield break;
            } else {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = currentQuestion.correctAnswerIndex;
        int draggableIndex = Array.IndexOf(answerContainers, selectedAnswer);

        if (draggableIndex == correctAnswerId) {
            quizRightEvent.Invoke();
            Debug.Log("Right");
            correctAnswerCount++;
            selectedAnswer.GetComponent<Image>().color = rightColor;

            waiting = true;
            Invoke("NextQuestion", nextQuestionDelay);
        } else {
            quizWrongEvent.Invoke();
            Debug.Log("Wrong");
            selectedAnswer.GetComponent<Image>().color = wrongColor;

            waiting = true;
            Invoke("NextQuestion", nextQuestionDelay);
        }
    }

    private void NextQuestion()
    {
        Show(false);

        selectedAnswer.transform.position = selectedAnswerStartPosition;
        selectedAnswer.transform.localPosition = new Vector3(selectedAnswer.transform.localPosition.x, selectedAnswer.transform.localPosition.y, -0.002f);

        selectedAnswer.GetComponent<Image>().color = defaultColor;
        currentQuestionAnswered = false;
        selectedAnswer = null;

        currentQuestionIndex++;

        UpdateQuiz();

        waiting = false;
    }

    void UpdateQuiz()
    {
        if (currentQuestionIndex < maxQuestions) {
            accordion.MoveTo(pickedLayers[currentQuestionIndex].Key, nextQuestionDelay);
        } else {
            this.state = QuizState.RESULT;
            accordion.MoveToStart(nextQuestionDelay);
        }
    }

    private void ShowResult()
    {
        Transform anchor = accordion.transform.Find("QuizResultAnchor");

        transform.position = anchor.position;
        transform.rotation = anchor.rotation;

        transform.SetParent(anchor);

        resultContainer.SetActive(true);
        resultContainer.transform.localPosition = new Vector3(0, 0, 0);

        string resultText = GetResultText();
        resultContainer.transform.GetChild(0).GetComponent<Text>().text = resultText;

        dropArea.SetActive(false);
    }

    private string GetResultText()
    {
        switch (correctAnswerCount) {
            case 0:
                return this.content.quiz.resultBad;
            case 1:
                return string.Format(this.content.quiz.resultOne, correctAnswerCount, maxQuestions);
            default:
                return string.Format(this.content.quiz.resultGood, correctAnswerCount, maxQuestions);
        }
    }

    public void SetPositions()
    {
        Transform anchor = accordion.CurrentLayerAnchor.transform.Find("QuizAnchor");

        Debug.Log(accordion.CurrentLayerAnchor.transform.parent.name);

        transform.position = anchor.position;
        transform.rotation = anchor.rotation;

        transform.SetParent(anchor);

        Vector3 questionAnchorPosition = anchor.Find("QuestionAnchor").transform.position;
        questionContainer.transform.position = new Vector3(questionAnchorPosition.x, questionAnchorPosition.y, questionAnchorPosition.z);

        Vector3 dropAreaAnchorPosition = anchor.Find("DropAnchor").transform.position;
        dropArea.transform.position = new Vector3(dropAreaAnchorPosition.x, dropAreaAnchorPosition.y, dropAreaAnchorPosition.z + dropArea.transform.localPosition.z);

        for (int i = 0; i < answerContainers.Length; i++) {
            Vector3 answerPosition = anchor.Find("AnswerAnchor" + (i + 1)).transform.position;
            answerContainers[i].transform.position = new Vector3(answerPosition.x, answerPosition.y, answerPosition.z);
        }

    }

    public void SetContent(Model.Accordion content)
    {
        this.content = content;
    }
}
