using UnityEngine;
using System.Collections;


public class StartScreenCanvas : MonoBehaviour
{
    private Controller Controller;

    private bool isFadeing;
    private float duration = 0.7f;
    private float prepareDelay = 0.5f;



    public void OnEnable()
    {
        StartCoroutine(Fade(0.0f, 1.0f, duration));
    }

    void Start()
    {
        Controller = GameObject.Find("Controller").GetComponent<Controller>();
    }

    public void OnStartButton()
    {
        StartCoroutine(PrepareMainView());
    }

    private IEnumerator PrepareMainView()
    {

        StartCoroutine(Fade(1.0f, 0.0f, duration));
        while (isFadeing) {
            yield return null;
        }

        yield return new WaitForSeconds(prepareDelay);

        this.transform.gameObject.SetActive(false);
        Controller.OnStart();
    }

    private IEnumerator Fade(float fadeFrom, float fadeTo, float duration)
    {
        isFadeing = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                GetComponent<CanvasGroup>().alpha = Mathf.Lerp(fadeFrom, fadeTo, progress);
                yield return new WaitForEndOfFrame();
            } else {
                this.GetComponent<CanvasGroup>().alpha = fadeTo;
                isFadeing = false;
                yield break;
            }
        }
    }
}


