using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class GameSelected : MonoBehaviour
{
    public static string version = "";
    public TMPro.TMP_Text loadingProgress;

    public TMPro.TMP_Text details;
    public GameObject gameSelection;
    public GameObject LoadingUI;
    public GameObject LoadingFragment;

    public GameObject gamePrefab;
    public GameObject scrollContent;

    private GamesInfo gamesInfo = null;

    private class GamesInfo{
        public List<string> directoryName = new List<string>();
        public List<string> gamesDescription = new List<string>();
    }

    private void Start()
    {
        string[] gameDirectories = Directory.GetDirectories(Application.streamingAssetsPath);
        if (gameDirectories.Length == 0)
            startGame(""); // Will display scene error on loading game
        else if (gameDirectories.Length == 1)
            startGame(Path.GetFileName(gameDirectories[0]));
        else
        {
            if (File.Exists(Application.streamingAssetsPath + "/GamesInfo.txt"))
                gamesInfo = JsonUtility.FromJson<GamesInfo>(File.ReadAllText(Application.streamingAssetsPath + "/GamesInfo.txt"));
            foreach (string gameDirectory in gameDirectories)
            {
                GameObject newGame = Instantiate(gamePrefab);
                newGame.name = Path.GetFileName(gameDirectory);
                newGame.GetComponentInChildren<TMPro.TMP_Text>().text = newGame.name;
                newGame.transform.SetParent(scrollContent.transform);
                newGame.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void startGame(string version)
    {
        GameSelected.version = version;
        gameSelection.SetActive(false);
        LoadingUI.SetActive(true);
        LoadingFragment.SetActive(true);
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

    public void toggleLoading()
    {
        LoadingUI.SetActive(!LoadingUI.activeSelf);
        LoadingFragment.SetActive(!LoadingFragment.activeSelf);
    }
    public void setDetails(string gameName)
    {
        if (gamesInfo != null && gamesInfo.directoryName.Contains(gameName)) {
            int i = gamesInfo.directoryName.IndexOf(gameName);
            if (i >= 0 && i < gamesInfo.gamesDescription.Count)
                details.text = gamesInfo.gamesDescription[i];
        }
    }

    public void removeDetails()
    {
        details.text = "";
    }
}
