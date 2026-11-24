using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerController;

public class PlayerController : MonoBehaviour
{
    public enum playerState
    {
        Moving,
        Sliding,
    }

    public playerState currentState;

    public float playerMoveSpeed = 5f;
    public float slideFactor = 0.5f; // Adjust this value to control the slide effect
    public GameObject Shotgun;
    public GameObject Beek;
    public SpriteRenderer shotgunRender;
    public SpriteRenderer playerSpriteRenderer;
    public DamageToPlayer damageToPlayer;
    public float currentPlayerSpeed;
    public Sprite sp1, sp2;
    public float maxHealth = 100.0f;  // Maximum health points
    public float currentHealth;
    public float slideTimer = 0;
    public bool canSlide = true;
    public bool isSliding = false;
    public float maxMovingSpeed = 22.0f;
    public float maxSlidingSpeed = 40.0f;

    public int shotsFired = 0;
    public float timeSinceLastShot = 2.0f;
    public float shootingCooldown = 1.2f; // Adjust the cooldown duration as needed

    private float tempSpeed;
    private float tempVelocity;

    private float chargeTime = 0.0f;
    private float fullCharge = 1.6f;

    public float speedPush = 8f;
    private int speedDirection;

    public GameObject crosshair; // Reference to the crosshair object
    public float crosshairDistance = 3f; // Adjust the distance from the shotgun to the crosshair

    public float jumpVelocity;
    private float fallMultiplier = 4f;
    private float lowJumpMultiplier = 3f;

    private Quaternion initialRotation;
    private float initialGravityScale;

    public float groundRaycastDistance = 0.5f;
    public LayerMask groundLayer; // Set the ground layer in the Unity editor
    public LayerMask speederLayer;
    public LayerMask platformLayer;
    public LayerMask tutorialGround;
    public bool isJumping;

    private BoxCollider2D playerCollider;

    public float beekPos;

    public SpriteRenderer sr;
    public Rigidbody2D rb;

    public ParticleSystem RunParticle;

    public ParticleSystem SlideParticle;

    public ParticleSystem FallParticle;

    private float tempYSpeed;

    public int RunParticleOccurAfterVelocity;

    public int SlideParticleOccurAfterVelocity;

    public float dustFormationPeriod;

    float Particlecounter;

    public bool running;
    private Animator animator;

    public ParticleSystem ChargedShotParticle;

    public float EmissionRateFactor;

    public AudioSource jumpSound;
    public AudioSource shotgunShot;
    public AudioSource reloadSound;

    public SimpleFlash flash;

    void Start()
    {
        currentState = playerState.Moving;
        shotgunRender.GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
        timeSinceLastShot = 2.0f;

        // crosshair initial position
        if (crosshair != null)
        {
            crosshair = Instantiate(crosshair, transform.position + new Vector3(0.5f, 0f, 0f), Quaternion.identity);
        }

        UnityEngine.Cursor.visible = false;
        Beek.SetActive(false);
        playerCollider = GetComponent<BoxCollider2D>();

        initialRotation = transform.rotation;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Particlecounter += Time.deltaTime;

        tempYSpeed = rb.velocity.y;

        if (damageToPlayer.isInvulnerable == true)
        {
            damageToPlayer.timeSinceLastHit += Time.deltaTime;
            if (damageToPlayer.timeSinceLastHit >= damageToPlayer.invulnerableTime)
            {
                damageToPlayer.isInvulnerable = false;
                damageToPlayer.timeSinceLastHit = 0.0f;
            }
        }

        if (IsGrounded() && MathF.Abs(rb.velocity.x) > RunParticleOccurAfterVelocity && MathF.Abs(rb.velocity.x) < SlideParticleOccurAfterVelocity)
        {
            if (Particlecounter > dustFormationPeriod)
            {
                RunParticle.Play();
                Particlecounter = 0;
            }

        }

        else if (IsGrounded() && MathF.Abs(rb.velocity.x) > SlideParticleOccurAfterVelocity)
        {
            if (Particlecounter > dustFormationPeriod)
            {
                SlideParticle.Play();
                Particlecounter = 0;
            }
        }

        if (tempYSpeed < 0 && CanJump())
        {
            FallParticle.Play();

        }

        speedCounter();
        slideTimer += Time.deltaTime;

        if (rb.velocity.y < 0f)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }else if (rb.velocity.y > 0f && Input.GetButton("Jump") && CanJump())
        {
            doMove();
            StartCoroutine(SlideReset());
        }

        if (Input.GetButtonDown("Jump") && currentState == playerState.Moving && CanJump())
        {
            animator.SetBool("Jump", true);
            Jump();
        }

        else if (CanJump())
        {
            animator.SetBool("Jump", false);

            if (running)
            {
                animator.SetBool("Running", true);
            }
            else
            {
                animator.SetBool("Running", false);
            }

        }

        if (isJumping && rb.velocity.y <= 0f && CanJump())
        {
            isJumping = false;
        }

        if (currentState == playerState.Moving)
        {
            animator.SetBool("Running", running);
            CharacterMovement();
            MoveCrosshairWithMouse();

            if (canSlide && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.S)))
            {
                doSlide(Input.GetAxisRaw("Horizontal"));
                canSlide = false;
            }

            if (Input.GetMouseButton(1))
            {
                Shotgun.GetComponent<ShotgunScript>().RotateShotgunDown();

                if (Input.GetMouseButtonDown(0) && CanShoot())
                {
                    DownShot();
                    shotsFired = 2;

                    if (shotsFired >= 2)
                    {
                        reloadSound.Play();
                        timeSinceLastShot = 0.0f; // reset the cooldown timer
                        shotsFired = 0; // reset the shots counter
                    }
                }
            }
            else if (Input.GetMouseButton(0) && CanShoot())
            {
                ChargedShotParticle.Play();
                var emissionP = ChargedShotParticle.emission;
                var mainF = ChargedShotParticle.main;
                var speedS = ChargedShotParticle.shape;
                if(chargeTime < 2.0f)
                {
                   emissionP.rateOverTime = EmissionRateFactor * chargeTime * 1.3f;
                }else
                {
                    mainF.startSpeed = 2.0f;
                    speedS.radius = 0.0f;
                }
                chargeTime += Time.deltaTime;
            }
            else if (Input.GetMouseButtonUp(0) && CanShoot())
            {
                shotgunShot.Play();
                var emissionP = ChargedShotParticle.emission;
                var mainF = ChargedShotParticle.main;
                var speedS = ChargedShotParticle.shape;
                speedS.radius = 1.41f;
                mainF.startSpeed = UnityEngine.Random.Range(-0.33f, -1.17f);
                if (chargeTime >= fullCharge)
                {
                    Shotgun.GetComponent<ShotgunScript>().ChargedShot();
                    shotsFired = 2;
                }
                else
                {
                    Shoot();
                    shotsFired++;
                }

                if (shotsFired >= 2)
                {
                    reloadSound.Play();
                    timeSinceLastShot = 0.0f;
                    shotsFired = 0;
                }
                chargeTime = 0.0f;
            }
            timeSinceLastShot += Time.deltaTime;
        }
        else if (currentState == playerState.Sliding)
        {
            animator.enabled = false;

            bool slideHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S);
            bool isOnSlideSurface = IsGrounded() || IsSpeeder();

            rb.gravityScale = isOnSlideSurface && slideHeld ? 0f : initialGravityScale;

            if (isOnSlideSurface && slideHeld)
            {
                int surfaceMask = groundLayer | speederLayer;

                // Check for rotation
                RaycastHit2D hit = Physics2D.Raycast(
                    rb.position,
                    -transform.up,
                    groundRaycastDistance,
                    surfaceMask);

                // Lower Check
                if (hit.collider == null || hit.fraction == 0f)
                {
                    hit = Physics2D.Raycast(
                        rb.position,
                        Vector2.down,
                        groundRaycastDistance,
                        surfaceMask);
                }

                if (hit.collider != null && hit.fraction > 0f)
                {
                    Vector2 normal = hit.normal;
                    Vector2 tangent = new Vector2(normal.y, -normal.x);

                    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    float speedAlongSurface = Vector2.Dot(rb.velocity, tangent);
                    const float facingSpeedThreshold = 0.05f;

                    if (Mathf.Abs(speedAlongSurface) > facingSpeedThreshold)
                    {
                        sr.flipX = speedAlongSurface < 0f;
                        beekPos = sr.flipX ? -0.8f : 0.6f;
                    }
                }
            }

            if (!slideHeld)
            {
                doMove();
                StartCoroutine(SlideReset());
            }
        }
        Beek.transform.position = new Vector2(transform.position.x + beekPos, transform.position.y - 0.1f);

        tempSpeed = rb.velocity.x;
        tempVelocity = rb.velocity.x / 3.5f;
    }

    void doSlide(float horizontalInput)
    {
        animator.enabled = false;
        currentState = playerState.Sliding;

        if (horizontalInput != 0f && tempVelocity != 0f)
        {
            float direction = Mathf.Sign(horizontalInput);
            float impulse = Mathf.Abs(rb.velocity.x / tempVelocity) * speedPush * 1.6f;

            rb.AddForce(Vector2.right * (direction * impulse), ForceMode2D.Impulse);
        }

        playerCollider.size = new Vector2(0.7862152f, 0.4594564f);
        playerCollider.offset = new Vector2(-0.02990451f, 0.03766604f);

        // hide the shotgun (visually)
        if (Shotgun != null)
        {
            shotgunRender.enabled = false;
        }

        GetComponent<SpriteRenderer>().sprite = sp2;

        // show the beak object
        if (Beek != null)
        {
            Beek.SetActive(true);
        }

        // move the crosshair to the mouse position
        if (crosshair != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            crosshair.transform.position = mousePos;
        }

        FlipSpriteBasedOnMovement();
        isSliding = true;
    }

    void doMove()
    {
        transform.rotation = initialRotation;

        animator.enabled = true;
        damageToPlayer.isInvulnerable = true;

        currentState = playerState.Moving;

        playerCollider.size = new Vector2(0.8172315f, 1.0f); // restore the original size
        playerCollider.offset = new Vector2(-0.09138423f, 0.0f); // restore the original offset

        GetComponent<SpriteRenderer>().sprite = sp1;

        if (crosshair != null)
        {
            crosshair.SetActive(true);
        }

        // Show the shotgun
        if (Shotgun != null)
        {
            shotgunRender.enabled = true;
        }

        // Hide the "beek" object
        if (Beek != null)
        {
            Beek.SetActive(false);
        }

        isSliding = false;
    }

    public float GetCurrentPlayerSpeed()
    {
        return currentPlayerSpeed;
    }

    bool CanShoot()
    {
        return timeSinceLastShot >= shootingCooldown;
    }

    void Shoot()
    {
        Shotgun.GetComponent<ShotgunScript>().ShootWithRecoil();
    }

    void speedCounter()
    {
        currentPlayerSpeed = Mathf.Abs(rb.velocity.x); // using Mathf.Abs to get the absolute value
    }

    void CharacterMovement()
    {
        float horizontalInput = 0;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput != 0)
        {
            running = true;
        }
        else
        {
            running = false;
        }

        Vector2 movement = new Vector2(horizontalInput, 0f);

        Beek.transform.position = new Vector2(transform.position.x, transform.position.y);

        // sliding force (momentum base)
        Vector2 slideForce = movement * playerMoveSpeed * slideFactor;
        rb.AddForce(slideForce);

        if (currentState == playerState.Moving)
        {
            // using clamp to prevent the player from being too fast
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxMovingSpeed, maxMovingSpeed), rb.velocity.y);
        }
        else if (currentState == playerState.Sliding)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSlidingSpeed, maxSlidingSpeed), rb.velocity.y);
        }
    }

    void MoveCrosshairWithMouse()
    {
        if (crosshair != null && currentState == playerState.Moving)
        {
            // mouse position in world coordinates
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            crosshair.transform.position = mousePos;
        }
    }

    void FlipSpriteBasedOnMovement()
    {
        // flip the player sprite based on the movement direction
        bool shouldFlip = (rb.velocity.x < 0); // check if the player is moving to the left
        if (shouldFlip)
        {
            beekPos = -0.8f;
        }
        else
        {
            beekPos = +0.6f;
        }
        sr.flipX = shouldFlip;
    }

    public Vector3 GetPlayerMovementDirection()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector3 movementDirection = new Vector3(horizontalInput, 0f, 0f);
        return movementDirection.normalized;
    }

    void DownShot()
    {
        shotgunShot.Play();
        Shotgun.GetComponent<ShotgunScript>().ShootDown();
    }

    void Jump()
    {
        // check if the player is grounded before jumping
        if (CanJump())
        {
            FallParticle.Play();
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);

            jumpSound.Play();

            float horizontalInput = Input.GetAxis("Horizontal");
            rb.velocity += new Vector2(horizontalInput * playerMoveSpeed * 0.5f, 0f);

            isJumping = true;
        }
    }

    bool isTutorialGrounded()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, 1.0f, tutorialGround);
        return collider != null;
    }

    bool IsGrounded()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, 1.0f, groundLayer);
        return collider != null;
    }

    bool CanJump()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, 1.0f, groundLayer);
        Collider2D collider2 = Physics2D.OverlapCircle(transform.position, 1.0f, platformLayer);
        Collider2D collider3 = Physics2D.OverlapCircle(transform.position, 1.0f, tutorialGround);
        if (collider != null || collider2 != null || collider3 != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsSpeeder()
    {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, 1.0f, speederLayer);
        return collider != null;
    }

    IEnumerator SlideReset()
    {
        yield return new WaitForSeconds(0.8f); // timer to reset the sliding state
        canSlide = true;
    }
}