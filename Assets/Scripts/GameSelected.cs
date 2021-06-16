using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelected : MonoBehaviour
{
    public static string version = "";
    public TMPro.TMP_Text loadingProgress;

    public void startGame(string version)
    {
        GameSelected.version = version;
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("E-LearningScape");
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            loadingProgress.text = (asyncOperation.progress * 100).ToString("F0") + "%";

            yield return null;
        }
    }
}
