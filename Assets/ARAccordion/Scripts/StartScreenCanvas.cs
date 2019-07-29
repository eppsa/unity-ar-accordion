using UnityEngine;

public class StartScreenCanvas : MonoBehaviour
{
    public void OnStartButton()
    {
        this.transform.gameObject.SetActive(false);
    }
}
