using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] GameObject quizImage;
    [SerializeField] GameObject accordionImage;

    private AudioSource clickSound;

    private void OnEnable()
    {
        clickSound = GameObject.Find("Sounds/Click").GetComponent<AudioSource>();
    }

    public void Toggle(bool active)
    {
        clickSound.Play();

        accordionImage.SetActive(active);
        quizImage.SetActive(!active);
    }
}
