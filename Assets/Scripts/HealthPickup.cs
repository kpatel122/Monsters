using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    // Start is called before the first frame update
    Player playerScipt;  //reference to the player
    public int spinSpeed =50; //speed of the spin 

    public int healValue = 10; //how many health points to give to the player
    bool isActive = true; //wether this ammo pack has been picked up

    AudioSource audioSource;//the pickup audio clip

    
    void playPickupSound()
    {
        //check if a sound is not already playing
        if(audioSource.isPlaying == false)
        {
            //play the pickup sound
            audioSource.Play();
        }
    }

    void Start()
    {
         //get a reference to the player's controller script
        playerScipt = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        //spin the health box around its Y axis
        transform.RotateAround(transform.position, Vector3.up, spinSpeed * Time.deltaTime);
        
    }

    IEnumerator SelfDestruct()
    {
        //wait for a few seconds before destroying, so the sound FX has time to finish
        yield return new WaitForSeconds(2f);

         //destroy the ammo pack now it's been picked up
        Destroy(transform.parent.gameObject);

    }

    private void OnTriggerEnter(Collider other) 
    {
        //check if the player has walked into the health pack and the health pack is active and the player's health isn't already full
        if(other.gameObject.tag == "Player" && isActive == true && playerScipt.IsHealthFull() == false)
        {
            //replenish the player's health
            playerScipt.HealthBoost(healValue);

            //hide the mesh by disabling its mesh renderer component
            transform.GetComponent<MeshRenderer>().enabled = false;
            
            //set the active flag to false
            isActive = false;

            //play soundFX
            playPickupSound();

            //wait a couple of seconds so the soundFX can complete, and then destroy the health pack
            StartCoroutine(SelfDestruct());
        }
        
    }
}
