using UnityEngine;

public class StartScreenCanvas : MonoBehaviour
{
    private Controller Controller;

    void Start()
    {
        Controller = GameObject.Find("Controller").GetComponent<Controller>();
    }

    public void OnStartButton()
    {
        this.transform.gameObject.SetActive(false);
        Controller.OnStart();
    }
}
