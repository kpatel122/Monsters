using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**************************************************************************
DEVELOPMENT BRANCH
**************************************************************************/



[System.Serializable]
public enum WeaponType //the different types of weapons available in the game
{
    SINGLE_SHOT = 0, //one bullet lost per fire
    MACHINE_GUN = 1  //multiple bullets lost per fire
}

[System.Serializable]
public class WeaponAmmoData //weapon info
{
    public int capacticty; //maximum bullets this weapon can hold
    public int ammoValue; //how many bullets to replenish
    public int damageValue; //how much damage is done to the enemy when hit
    public WeaponType type; //the weapon type
    public string weaponNameUI; //the UI name to sidplay for this weapon
    public int numberOfAmmoLostForSingleFire;//how many bullets are lost when a single fire event has occured
}

public class Player : MonoBehaviour
{
    const int MAX_HEALTH = 100; //the health capacity

    public Camera mainCamera; //main player camera for raycasts

    public GameObject damageImageObject; //the damage image game object

    Image damageImage; //the image component part of damage image's game object

    public ProgressBar healthBar; //reference to the health bar UI

    int health = MAX_HEALTH; //initial health

    bool isHit = false; //flag variable for damage image fade effect

    public int gunDamage = 10; //amount of damage done by the gun

    public TextMeshProUGUI weaponInfo; //the UI info to display for the weapon

    public WeaponAmmoData[] weapons; //all weapons available for the game

    int currentWeaponIndex = 0; //the index into the weapon array which represents the current weapon

    WeaponAmmoData currentWeapon;  //holds the current weapon

    void weaponChange()
    {
        //validate the weapon index to roll back around the weapons array
        if (currentWeaponIndex >= weapons.Length)
        {
            //move back to the first weapon
            currentWeaponIndex = 0;
        }
        else if (currentWeaponIndex < 0)
        {
            //move to the last weapon 
            currentWeaponIndex = weapons.Length - 1;
        }

        //update the current weapon
        currentWeapon = weapons[currentWeaponIndex];

        //update the GUI to show the new weapon info
        UpdateWeaponInfoUI();
    }

    public WeaponType getCurrentWeaponType()
    {
        //cut the current weapon type
        return (WeaponType)currentWeaponIndex;
    }

    void UpdateWeaponInfoUI()
    {
        //show the weapon stats on the UI
        weaponInfo.text = currentWeapon.weaponNameUI + ": " + currentWeapon.ammoValue + "/" + currentWeapon.capacticty;
    }

    // Start is called before the first frame update
    void Start()
    {
        //set the initial health bar value
        healthBar.BarValue = health;

        //get the image component
        damageImage = damageImageObject.GetComponent<Image>();

        //set the current current weapon
        currentWeapon = weapons[currentWeaponIndex];

        //set the weapon ui info
        UpdateWeaponInfoUI();
    }

    public void AmmoBoost(int ammoValue, WeaponType type)
    {
        
        //get the weapon to boost ammo on
        WeaponAmmoData weaponToReplenish = weapons[(int)type];

        //increase the ammo
        weaponToReplenish.ammoValue += ammoValue;

        //check if the ammo boost exceeds the capacity of the weapon
        if (weaponToReplenish.ammoValue > weaponToReplenish.capacticty)
        {
            //clamp the ammo to the max capacity 
            weaponToReplenish.ammoValue = weaponToReplenish.capacticty;
        }

        //update the UI with the new weapon ammo update
        UpdateWeaponInfoUI();
    }

    public bool IsHealthFull()
    {
        //return if the health is at capacity
        return (health >= 100);
    }

    public bool IsWeaponFull(WeaponType type)
    {
        //return if the weapon's ammo is at capacity
        return (currentWeapon.ammoValue >= currentWeapon.capacticty);
    }

    public bool IsWeaponEmpty()
    {
        //return if the weapon's ammo is empty
        return (currentWeapon.ammoValue == 0);
    }

    public void HealthBoost(int healValue)
    {
        //replenish the playe's health by the heal value
        health += healValue;
        
        //check if we exceed the maximum health
        if (health > MAX_HEALTH)
        {
            //clamp the health to the maximum value 
            health = MAX_HEALTH;
        }

        //update the health UI
        healthBar.BarValue = health;

    }

    public void Hit(int damageValue)
    {
        //set the alpha value of damage image to 1 i.e. completly visible
        var colour = damageImage.color;
        colour.a = 1f;
        damageImage.color = colour;

        //set the hit flag for the fade out effect of the damage image 
        isHit = true;

        //decrease the health
        health -= damageValue;

        //update the health bar value
        healthBar.BarValue = health;

    }

    void UpdateAmmo()
    {
        //the weapon has been fired, remove ammo value from weapon
        currentWeapon.ammoValue -= currentWeapon.numberOfAmmoLostForSingleFire;
        
        //check if the weapon is empty
        if (currentWeapon.ammoValue < 0)
        {
            //clamp the ammo to zero
            currentWeapon.ammoValue = 0;
        }

        //update the weapon UI    
        UpdateWeaponInfoUI();
    }

    void Shoot()
    {
        //stote the result of the raycast
        RaycastHit hit;

        //fire ray from middle of the screen 
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit))
        {
            //check if we hit a monster
            if (hit.transform.tag == "Monster")
            {
                //monster hit call monster's hot function
                hit.transform.gameObject.GetComponent<SimpleEnemyAI>().Hit(gunDamage);
            }
        }

        //update the ammo info
        UpdateAmmo();


    }

    // Update is called once per frame
    void Update()
    {
        //check if the player has been hit
        if (isHit)
        {
            //check if the image is still being displayed
            if (damageImage.color.a > 0)
            {
                //decrease the image alpha value, i.e. fade out
                var colour = damageImage.color;
                colour.a -= 0.01f;
                damageImage.color = colour;
            }
            else
            {
                //image has completly faded out, hit visual effect is complete
                isHit = false;
            }
        }

        //check for player shooting
        if (Input.GetButtonDown("Fire1"))
        {
            //check if we have ammo for a fire
            if (IsWeaponEmpty() == false)
            {
                //fire the weapon
                Shoot();
            }

        }

        //weapon change occurs on key up and key down
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //move the current weapon index, validation of the index occurs in the weaponChange() function
            currentWeaponIndex++;

            //process the weapon change
            weaponChange();

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //move the current weapon index, validation of the index occurs in the weaponChange() function
            currentWeaponIndex--;
            
            //process the weapon change
            weaponChange();

        }

    }
}
