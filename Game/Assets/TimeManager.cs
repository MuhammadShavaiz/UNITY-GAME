using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] bool isTimePaused = false;
    float timeScale = 1.0f;
    [SerializeField] bool isRewinding = false;
    List<RecordSnapshot> snapshots = new List<RecordSnapshot>();

    public float rewindDuration = 5.0f; // Duration of rewind in seconds
    public float temporalCessationDuration = 3.0f; // Duration of temporal cessation in seconds
    private int maxSnapshots = 10;

    private Tower tower;
    private GameManager gameManager;

    public void PauseTime()
    {
        isTimePaused = true;
        Time.timeScale = 0.0f;
    }

    public void ResumeTime()
    {
        Debug.Log("Resume Time");
        isTimePaused = false;
        Time.timeScale = timeScale;
    }

    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
        Time.timeScale = isTimePaused ? 0.0f : timeScale;
    }

    // Start is called before the first frame update
    public void StartRewind()
    {
        if (!isRewinding)
        {
            isRewinding = true;
            snapshots.Clear(); // Clear previous snapshots
            StartCoroutine(RewindCoroutine());
        }
    }

    private IEnumerator RewindCoroutine()
    {
        float startTime = Time.time;
        float elapsedTime = 0.0f;

        while (elapsedTime < rewindDuration && snapshots.Count > 0)
        {
            elapsedTime = Time.time - startTime;
            RecordSnapshot snapshot = snapshots[snapshots.Count - 1];
            snapshots.RemoveAt(snapshots.Count - 1);

            // Interpolate and update game state using snapshot data
            foreach (var enemySnapshot in snapshot.enemySnapshots)
            {
                Enemy enemy = enemySnapshot.enemy;
                Vector3 interpolatedPosition = Vector3.Lerp(enemy.transform.position, enemySnapshot.position, elapsedTime / rewindDuration);
                enemy.transform.position = interpolatedPosition;
            }

            yield return null;
        }

        isRewinding = false;
    }

    public void TemporalCessation()
    {
        StartCoroutine(TemporalCessationCoroutine());
    }

    private IEnumerator TemporalCessationCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.5f; // Slow down time during temporal cessation

        yield return new WaitForSeconds(temporalCessationDuration);

        Time.timeScale = originalTimeScale;
    }

    public void StopRewind()
    {
        isRewinding = false;
    }

    public void SetTower(Tower tower)
    {
        this.tower = tower;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isRewinding)
        {
            RewindGame();
        }
        else if (tower != null) // Make sure tower reference is not null
        {
            recordSnapshot(tower); // Store the current game state
        }
    }

    private void RewindGame()
    {
        if (snapshots.Count > 0)
        {
            var snapshot = snapshots[snapshots.Count - 1];
            var interpolationFactor = (Time.time - snapshot.timestamp) / rewindDuration;
            //RecordSnapshot snapshot = snapshots[snapshots.Count - 1];

            // Update tower position
            tower.transform.position = snapshot.position;

            // Update enemy positions and health
            // Interpolate and update game state using snapshot data
            foreach (var enemySnapshot in snapshot.enemySnapshots)
            {
                var enemy = enemySnapshot.enemy;
                var interpolatedPosition = Vector3.Lerp(enemy.transform.position, enemySnapshot.position, interpolationFactor);
                enemy.transform.position = interpolatedPosition;
            }

            // Remove the used snapshot from the list
            snapshots.RemoveAt(snapshots.Count - 1);
        }
        else
        {
            StopRewind();
        }
    }

    public void recordSnapshot(Tower tower)
    {
        // Create a new snapshot using the tower's current state
        RecordSnapshot snapshot = new RecordSnapshot(Time.time, tower.transform.position);

        foreach (Enemy enemy in GameManager.Instance.EnemyList)
        {
            snapshot.enemySnapshots.Add(new EnemySnapshot(enemy, enemy.transform.position, enemy.HealthPoints));
        }

        // Add the snapshot to the snapshots list
        snapshots.Add(snapshot);

        // Limit the number of snapshots to a certain maximum count
        if (snapshots.Count > maxSnapshots)
        {
            snapshots.RemoveAt(0); // Remove the oldest snapshot if the list exceeds the limit
        }
    }

    //[System.Serializable]
    //public struct GameSnapshot
    //{
    //    public float timestamp; // The timestamp when the snapshot was taken
    //    public List<EnemySnapshot> enemySnapshots; // Snapshots of enemy states
                                                   // Add more snapshot data as needed for your game

    //    public GameSnapshot(float timestamp)
    //    {
    //        this.timestamp = timestamp;
    //        enemySnapshots = new List<EnemySnapshot>();
    //    }
    //}

    [System.Serializable]
    public struct EnemySnapshot
    {
        public Enemy enemy; // Reference to the enemy
        public Vector3 position; // Position of the enemy
        public int health; // Health of the enemy

        public EnemySnapshot(Enemy enemy, Vector3 position, int health)
        {
            this.enemy = enemy;
            this.position = position;
            this.health = health;
        }
    }

    [System.Serializable] // This attribute makes the struct visible in the Unity Inspector
    public struct RecordSnapshot
    {
        public float timestamp; // Time at which the snapshot was taken
        public Vector3 position; // Position of the tower at the time of the snapshot
        public List<EnemySnapshot> enemySnapshots; // Snapshots of enemy states

        public RecordSnapshot(float timestamp, Vector3 position)
        {
            this.timestamp = timestamp;
            this.position = position;
            this.enemySnapshots = new List<EnemySnapshot>();
        }
    }

}
