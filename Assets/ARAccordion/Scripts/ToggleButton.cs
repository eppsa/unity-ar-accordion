using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] GameObject quizImage;
    [SerializeField] GameObject accordionImage;

    public void Toggle(bool active)
    {
        accordionImage.SetActive(active);
        quizImage.SetActive(!active);
    }
}
