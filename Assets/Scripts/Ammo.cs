using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    int spinValue = 50; //speed of the spin
    Player playerScript; //reference to the player

    bool isActive = true; //wether this ammo pack has been picked up

    public WeaponType weaponType; //the weapon type this ammo box is for

    public int ammoValue = 0; //how much ammo to replenish

    MeshRenderer [] childrenMR; //the child mesh renderers, so the mesh dissapears when picked up

    AudioSource audioSource; //audio player



    void playPickupSound()
    {
        //check if the sound is not playing
        if(audioSource.isPlaying == false)
        {
            //play the soundFX
            audioSource.Play();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //get a reference to the player's controller script
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        //get the child mesh renderers
        childrenMR = GetComponentsInChildren<MeshRenderer>();

        //Get the audio source
        audioSource = GetComponent<AudioSource>();

    }

    IEnumerator SelfDestruct()
    {
        //wait for a few seconds before destroying, so the sound FX has time to finish
        yield return new WaitForSeconds(2f);

        //destroy the ammo pack now it's been picked up
        Destroy(gameObject);

    }

    private void OnTriggerEnter(Collider other)
    {
        //check if the player walked into the ammo box and if the ammo box is active
        if (other.gameObject.tag == "Player" && isActive == true)
        {

            //check if the ammo for the weapon this ammo is for is already full
            if (playerScript.IsWeaponFull(weaponType) == false)
            {
                 
                //hide the ammo box by disabling all child mesh rendererd
                foreach (MeshRenderer MR in childrenMR )
                {
                    //disable the mesh renderer
                    MR.enabled = false;
                }

                //the ammo box has been picked up, set its active flag to fals
                isActive = false;

                //replenish the player's ammo for this ammo type
                playerScript.AmmoBoost(ammoValue,weaponType);

                //play sound effect
                playPickupSound();

                //kill this object after a few seconds so the soundFX can complete
                StartCoroutine(SelfDestruct());

            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //spin the ammo box around its Y axis
        transform.RotateAround(transform.position, Vector3.up, spinValue * Time.deltaTime);
    }
}
