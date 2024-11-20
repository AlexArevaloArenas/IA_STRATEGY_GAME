using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
/*
    public static GameManager Instance { get; private set; }
    int state = -1;
    public int liveChickens = 0;
    public bool caught = false;
    bool victory = false;

    public GameObject canvasPrefab;

    void Start() {
        Time.timeScale = 1;
    }

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (this != Instance)
        {
            Destroy(gameObject);
        }

    }

    void Update() {

        switch (state) {
            case 0: startGame(); break;
            case 1: runGame(); break;
            case 2: endGame(); break;
        }
        statCheck();
    }

    public void statCheck() {

        if (caught) {
            state = 2;
        }
        else if (liveChickens == 0) {
            victory = true;
            state = 2;
        }
    }

    void startGame() {
    }

    void runGame() {

    }

    void endGame() {
        
        if (victory) {
            canvasPrefab.GetComponent<Menu>().YouWin();
        }
        else {
            canvasPrefab.GetComponent<Menu>().YouDied();
        }
        Time.timeScale = 0;
    }
    */
}