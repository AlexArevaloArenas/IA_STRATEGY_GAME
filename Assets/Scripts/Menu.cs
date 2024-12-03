using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject deadMenu;
    [SerializeField] private GameObject winMenu;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
        mainMenu.SetActive(false);
    }
    public void EndGame()
    {
        Debug.Log("llendo al ppal");
        SceneManager.LoadScene(0);
    }

    public void YouDied()
    {
        //deadMenu.SetActive(true);
        SceneManager.LoadScene(2);

    }
    public void YouWin()
    {
        SceneManager.LoadScene(3);

    }
}
