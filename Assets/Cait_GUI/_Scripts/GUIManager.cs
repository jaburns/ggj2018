using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;


public class GUIManager : MonoBehaviour {

    public GameObject scrMainMenu;
    public GameObject scrOptions;
    public GameObject scrGamePlay;
    public GameObject scrPause;
    public GameObject scrWin;
    public GameObject scrLose;

    public AudioControl audioControl;

   
    void Start () {

        scrMainMenu.SetActive(true);
        scrOptions.SetActive(false);
        scrGamePlay.SetActive(false);
        scrPause.SetActive(false);

        audioControl.loadExisting();

    }
	

	void Update () {

		
	}

    

    public void GoToGame()
    {
        scrMainMenu.SetActive(false);
        scrPause.SetActive(false);
        scrGamePlay.SetActive(true);
    }

    public void GoToPause()
    {
        scrPause.SetActive(true);
    }

    public void GoToOptions()
    {
        scrOptions.SetActive(true);
    }

    public void GoToWin()
    {
        scrGamePlay.SetActive(false);
        scrWin.SetActive(true);
    }

    public void GoToLose()
    {
        scrGamePlay.SetActive(false);
        scrLose.SetActive(true);
    }
}
