using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AsyncLoadItem))]
public class AndroidBackButton : MonoBehaviour {

    private static AndroidBackButton instance;
    private Dictionary<string, string> backSceneMap;
    private PauseToggler pauseToggler;
    private StoreSelectionScreen storeSelection;
    public GameObject storeselection;
    public GameObject BackButton;
    private AsyncLoadItem asyncItem;
    
	// Use this for initialization
	void Start () {


            #if UNITY_ANDROID 
                Destroy(this.gameObject);
            #endif

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            Destroy(this.gameObject);
        }
      
        asyncItem = GetComponent<AsyncLoadItem>();
        MakeMap();	
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.loadedLevelName == "Shop")
        {
            //Checks if Buy selection screen is open
            storeselection = GameObject.FindGameObjectWithTag("StoreSelection");
            BackButton = GameObject.FindGameObjectWithTag("BackButton");
        }

		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            //if in start menu, then quit the game
            if (Application.loadedLevelName == "StartMenu")
            {
                ActualApplicationQuit();
            }
                
            //if in game then pause or unpause
            else if(Application.loadedLevelName == "Game")
            {
                //When back button is clicked, the game pauses
                if(pauseToggler != null)
                {
                    if(pauseToggler.paused)
                    {
                        pauseToggler.UnPause();
                    }
                    else if(!pauseToggler.paused)
                    {
                        pauseToggler.Pause();
                    }
                }
            }

            //Either sends user back to main menu or closes shop menu
            else if(Application.loadedLevelName == "Shop")
            {
                if (storeselection == null)
                {
                    GoBackToScene(Application.loadedLevelName);
                }

                else
                    BackButton.GetComponent<Button>().onClick.Invoke();
            }

            else if(Application.loadedLevelName == "EndGame")
            {
                Debug.Log("Worked");
                Application.LoadLevel("StartMenu");
            }

            else if(Application.loadedLevelName != "SplitBinaryLoader")
            {
                GoBackToScene(Application.loadedLevelName);
            }
        }
	
	}

    public static AndroidBackButton Instance()
    {
        return instance;
    }

    private void MakeMap()
    {
        backSceneMap = new Dictionary<string, string>();

        //we can add more in necessary
        backSceneMap.Add("OptionsMenu", "StartMenu");
        backSceneMap.Add("Shop", "StartMenu");
        
    }

    private void GoBackToScene(string currentScene)
    {
        if(backSceneMap.ContainsKey(currentScene))
        {
            StartAsync(backSceneMap[currentScene]);
        }
    }

    void OnLevelWasLoaded(int level)
    { 

        if (Application.loadedLevelName == "Game")
        {
            pauseToggler = GameObject.Find("Pauser").GetComponent<PauseToggler>();
        }
    }

    private void StartAsync(string sceneToGoTo)
    {
        asyncItem.ButtonLoad(sceneToGoTo);
    }
    
    private void ActualApplicationQuit()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call<bool>("moveTaskToBack", true);
    }
}
