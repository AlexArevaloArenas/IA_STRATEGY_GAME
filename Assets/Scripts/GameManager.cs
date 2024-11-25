using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    int state = -1;
    GameObject[] playerTeam;
    GameObject[] enemyTeam;
    public bool acabado = false;
    bool victory = false;
    int teamSize;

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

        
        
    }

    void startGame() {
        GeneraEquipos();
        //if (creación de personajes completa) state = 1;
    }

    void runGame() {

    }

    void endGame() {
        
        if (victory) {
            //canvasPrefab.GetComponent<Menu>().YouWin();
        }
        else {
            //canvasPrefab.GetComponent<Menu>().YouDied();
        }
        Time.timeScale = 0;
    }

    void GeneraEquipos()
    {
        int r = -1;
        for (int i = 0; i<teamSize;i++){
            if (i >teamSize/2 && teamSize > 3){
                //playerTeam[i] = infanteria
                //enemyTeam[i] = infanteria
            }
            else{
                r = Random.Range(0,2);
                switch (r) {
                    case 0: {
                        //playerTeam[i] = mago

                        break;
                    } 
                    case 1: {
                        //playerTeam[i] = caballero
                        break;
                    } 
                    case 2: {
                        //playerTeam[i] = arquero
                        break;
                    } 
                }
                r = Random.Range(0,2);
                switch (r) {
                    case 0: {
                        //enemyTeam[i] = mago

                        break;
                    } 
                    case 1: {
                        //enemyTeam[i] = caballero
                        break;
                    } 
                    case 2: {
                        //enemyTeam[i] = arquero
                        break;
                    } 
                }
            }
            //playerTeam[i].position = ;
            //enemyTeam[i].position = ;
        }


    }    
}