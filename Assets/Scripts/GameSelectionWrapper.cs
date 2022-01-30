using UnityEngine;
using UnityEngine.EventSystems;

public class GameSelectionWrapper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameSelected gameSelected;

    void Start()
    {
        GameObject go = GameObject.Find("GameSelected");
        if (go)
            gameSelected = go.GetComponent<GameSelected>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameSelected)
            gameSelected.setDetails(gameObject.name);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameSelected)
            gameSelected.removeDetails();
    }

    public void startGame()
    {
        if (gameSelected)
            gameSelected.startGame(gameObject.name);
    }
}
