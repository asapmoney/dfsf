using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public GameObject[] difficultyButtons; 
    GameManager man;

    private void Start()
    {
        man = GameManager.instance;
    }

    public void Easy()
    {
        man.ChangeDifficulty(GameManager.Difficulty.Easy);
        HideAllButtons();

    }

    public void Medium()
    {
        man.ChangeDifficulty(GameManager.Difficulty.Medium);
        HideAllButtons();
    }

    public void Hard()
    {
        man.ChangeDifficulty(GameManager.Difficulty.Hard);
        HideAllButtons();
    }

    public void HideAllButtons(){
        foreach (var button in difficultyButtons){
            button.SetActive(false); 
        }
    }
}
