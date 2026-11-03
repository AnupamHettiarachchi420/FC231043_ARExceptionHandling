using UnityEngine;

public class SafetyNotice : MonoBehaviour
{
    [SerializeField] private GameObject menuContent;

    private static bool accepted;

    private void Start()
    {
        // First launch: show only the notice. After that: show the menu directly.
        menuContent.SetActive(accepted);
        gameObject.SetActive(!accepted);
    }

    public void Accept()
    {
        accepted = true;
        menuContent.SetActive(true);
        gameObject.SetActive(false);
    }
}