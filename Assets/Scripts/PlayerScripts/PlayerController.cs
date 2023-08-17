using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    //private float distance = 10;

    private SpriteRenderer playerSprite;

    //COMPONENT GRAB
    private Rigidbody2D rb2D;
    private BoxCollider2D bc2D;

    //MOVEMENT BOOLEANS
    public bool grounded;
    public bool isMoving;

    //MOVEMENT VARIABLES
    private float horizontal;
    private float moveSpeed = 10f;
    private float jumpVelocity = 10f;
    private float verticalDeadzone = 0.6f;

    //HEALTH BAR
    public HealthBar healthBar;
    public int maxHealth = 100;
    public int currentHealth;

    //COMBAT VARIABLES
    public Vector2 bulletVelocity = new Vector2(15f, 0f);
    public int bulletsRemaining;
    public int magSize = 9;
    private float FireTime;
    private bool isReloaded = true;


    // will use later to flip player sprite depending on direction
    public enum Direction { right, left };
    public Direction playerDirection = Direction.right;

    //For jumping
    [SerializeField] private LayerMask platformLayerMask;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform fireTransR;
    [SerializeField] private Transform fireTransL;


    private void Awake()
    {

        playerSprite = this.GetComponent<SpriteRenderer>();

        rb2D = transform.GetComponent<Rigidbody2D>();
        bc2D = transform.GetComponent<BoxCollider2D>();

        bulletsRemaining = magSize;

        currentHealth = maxHealth;
        healthBar.setMaxHealth(maxHealth);
    }

    // Update is called once per frame
    private void Update()
    {

        playerSprite.flipX = playerDirection == Direction.right ? true : false;

        if ((Time.time - FireTime) > 1f && isReloaded == false)
        {
            isReloaded = true;
            bulletsRemaining = 9;
        }

        healthBar.setHealth(currentHealth);
        // update velocity based on horizontal component
        rb2D.velocity = new Vector2(horizontal * moveSpeed, rb2D.velocity.y);
        // Smooth look up and down camera movements
        isMoving = rb2D.velocity != Vector2.zero;

            if(checkDeath()){
                Destroy(gameObject);
            }
    }

    private bool checkDeath(){
        if(currentHealth <= 0)
            return true;
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6 && !grounded)
        {
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6 && grounded)
        {
            grounded = false;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (Mathf.Abs(context.ReadValue<Vector2>().y) > verticalDeadzone)
        {
            horizontal = 0f;
        }
        else
        {
            horizontal = context.ReadValue<Vector2>().x;
            if (context.ReadValue<Vector2>().x > 0)
            {
                playerDirection = Direction.right;
            }
            else if (context.ReadValue<Vector2>().x < 0)
            {
                playerDirection = Direction.left;
            }
            else if (context.ReadValue<Vector2>().x == 0 && playerDirection == Direction.right)
            {
                playerDirection = Direction.right;
            }
            else if (context.ReadValue<Vector2>().x == 0 && playerDirection == Direction.left)
            {
                playerDirection = Direction.left;
            }
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpVelocity);
        }


        // Adds functionality for variable jump (if you hold space you jump higher)
        if (context.canceled && rb2D.velocity.y > 0f)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, rb2D.velocity.y * 0.5f);
        }
    }

    public void SecondaryFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            if (bulletsRemaining == 0)
            {
                FireTime = Time.time;
                isReloaded = false;
            }

            bulletsRemaining--;

            if (isReloaded)
            {
                if (playerDirection == Direction.right)
                {
                    instantiateBullet(bullet, fireTransR.position, bulletVelocity);
                }
                else if (playerDirection == Direction.left)
                {
                    instantiateBullet(bullet, fireTransL.position, -bulletVelocity);
                }
            }
        }
    }

    private void instantiateBullet(GameObject bullet, Vector3 position, Vector2 velocity)
    {
        GameObject bulletInstance = Instantiate(bullet, position, Quaternion.Euler(0, 0, 90));
        bulletInstance.GetComponent<BulletShellController>().setOrigin(1);
        Rigidbody2D rbBulletInstance = bulletInstance.GetComponent<Rigidbody2D>();
        rbBulletInstance.velocity = velocity;
    }
}
