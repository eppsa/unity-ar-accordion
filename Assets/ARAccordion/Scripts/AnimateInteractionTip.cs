using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class AnimateInteractionTip : MonoBehaviour
{
    private const float DURATION = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;

    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void OnEnable()
    {
        this.startPosition = transform.localPosition - Vector3.up * 100;
        this.endPosition = transform.localPosition - Vector3.down * 200;

        this.transform.localPosition = this.startPosition;

        StartCoroutine(DoMove(startPosition, endPosition, DURATION));
        StartCoroutine(DoFade(0, 1, DURATION));
    }

    private IEnumerator DoMove(Vector3 from, Vector3 to, float duration)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                transform.localPosition = Vector3.Lerp(from, to, moveCurve.Evaluate(progress));

                yield return new WaitForEndOfFrame();
            } else {
                transform.localPosition = to;

                this.transform.localPosition = this.startPosition;
                StartCoroutine(DoMove(startPosition, endPosition, DURATION));

                yield break;
            }
        }
    }

    private IEnumerator DoFade(float from, float to, float duration)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                float alpha = Mathf.Lerp(from, to, fadeCurve.Evaluate(progress));
                Color color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, alpha);

                GetComponent<Image>().color = color;

                yield return new WaitForEndOfFrame();
            } else {
                Color color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, 1);

                GetComponent<Image>().color = color;
                StartCoroutine(DoFade(0, 1, DURATION));

                yield break;
            }
        }
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}
