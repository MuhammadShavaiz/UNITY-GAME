using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum gameStatus
{
    next, play, gameover, win
}

public class GameManager : Singleton<GameManager> {
    //SerializeField - Allows Inspector to get access to private fields.
    //If we want to get access to this from another class, we'll just need to make public getters
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private Text totalMoneyLabel;   //Refers to money label at upper left corner
    [SerializeField] private Text currentWaveLabel;
    [SerializeField] private Text totalEscapedLabel;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private Enemy[] enemies;
    [SerializeField] private int totalEnemies = 3;
    [SerializeField] private int enemiesPerSpawn;
    [SerializeField] private Text playButtonLabel;
    [SerializeField] private Text timeButtonLabel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button timeButton;

    private bool pausebutton = true;

    private int waveNumber = 0;
    private int totalMoney = 100;
    private int totalEscaped = 0;
    private int roundEscaped = 0;
    private int totalKilled = 0;
    private int whichEnemiesToSpawn = 0;
    private int enemiesToSpawn = 0;
    private gameStatus currentState = gameStatus.play;
    private AudioSource audioSource;

    private WaveManager waveManager;
    private TimeManager timeManager;

    public List<Enemy> EnemyList = new List<Enemy>();
    const float spawnDelay = 2f; //Spawn Delay in seconds

    public int TotalMoney
    {
        get { return totalMoney; }
        set
        {
            totalMoney = value;
            totalMoneyLabel.text = totalMoney.ToString();
        }
    }

    public int TotalEscape
    {
        get { return totalEscaped; }
        set { totalEscaped = value; }
    }
 
    public int RoundEscaped
    {
        get { return roundEscaped; }
        set { roundEscaped = value; }
    }

    public int TotalKilled
    {
        get { return totalKilled; }
        set { totalKilled = value; }
    }

    public AudioSource AudioSource
    {
        get { return audioSource; }
    }
    
    // Use this for initialization
    void Start () {
        Debug.Log("GameManager instance initialized");

        playButton.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();

        // Initialize the waveManager reference by finding the WaveManager component
        waveManager = FindObjectOfType<WaveManager>();
        timeManager = FindObjectOfType<TimeManager>();

        ShowMenu();
	}
	
	// Update is called once per frame
	private void Update () 
    {
        handleEscape();
        CheckWaveOver();
    }

    //This will spawn enemies, wait for the given spawnDelay then call itself again to spawn another enemy
    IEnumerator spawn()
    {
        if (enemiesPerSpawn > 0 && EnemyList.Count < totalEnemies)
        {
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (EnemyList.Count < totalEnemies)
                {
                    Enemy newEnemy = Instantiate(enemies[Random.Range(0, enemiesToSpawn)]);
                    newEnemy.transform.position = spawnPoint.transform.position;
                }
            }
            yield return new WaitForSeconds(spawnDelay);
            StartCoroutine(spawn());
        }
    }

    ///Register - when enemy spawns
    public void RegisterEnemy(Enemy enemy)
    {
        EnemyList.Add(enemy);
    }

    ///Unregister - When they escape the screen
    public void UnregisterEnemy(Enemy enemy)
    {
        EnemyList.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    ///Destroy - At the end of the wave
    public void DestroyAllEnemies()
    {
        foreach(Enemy enemy in EnemyList)
        {
            Destroy(enemy.gameObject);
        }
        EnemyList.Clear();
    }

    public void AddMoney(int amount)
    {
        TotalMoney += amount;
    }

    public void SubtractMoney(int amount)
    {
        TotalMoney -= amount;
    }

    public void isWaveOver()
    {
        totalEscapedLabel.text = "Escaped " + TotalEscape + "/10";
        if (RoundEscaped + TotalKilled == totalEnemies)
        {
            if(waveNumber <= enemies.Length)
            {
                enemiesToSpawn = waveNumber;
            }
            setCurrentGameState();
            ShowMenu();
        }
    }

    public void CheckWaveOver()
    {
        if (RoundEscaped + TotalKilled == totalEnemies)
        {
            if (currentState == gameStatus.next && waveNumber < totalWaves)
            {
                //currentState = gameStatus.next;
                ShowMenu();
            }
            else if (currentState == gameStatus.win)
            {
                //currentState = gameStatus.win;
                ShowMenu();
            }
            //ShowMenu();
        }
    }

    public Enemy FindClosestEnemyToPosition(Vector3 position)
    {
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in EnemyList)
        {
            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    //getter function
    public Enemy[] GetEnemies()
    {
        return enemies;
    }

    //getter function
    public Transform GetSpawnPoint()
    {
        return spawnPoint.transform;
    }

    public void setCurrentGameState()
    {
        if(totalEscaped >= 10)
        {
            currentState = gameStatus.gameover;
        }
        else if(waveNumber == 0 && (TotalKilled + RoundEscaped) == 0)
        {
            currentState = gameStatus.play;
        }
        else if(waveNumber >= totalWaves)
        {
            currentState = gameStatus.win;
        }
        else
        {
            currentState = gameStatus.next;
        }
    }

    public void ShowMenu()
    {
        switch (currentState)
        {
            case gameStatus.gameover:
                playButtonLabel.text = "Play Again!";
                AudioSource.PlayOneShot(SoundManager.Instance.Gameover);
                break;
            case gameStatus.next:
                playButtonLabel.text = "Next Wave";
                break;
            case gameStatus.play:
                playButtonLabel.text = "Play";
                break;
            case gameStatus.win:
                playButtonLabel.text = "Play";
                break;
        }
        playButton.gameObject.SetActive(true);
    }

    public void playButtonPressed()
    {
        Debug.Log("Play Button Pressed");

        if (currentState == gameStatus.next || currentState == gameStatus.play)
        {
            waveManager.StartWave();
        }
        else if (currentState == gameStatus.win || currentState == gameStatus.gameover)
        {
            // Restart or start a new game here
            // Depending on your game logic
            GameManager.Instance.RestartOrStartGame();
        }

        //waveManager.StartWave();
    }
    public void timeButtonPressed()
    {
        Debug.Log("time button pressed");
        if (pausebutton == true)
        {
            timeManager.PauseTime();
            //timeButtonLabel.text = "Resume";
            pausebutton = false;
        }
        else
        {
            timeManager.ResumeTime();
            //timeButtonLabel.text = "Pause";
            pausebutton = true;
        }
        
    }
    public void rewindButtonPressed()
    {
        Debug.Log("rewind button pressed");
        RestartGame();

    }

    public void RestartOrStartGame()
    {
        switch (currentState)
        {
            case gameStatus.gameover:
            case gameStatus.win:
                RestartGame();
                break;
            case gameStatus.play:
            case gameStatus.next:
                waveManager.StartWave();
                break;
        }
    }

    public void RestartGame()
    {
        // Reset all relevant game variables and states to their initial values
        // For example:
        waveNumber = 0;
        totalMoney = 100;
        totalEscaped = 0;
        roundEscaped = 0;
        totalKilled = 0;
        currentState = gameStatus.play;

        // Clear enemy list and destroy remaining enemies
        DestroyAllEnemies();

        // Hide the play button and start the wave
        //playButton.gameObject.SetActive(false);
        waveManager.StartWave();
    }

    private void handleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TowerManager.Instance.disableDragSprite();
            TowerManager.Instance.towerButtonPressed = null;
        }
    }
}
