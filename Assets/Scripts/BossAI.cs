using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    //boss state machine
    enum BOSS_STATE
    {
        IDLE,
        STOMP,
        THROW,
        WAITING,
        DEAD
    }

    public float distanceForStomp = 5;//how close the player has to be for the stomp effect
    public float distanceForProjectileThrow = 10; //how close the player has to be for a projectile throw

    public float timeBetweenStomps = 5; //how many seconds to wait before another stomp can be intiated (if in range)
    public float timeBetweenProjectileThrows = 5; //how many seconds to wait before another throw can be initiated (if in range)

    public int damageForStomp = 20; //how much player damage for stomp
    public int damageForProjectile = 30; //how much damage for the projectile

    public int shockWaveAnimationTimeOffset = 1; //time offset of playing the particle system effect whn stomp animation is playing
    public float projectileThrowAnimationTimeOffset = 1.5f; //time offset of when the projectile leaves the bosse's hand from the throw animation

    public AudioClip stompSoundFX; //audio clip for the stomp effect
    public AudioClip projectileSoundFX; //audio clip for the projectile effect

    Animator bossAnimator; //reference to the boss Animator component
    ParticleSystem shockwavePS; //reference to the shockwave particle effect
    float distanceToPlayer; //distance between player and boss
    BOSS_STATE state = BOSS_STATE.IDLE; //the current state of the boss
    float countdownTimer = 0; //seconds countdown for timeBetween stomps
    GameObject player; //reference to the player
    Player playerScript; //refernce to the player script to call public functions
    AudioSource audioSource; //reference to the bosses audio source

    GameObject projectile; //reference to the projectile game object
    Rigidbody projectileRB; //reference to the rigid body of the projectile

    Vector3 projectileOrigin;

    void PlayShockwaveSound()
    {
        //check if sound is already playing
        if (audioSource.isPlaying == false)
        {
            //set the sound effect clip TODO: should be a parameter
            audioSource.clip = stompSoundFX;
            
            //play the soun effect
            audioSource.Play();
        }
    }

    void PlayProjectileSound()
    {
        //check if sound is already playing
        if (audioSource.isPlaying == false)
        {
            //set the sound effect clip TODO: should be a parameter
            audioSource.clip = projectileSoundFX;
            
            //play the soun effect
            audioSource.Play();
        }
    }

    void CheckForDamageFromShockwave()
    {
        //cehck if the player is on the ground when shockwave occurs
        if (playerScript.TouchingGroundCheck() == true)
        {
            //the player is on the ground, damage the player from the shockwave 
            playerScript.Hit(damageForStomp);
        }
    }
    IEnumerator StompShockwave()
    {
        //start the stomp animation
        bossAnimator.SetTrigger("Stomp");

        //wait until the boss hits the ground again after jumping
        yield return new WaitForSeconds(shockWaveAnimationTimeOffset);

        //play the shockwave particle effect
        shockwavePS.Play();

        //check if the player recieves damage for the shockwave
        CheckForDamageFromShockwave();

        //play the shockwave sound effect 
        PlayShockwaveSound();
    }

    IEnumerator ThrowProjectile()
    {
        //set the animation triger
        bossAnimator.SetTrigger("Throw");
        
        //wait for a period before the projectile leaves the hand of the boss
        yield return new WaitForSeconds(projectileThrowAnimationTimeOffset);

        //let the physics engine control the object
        projectileRB.isKinematic = false;
        
        //break the parent child relationship
        projectile.transform.parent = null;
        
        //make the projectile fzce the player
        projectile.transform.LookAt(player.transform);
        
        //throw the projectile
        projectileRB.AddForce(projectileRB.transform.forward * 5000);
    }

    void ResetProjectile()
    {
        //re-create the parent child relationship
        projectile.transform.parent = transform;
        
        //the projectile is now controlled by the script
        projectileRB.isKinematic = true;
        
        //reset the position of the projectile to be under the hand of the boss character
        projectile.transform.localPosition = new Vector3(projectileOrigin.x, projectileOrigin.y, projectileOrigin.z);
        
        //re-enable the coliisions
        projectileRB.detectCollisions = true;
    }

    public void ProjectileCollision(Collision other)
    {
        //check if we hit the player
        if (other.transform.tag == "Player")
        {
            //check if collisions are enabled otherwise the collision gets detected twice
            if (projectileRB.detectCollisions == true)
            {
                //damage the player
                playerScript.Hit(damageForProjectile);
                
                //play the sound fx
                PlayProjectileSound();
                
                //disbale collisions on the projectile otherwise the collision gets detected twice
                projectileRB.detectCollisions = false;
            }
        }
    }

    void UpdateState()
    {
        //get the distance to the player
        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        switch (state)
        {
            case BOSS_STATE.IDLE:
                {
                    //make sure the boss faces the player
                    transform.LookAt(player.transform);

                    //chek if we are in range for stomp
                    if (distanceToPlayer < distanceForStomp)
                    {
                        //initiate stomp move
                        state = BOSS_STATE.STOMP;
                    }
                    //check if we are close enough to throw a projectile
                    else if (distanceToPlayer < distanceForProjectileThrow)
                    {
                        //transition into the throwing projectile effect
                        state = BOSS_STATE.THROW;
                    }
                }
                break;

            case BOSS_STATE.THROW:
                {
                    //set the countdown timer
                    countdownTimer = timeBetweenProjectileThrows;
                    
                    //move the projectile under the hand of the boss
                    ResetProjectile();
                    
                    //throw the projectile
                    StartCoroutine(ThrowProjectile());
                    
                    //go into the waiting state
                    state = BOSS_STATE.WAITING;
                }
                break;

            case BOSS_STATE.STOMP:
                {
                    //start stomp effect, set the countdown timer
                    countdownTimer = timeBetweenStomps;

                    //play the shockwave animation
                    StartCoroutine(StompShockwave());

                    //we have started a stomp move, wait for it to finish
                    state = BOSS_STATE.WAITING;
                }
                break;
            case BOSS_STATE.WAITING:
                {
                    //update the countdown timer
                    countdownTimer -= Time.deltaTime;

                    //check of the timer has reached 0
                    if (countdownTimer <= 0)
                    {
                        //waiting time is over, reset to the idle state
                        state = BOSS_STATE.IDLE;
                    }
                }
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //get component references
        bossAnimator = GetComponent<Animator>();
        shockwavePS = transform.Find("ShockWave").GetChild(0).GetComponent<ParticleSystem>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
        projectile = transform.Find("Projectile").gameObject;
        projectileRB = projectile.GetComponent<Rigidbody>();
        
        //store the origin of the projectile, under the hand of the boss
        projectileOrigin = new Vector3(projectile.transform.localPosition.x,
                                        projectile.transform.localPosition.y,
                                        projectile.transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        //update state machine
        UpdateState();
    }
}
