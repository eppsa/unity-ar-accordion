using UnityEngine;

public class Sounds : MonoBehaviour
{
    [SerializeField] private AudioSource click;
    [SerializeField] private AudioSource drag;
    [SerializeField] private AudioSource drop;
    [SerializeField] private AudioSource quizRight;
    [SerializeField] private AudioSource quizWrong;
    [SerializeField] private AudioSource rotationWheelSnap;

    public void OnClick()
    {
        click.Play();
    }

    public void OnDrag()
    {
        drag.Play();
    }

    public void OnDrop()
    {
        drop.Play();
    }

    public void OnQuizRight()
    {
        quizRight.Play();
    }

    public void OnQuizWrong()
    {
        quizWrong.Play();
    }

    public void OnRotationWheelSnap()
    {
        rotationWheelSnap.Play();
    }
}
