using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] GameObject quizImage;
    [SerializeField] GameObject accordionImage;

    public void Toggle(State state)
    {
        switch (state) {
            case State.ACCORDION:
                accordionImage.SetActive(false);
                quizImage.SetActive(true);
                break;
            case State.QUIZ:
                accordionImage.SetActive(true);
                quizImage.SetActive(false);
                break;
        }
    }
}
