using UnityEngine;

public class BackButton : MonoBehaviour
{

    [SerializeField] private ToggleButton toggleButton;
    [SerializeField] private Accordion accordion;
    [SerializeField] private RotationWheel rotationWheel;
    [SerializeField] private Quiz quiz;

    public void OnButtonClick()
    {
        rotationWheel.Toggle(true);
        toggleButton.Toggle(false);
        accordion.DistanceFactor = 0.5f;
        quiz.gameObject.SetActive(false);
        bool quizActive = GameObject.Find("Controller").transform.gameObject.GetComponent<Controller>().quizActive;
        GameObject.Find("Controller").transform.gameObject.GetComponent<Controller>().quizActive = !quizActive;
    }

}