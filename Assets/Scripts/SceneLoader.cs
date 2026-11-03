using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

public class SceneLoader : MonoBehaviour
{
    private bool loading;

    public void Load(string sceneName)
    {
        if (loading) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        loading = true;

        // Stop the AR session in this scene before leaving it
        ARSession session = FindFirstObjectByType<ARSession>();
        if (session != null)
        {
            session.enabled = false;
            yield return null;
        }

        // Restart the XR system so the next scene starts with fresh AR subsystems
        LoaderUtility.Deinitialize();
        yield return null;
        LoaderUtility.Initialize();
        yield return null;

        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        UnityEngine.Application.Quit();
    }
}