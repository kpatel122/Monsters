using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;
using UnityEngine.Assertions;
using UnityEngine.AI;

/**************************************************************************
DEVELOPMENT BRANCH
**************************************************************************/

public class SimpleEnemyAI : MonoBehaviour
{
    
    enum MONSTER_STATE
    {
        PATROLLING, //following waypoints
        TRACKING,  //chasing player
        ATTACKING,  //attacking player
        WAITING,   //waiting for attack frequency to pass
        DEAD
    };

    MONSTER_STATE state; //current state of the enemy
    public Transform player; //treference to the player

    NavMeshAgent navMeshAgent; //reference to the navmesh agent
    WaypointsTraveler waypointsScript; //reference to waypoint script
    public float distanceForTracking; //inside this distance the enemy can begin to track
    public float distanceForAttacking; //inside this distance the enemy can attack
    
    public int distanceToHearPatrolSounds = 20; //how close the player has to be to hear the idle sounds
    
    public float attackFrequency = 2; //time between attacks as long as enemy is inside distanceForAttacking
    public int damageForSwing = 10; //damage done to the player when monster attacks using arm swing
    
    public string[] fallTriggers; //the different animation triggers for when the monster dies

    public AudioClip[] patrolSounds; //patrol sound bank
    public int patrolSoundRandomMaxValue = 100; //the chances of playing the sound, if in range
    int numberOfPatrolSounds; //the number of patrol sounds

    public AudioClip[] attackSounds; //attack sound bank
    int numberOfAttackSounds; //the number of attack sounds

    private AudioSource audioSource; //the audio player
    
    private float timer = 0.0f; //internal counter
    private float timesinceLastAttack = 0; //time since enemy attacked player
    Animator enemyAnimator; //reference to the Animator component of the enemy
    private float distanceToPlayer; //distance to the player

    /**************************************************************************
    DEVELOPMENT BRANCH
    **************************************************************************/ 

    public void Hit(int damageValue)
    {
        //generate a random number between 0 and number of fall triggers
        int randomFall = Random.Range(0, fallTriggers.Length);

        //get the trigger name based on the random index
        string triggerName = fallTriggers[randomFall];

        //set the random fall animation
        enemyAnimator.SetTrigger(triggerName);

        //disable AI components
        waypointsScript.enabled = false;
        navMeshAgent.isStopped = true;

        //prevent any further AI calculations
        state = MONSTER_STATE.DEAD;
    }

    
    void playPatrolSound()
    {
        //get a random patrol sound
        audioSource.clip = patrolSounds[Random.Range(0,numberOfPatrolSounds)];
        
        //play the sound
        audioSource.Play();
    }
    void playAttackSound()
    {
        //get a random attack sound
        audioSource.clip = attackSounds[Random.Range(0,numberOfAttackSounds)];
        
        //play the sound
        audioSource.Play();
    }

    void Initialise()
    {
        //set initial state
        state = MONSTER_STATE.PATROLLING;
        
        //get attached components
        waypointsScript = GetComponent<WaypointsTraveler>();
        enemyAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        //retrieve the audio player
        audioSource = GetComponent<AudioSource>();

        //get the number of the patrol and attack sound banks
        numberOfPatrolSounds = patrolSounds.Length;
        numberOfAttackSounds = attackSounds.Length;
    }

    void UpdateState()
    {
        //get the distance to the player
        distanceToPlayer = Vector3.Distance(player.position, transform.position);
        //Debug.Log("Distance to player is " + distanceToPlayer + " distanceToHearPatrolSounds " + distanceToHearPatrolSounds + " Monster state is " + state );
        
        if(state == MONSTER_STATE.PATROLLING)
        {
            //check if we are close enough to chase the player
            if(distanceToPlayer < distanceForTracking)
            {
                //we are close enough break off waypoints and chase the player
                state = MONSTER_STATE.TRACKING;
                
                //stop following waypoints, follow player instead
                waypointsScript.enabled = false;
                
                //transition to run animation
                enemyAnimator.SetTrigger("RunTrigger");
            }
            //check if we are in range for the player to hear a patrol sound
            else if(distanceToPlayer < distanceToHearPatrolSounds)
            {
                //check if no ther sounds are playing
                if(audioSource.isPlaying == false)
                {
                    //randomise whether we hear the sound effect
                    if(Random.Range(0,patrolSoundRandomMaxValue) == 1)
                    {
                        //play the patrol sound
                        playPatrolSound();
                    }
                    
                }
                
            }
        }
        if(state == MONSTER_STATE.TRACKING)
        {
            //we are now chasing the plater
            navMeshAgent.SetDestination(player.position);

            //check if we are close enough to launch an attack
            if(distanceToPlayer < distanceForAttacking)
            {
                //attack the payer
                state = MONSTER_STATE.ATTACKING;
            }
        }
        if(state == MONSTER_STATE.ATTACKING)
        {
            //continue to follow the player
            navMeshAgent.SetDestination(player.position);

            //transition to attack animation
            enemyAnimator.SetTrigger("AttackTrigger");

            //call player hit
            player.GetComponent<Player>().Hit(damageForSwing);

            if(audioSource.isPlaying == false)
            {
                playAttackSound();
            }

            //check if we are still in range for an attack
            if(distanceToPlayer >= distanceForAttacking)
            {
                //we are not in range go back to the tracking state
                state = MONSTER_STATE.TRACKING;
                enemyAnimator.SetTrigger("RunTrigger");
            }
            else
            {
                //wait until attackFrequency has been reached to decide wether or not to attack again
                state = MONSTER_STATE.WAITING;

                //track how much time has passed since the last attack
                timer = Time.deltaTime;
            }

        }
        if( state == MONSTER_STATE.WAITING )
        {
            //increment the timer
            timer += Time.deltaTime;

            //check if attackFrequency time has eleapsed
            if(timer > attackFrequency)
            {
                //the required amaount of time has passed, check whether to attack again
                if(distanceToPlayer < distanceForAttacking)
                {
                    //we are close enough to launch another attack
                    state = MONSTER_STATE.ATTACKING;
                }
                else
                {
                    //the player is to far away to attack, go back into tracking
                    state = MONSTER_STATE.TRACKING;
                    //transition to run animation
                    enemyAnimator.SetTrigger("RunTrigger");
                }
            }
        }

    }
    
    // Start is called before the first frame update
    void Start()
    {
        Initialise(); 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
    }
}
