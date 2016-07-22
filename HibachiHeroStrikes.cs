using UnityEngine;
using UnityEngine.UI;
using Heyzap;
using System.Collections.Generic;
using System.Collections;

// Strikes that occur when a food falls off the bottom of the screen
// Since the lose condition for the game is currentStrikes > maxStrikes, this script
// indicates the end of the game, via the EndGameEvent and its subscribers
public class Strikes : MonoBehaviour 
{
	private  int                     maxStrikes = 5;   //The max number of strikes before game over
	private  int                     currentStrikes;// The current number of strikes that the player has

	public   Image                   baseStrikeImage;  // What the unfilled strikes look like
	public   Image                   filledStrikeImage;  // What the strike looks like when filled in
	private  List<Image>             baseStrikes; // The outline of the strikes - shows the placement and how many strikes possible total
	private  float                   strikeWidth;  //The preferred width of the strike image
	public   Canvas                  canvas;      
	public   HorizontalLayoutGroup   iconImageLayout;  //Spaces the elements of the order appropriately, horizontally 
	public	 AudioClip				 strikeSFX;	// Sound that plays when a strike is added
	public	 Image 					 strikeFlash;	// Canvas image that holds the strike flash
	public   AnimationClip 			 strikeFlash_animClip;	// To ensure we shut the gameobject off directly after the animation ends
	private  Sprite 				 filledStrike;

    private string lastNetwork = "";

    private float buffertime;

    private Sprite unfilledStrike;

	// To store the slow motion script, used to control when the slow motion plays (when a strike occurs)
	public 	 SlowMo 				slowMo;
	
	public delegate  void 		     EndGameEvent(); //Delegate for when to end game
	public static event EndGameEvent EndGame; 


	public 	bool HT; //used for Hibachi Time
    public int reviveadwatched; //revives if add is watched
    private bool continuegame; //Ends the game
    public GameObject AdButtons;

    // Use this for initialization
    void Start () 
	{
		// Grab strikes gameobject off the background controller prefab
		strikeFlash = PersistentCanvas.instance.gameObject.transform.GetChild(1).GetComponent<Image>();

        slowMo = GameObject.Find (SlowMo.GO_Name).GetComponent<SlowMo>();

		currentStrikes = 0; // starts at zero strikes
		baseStrikes = new List<Image>();

		InitializeBaseStrikes(); // Sets up the correct number of base strikes and positions them
		FoodPassed.SignalStrike += OnStrikeSignal;	// Set up delegate with customer, fires when customer leaves (happily or angrily)
		StartCut.BadCutEvent += OnStrikeSignal;


		filledStrike = filledStrikeImage.GetComponent<Image>().sprite;
        unfilledStrike = baseStrikeImage.GetComponent<Image>().sprite;

		HT = false;
        reviveadwatched = 0;
        continuegame = false;

        buffertime = 0.5f;
	}

    void Update()
    {
        //Slows down time so the X stops flashing after lose
        if (currentStrikes >= maxStrikes)
        {
            if (reviveadwatched != 2)
            {
                Time.timeScale = 0.5f;
                buffertime -= Time.deltaTime;
                
                if (buffertime <= 0)
                {
                    Time.timeScale = 0.0f;
                }
            }
        }

    }

    void InitializeBaseStrikes()
	{
		for (int i = 0; i < maxStrikes; i++)
		{
			Image strike = Instantiate(baseStrikeImage); // Make new strike (with correct darker strike sprite) and set it as a child to the image layout
			strike.transform.SetParent(iconImageLayout.transform, false);	
			baseStrikes.Add (strike);
		}
	}

	// When a strike occurs
	void OnStrikeSignal()
	{
		//if Hibachi Time is false, will not strike out the player
		if (HT == false){
			
		AddStrike(); // adds a strike and Updates the display of the current strikes
		Combo.instance.Hide(1f); // Hide the combo system when a strike occurs
		GlobalMultiplier.instance.Reset(); // Reset the multiplier to it's base when a strike occurs


		// If the current strikes > maxStikres then the game is going to end
		if (currentStrikes >= maxStrikes) 
		{
                if (reviveadwatched != 2)
                {
                    AdButtons.SetActive(true);
                }

                if (reviveadwatched == 2)
                {
                    // Ad controller needs to know when the game ended, so it can show an interstital 
                    AdController.gameOverScreen = true;
                    FoodPassed.SignalStrike -= OnStrikeSignal;
                    EndGame(); // Tells Tip system to end the game
                    EndGame -= SlideUIController.SlideOut;
                }
         } 
		else 
		{
			// Only play the slow mo if this isn't the end of the game (would affect the gong otherwise)
			slowMo.StartSlowMo();
		}
		}
	}
	
	// Adds another strike and displays it, spacing all current strikes out appropriately using the layout width object.
	void AddStrike()
	{
		// Store the image brifely so we can manipulate it
		Image currentStrike = baseStrikes[currentStrikes].GetComponent<Image>();

		// Assign the appropriate sprite
		currentStrike.sprite = filledStrike;

		// Punch the scale to give it that "pop" in effect
		iTween.PunchScale(currentStrike.gameObject, iTween.Hash("amount",new Vector3(5f,5f,1f),"time",1.2f, "islocal", true));

		// Start a coroutine to flash the strike
		StartCoroutine(FlashStrike());

		// Play the strike sound effect
		AudioManager.instance.PlaySFXNoVariation(strikeSFX, 1.0f, false);

		currentStrikes++;
	}

  public void RemoveStrike()
    {
        //A for loop that removes strikes from the game so that it continue once again
        for (int i = -4; i < currentStrikes; i++)
        {
            Image currentStrike = baseStrikes[currentStrikes - 1].GetComponent<Image>();
            currentStrike.sprite = unfilledStrike;
            currentStrikes--;
            Debug.Log("Removed Strike");
        }
    }

	// Flash the border around the screen to indicate a strike has occured
	private IEnumerator FlashStrike()
	{
		// Enable the gameobject (which lets the animation start playing)
		strikeFlash.gameObject.SetActive(true);	

		// Wait until the entire clip has played
		yield return new WaitForSeconds(strikeFlash_animClip.length);

		// Disable the gameobject to complete the cycle
		strikeFlash.gameObject.SetActive(false);
	}
		
	
	// Disconnect from delegate on destroy
	void OnDestroy()
	{
		FoodPassed.SignalStrike -= OnStrikeSignal;	
		StartCut.BadCutEvent -= OnStrikeSignal;
	}

    public void ContinueGame()
    {
        //Ends the game to send to ENDGAME scene
        AdButtons.SetActive(false);
        reviveadwatched = 2;
        Time.timeScale = 1;
        AdController.gameOverScreen = true;
        FoodPassed.SignalStrike -= OnStrikeSignal;
        EndGame(); // Tells Tip system to end the game
        EndGame -= SlideUIController.SlideOut;
    }

  public void ShowRewardedVideo()
    {
            HZIncentivizedAd.AdDisplayListener listener = delegate (string adState, string adTag)
            {
                if (adState.Equals("show"))
                {
                    // Sent when an ad has been displayed.
                    // This is a good place to pause your app, if applicable.
                    DeltaDNAGameEvents.instance.StartVideoReward(lastNetwork);
                }
                    // The user has watched the entire video and should be given a reward.
                if (adState.Equals("incentivized_result_complete"))
                {
                    // Run the reward code which restarts the game with current score
                    RemoveStrike();
                    reviveadwatched = 2;
                    AdButtons.SetActive(false);
                    Time.timeScale = 1;
      
                    // Report to DDNA that the ad completed
                    DeltaDNAGameEvents.instance.CompleteVideoReward(GLOBALS.AD_REWARD_COINS_AMOUNT, lastNetwork);
                }
                // The user did not watch the entire video and should not be given a reward.
                if (adState.Equals("incentivized_result_incomplete"))
                {
                    // TODO should anything occur if the player doesn't finish?
                }
                if (adState.Equals("audio_starting"))
                {
                    // The ad about to be shown will need audio.
                    // Mute any background music.
                    ToggleMusic(false);
                }
                if (adState.Equals("audio_finished"))
                {
                    // The ad being shown no longer needs audio.
                    // Any background music can be resumed.
                    ToggleMusic(true);
                }
            };

            // Setup the listener
            HZIncentivizedAd.SetDisplayListener(listener);

            // Show the actual ad
            HZIncentivizedAd.Show();
            // Fetch to prepare for the next showing
            HZIncentivizedAd.Fetch();
        }

private void ToggleMusic(bool isOn)
{
    if (isOn)
    {
        // Unmute audio
        AudioManager.instance.sfxSource.volume = 1;
        AudioManager.instance.noVariationSource.volume = 1;
        AudioManager.instance.musicSource.volume = 1;
    }
    else
    {
        // Mute audio
        AudioManager.instance.sfxSource.volume = 0;
        AudioManager.instance.noVariationSource.volume = 0;
        AudioManager.instance.musicSource.volume = 0;
    }
}

}

