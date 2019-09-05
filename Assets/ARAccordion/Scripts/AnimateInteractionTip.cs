using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimateInteractionTip : MonoBehaviour
{
    private const float FADE_IN_DURATION = 0.5f;
    private const float FADE_OUT_DURATION = 0.2f;

    Color color;

    public void OnEnable()
    {
        StartCoroutine(DoFade(0.0f, 1.0f, FADE_IN_DURATION));

        color = GetComponent<Color>();
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
                float alpha = Mathf.Lerp(from, to, progress);
                color = new Color(color.r, color.g, color.b, alpha);

                yield return new WaitForEndOfFrame();
            } else {
                color = new Color(color.r, color.g, color.b, 1);

                yield break;
            }
        }
    }

    void Update()
    {
        // 1. Fade in
        // 2. Move up
        // 3. Fade out
        // 4. Repeat
    }
}
