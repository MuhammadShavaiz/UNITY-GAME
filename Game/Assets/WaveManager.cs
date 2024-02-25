using System;
using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private float timeBetweenWaves = 10f; // Time between each wave
    [SerializeField] private int initialEnemiesPerWave = 1; // Initial number of enemies per wave
    [SerializeField] private int enemiesPerWaveIncrement = 2; // Increment in enemies per wave
    [SerializeField] private float spawnDelay = 1.5f; // Delay between spawning each enemy

    private int currentWave = 0;
    private bool isWaveActive = false;

    // Reference to the GameManager
    private GameManager gameManager;

    // Start is called before the first frame update
    public void StartWave()
    {
        gameManager = FindObjectOfType<GameManager>();

        Debug.Log("Start Wave");

        if (gameManager == null)
        {
            Debug.LogError("GameManager.Instance is nullified!");
            return;
        }

        //if (gameManager != null)
        //{
        //    StartCoroutine(StartWaveCoroutine());
        //}
        //else
        //{
        //    Debug.LogError("GameManager is not assigned!");
        //}

        StartCoroutine(StartWaveCoroutine());

        Debug.Log("Start Coroutine");
    }

    private IEnumerator StartWaveCoroutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        while (true)
        {
            currentWave++;
            isWaveActive = true;

            int enemiesToSpawn = initialEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrement;

            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
            
            //for (int i = 0; i < enemiesToSpawn; i++)
            //{
            //    SpawnEnemy();
            //    yield return new WaitForSeconds(spawnDelay);
            //}

            Debug.Log("What is happening here?");

            //while (gameManager.EnemyList.Count > 0)
            //{
            //    yield return null;
            //    Debug.Log("Inside here!");
            //}

            isWaveActive = false;
            gameManager.isWaveOver();
            Debug.Log("First Wave Over");

            yield return new WaitForSeconds(timeBetweenWaves);
            Debug.Log("Waiting for next wave");
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("Spawning enemies");

        // Get the array of enemies from the GameManager
        Enemy[] enemyArray = GameManager.Instance.GetEnemies();
        Debug.Log("Enemy array initialized");

        // Choose a random enemy from the array
        Enemy enemyPrefab = enemyArray[UnityEngine.Random.Range(0, enemyArray.Length)];
        Debug.Log("Enemy chosen");

        // Get the spawn point from the GameManager
        Transform spawnPoint = GameManager.Instance.GetSpawnPoint();
        Debug.Log("Spawn point received");

        // Spawn the enemy at the spawn point
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log("Enemy spawned at spawn point");

    }

    public void TowerAttackedEnemy(Enemy enemy, int attackStrength)
    {
        if (enemy != null)
        {
            enemy.enemyHit(attackStrength);

            // Check if the enemy has been defeated
            if (enemy.IsDead)
            {
                // Increase the player's score
                GameManager.Instance.TotalKilled += 1;
                Debug.Log("Enemy defeated by tower!");

                // Grant rewards to the player (increase money, etc.)
                int rewardAmount = enemy.RewardAmount;
                GameManager.Instance.AddMoney(rewardAmount);

                // Play victory sound effect
                //GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Victory);

                // Show a victory message or animation
                // For example, you could display a "Victory!" message on the screen for a few seconds.
                //UIManager.Instance.ShowVictoryMessage();

                // Unregister the defeated enemy from the GameManager
                GameManager.Instance.UnregisterEnemy(enemy);

                // Check if the wave is over
                gameManager.isWaveOver();
            }
        }
    }

    public bool IsWaveActive()
    {
        return isWaveActive;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

}
