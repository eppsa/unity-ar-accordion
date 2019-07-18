using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Model;
using System;
using System.Linq;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler
{

    private Accordion accordion;

    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject dropArea;

    [SerializeField] private Text questionText;

    [SerializeField] private Color rightColor = new Color(0, 200, 0);
    [SerializeField] private Color wrongColor = new Color(200, 0, 0);
    [SerializeField] private Color defaultColor = new Color(255, 255, 255);

    [SerializeField] private float scaleFactor = 1.2f;
    [SerializeField] private float defaultScaleFactor = 1.0f;
    [SerializeField] private float nextQuestionDelay = 1.5f;

    private int correctAnswers = 0;
    private int maxQuestions = 5;

    [SerializeField] private int maxCharacters = 14;
    [SerializeField] private int normalTextSize = 40;
    [SerializeField] private int smallTextSize = 30;

    private Model.Accordion content;

    private List<Layer> pickedLayers = new List<Layer>();

    private int currentQuestionIndex = 0;

    private GameObject activeDraggable;
    private Vector3 activeDraggableStartPosition;
    bool questionAnswered;

    int pickedId;


    public void Awake()
    {
        accordion = this.transform.parent.GetComponent<Accordion>();
    }

    public void OnEnable()
    {
        InitQuiz();
        StartCoroutine(StartQuiz());
    }

    IEnumerator StartQuiz()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);

        StartCoroutine(accordion.MoveToLayer(0));
        while (accordion.isMoving) yield return null;
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].id));
        while (accordion.isMoving) yield return null;

        foreach (Transform child in transform) child.gameObject.SetActive(true);

        UpdateQuizContent();
    }


    void Start()
    {
        // for (int i = 0; i < pickedLayers.Count; i++) {
        //     Debug.Log(pickedLayers[i].id);
        // }
    }


    public void SetContent(Model.Accordion content)
    {
        this.content = content;
    }

    private void InitQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswers = 0;
        pickedLayers = GetRandomLayers(maxQuestions);

    }

    private List<Layer> GetRandomLayers(int count)
    {
        var random = new System.Random();
        List<Layer> randomLayers = this.content.layers.OrderBy(layer => random.Next()).ToList();
        List<Layer> pickedLayers = randomLayers.GetRange(0, count);
        List<Layer> sortedLayers = pickedLayers.OrderBy(layer => layer.id).ToList();

        return sortedLayers;
    }

    IEnumerator UpdateQuiz()
    {
        if (currentQuestionIndex < maxQuestions) {
            StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].id));
            while (accordion.isMoving) yield return null;
            UpdateQuizContent();
        } else {
            ShowResult();
        }
    }

    private void UpdateQuizContent()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(true);
        SetPositions();

        List<Question> questions = pickedLayers[currentQuestionIndex].questions;
        pickedId = UnityEngine.Random.Range(0, questions.Count);
        Question question = questions[pickedId];
        questionText.text = question.questionText;

        for (int i = 0; i < answerContainers.Length; i++) {

            Text containerText = answerContainers[i].GetComponentInChildren<Text>();
            containerText.text = question.answers[i];

            if (containerText.text.Length > maxCharacters) {
                containerText.fontSize = smallTextSize;
            } else {
                containerText.fontSize = normalTextSize;
            }
        }
    }

    private void ShowResult()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        string resultText = string.Format(this.content.resultText, correctAnswers, maxQuestions);
        questionText.text = resultText;

        dropArea.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeDraggable) {
            Vector3 worldPoint;

            bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                activeDraggable.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint
            );

            if (hit && !questionAnswered) {
                activeDraggable.transform.position = worldPoint;
                activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.004f);
            }

            if (eventData.pointerEnter && eventData.pointerEnter.gameObject == dropArea) {
                dropArea.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            } else {
                dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            }
        } else {
            if (eventData.pointerEnter && eventData.pointerEnter.tag == "AnswerContainer") {
                activeDraggable = eventData.pointerEnter;
                activeDraggableStartPosition = activeDraggable.transform.position;

                activeDraggable.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (activeDraggable == null) {
            return;
        }

        if (eventData.pointerEnter.gameObject == dropArea) {
            dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable.transform.position = eventData.pointerEnter.transform.position;
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            questionAnswered = true;

            CheckAnswer();
        } else {
            activeDraggable.transform.position = activeDraggableStartPosition;
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable = null;
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = pickedLayers[currentQuestionIndex].questions[pickedId].correctAnswerId;
        int draggableIndex = Array.IndexOf(answerContainers, activeDraggable);

        if (draggableIndex == correctAnswerId) {
            Debug.Log("Right");
            correctAnswers++;
            activeDraggable.GetComponent<Image>().color = rightColor;
            Invoke("Reset", nextQuestionDelay);
        } else {
            Debug.Log("Wrong");
            activeDraggable.GetComponent<Image>().color = wrongColor;
            Invoke("Reset", nextQuestionDelay);
        }
    }

    private void Reset()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);

        activeDraggable.transform.position = activeDraggableStartPosition;
        activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);

        activeDraggable.GetComponent<Image>().color = defaultColor;
        questionAnswered = false;
        activeDraggable = null;

        currentQuestionIndex++;
        StartCoroutine(UpdateQuiz());
    }

    public void SetPositions()
    {

        GameObject quizAnchor = accordion.activeTile.transform.GetChild(1).gameObject;
        Debug.Log(quizAnchor.transform.parent.parent.name);


        GameObject.Find("QuestionContainer").transform.position = quizAnchor.transform.Find("QuestionAnchor").transform.position;
        //QuestionObj.transform.position = GameObject.Find("QuestionAnchor").transform.TransformPoint(GameObject.Find("QuestionAnchor").transform.position);



        GameObject.Find("AnswerContainer 1").transform.position = quizAnchor.transform.Find("AnswerAnchor1").transform.position;

        GameObject.Find("AnswerContainer 2").transform.position = quizAnchor.transform.Find("AnswerAnchor2").transform.position;

        GameObject.Find("AnswerContainer 3").transform.position = quizAnchor.transform.Find("AnswerAnchor3").transform.position;

        GameObject.Find("AnswerContainer 4").transform.position = quizAnchor.transform.Find("AnswerAnchor4").transform.position;

        GameObject.Find("DropArea").transform.position = quizAnchor.transform.Find("DropAnchor").transform.position;
    }

}
