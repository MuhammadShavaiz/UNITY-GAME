using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgeType
{
    Stone,
    Bronze,
    Iron,
    Middle,
    Modern,
    Cyber
}

public class Enemy : MonoBehaviour {
    //SerializeField - Allows Inspector to get access to private fields.
    //If we want to get access to this from another class, we'll just need to make public getters
    [SerializeField] private AgeType ageType;
    [SerializeField] private int healthPoints;
    [SerializeField] private int rewardAmount;

    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float navigationUpdate;

    private int target = 0;
    private Transform enemy;
    private Collider2D enemyCollider;
    private Animator anim;
    private float navigationTime = 0;
    private bool isDead = false;

    public bool IsDead => isDead;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null in Enemy Awake");
        }
    }

    // Use this for initialization
    private void Start () {
        enemy = transform;
        enemyCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        GameManager.Instance.RegisterEnemy(this);
    }
	
	// Update is called once per frame
	private void Update () {
		if (wayPoints != null && !isDead)
        {
            //Lets use change how fast the update occurs
            navigationTime += Time.deltaTime;
            if(navigationTime > navigationUpdate)
            {
                //If enemy is not at the last wayPoint, keep moving towards the wayPoint
                //otherwise move to the exitPoint
                if(target < wayPoints.Length)
                {
                    enemy.position = Vector2.MoveTowards(enemy.position, wayPoints[target].position, navigationTime);
                }
                else
                {
                    enemy.position = Vector2.MoveTowards(enemy.position, exitPoint.position, navigationTime);
                }
                navigationTime = 0;
            }
        }
	}

    //getter function
    public int RewardAmount
    {
        get { return rewardAmount; }
    }

    //getter function
    public int HealthPoints
    {
        get { return healthPoints; }
    }

    //If we trigger the collider2D.tag for checkpoints for finish. 
    //If it hits the checkpoints, increase the index and move to the next checkpoint
    //otherwise enemy is at the finish line and should be destroyed.
    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (GameManager.Instance == null)
        {
            // Handle the case where GameManager.Instance is not yet initialized
            Debug.LogError("GameManager.Instance is null in Enemy OnTriggerEnter2D");
            return;
        }

        if (collider2D.tag == "checkpoint")
        {
            target += 1;
        }  
        else if (collider2D.tag == "Finish")
        {
            GameManager.Instance.RoundEscaped += 1;
            GameManager.Instance.TotalEscape += 1;
            GameManager.Instance.UnregisterEnemy(this);
            GameManager.Instance.CheckWaveOver();
        }
        else if(collider2D.tag == "projectile")
        {
            Projectile newP = collider2D.gameObject.GetComponent<Projectile>();
            enemyHit(newP.AttackStrength);
            Destroy(collider2D.gameObject);
        }
    }

    public void enemyHit(int hitPoints)
    {
        if(healthPoints - hitPoints > 0)
        {
            healthPoints -= hitPoints;
            anim.Play("Hurt");
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Hit);
        }
        else
        {
            anim.SetTrigger("didDie");
            die();
        }
    }

    public void die()
    {
        isDead = true;
        enemyCollider.enabled = false;
        GameManager.Instance.TotalKilled += 1;
        GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Death);
        GameManager.Instance.AddMoney(rewardAmount);
        GameManager.Instance.CheckWaveOver();

        // Unregister this enemy from the GameManager
        GameManager.Instance.UnregisterEnemy(this);
    }
}
