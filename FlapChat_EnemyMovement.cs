using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyBehaviorType1 : MonoBehaviour
{
    public float speed = 6f;
    public float maxHealth = 50.0f;
    public float currentHealth; 
    public float slidingRange = 5f; // range for enemies to detect player sliding
    public float attackRange = 2.0f; // range for attacking the player
    public float swordDamage = 60.0f;
    public float attackDelay = 0.6f;
    public float attackDuration = 1.0f;
    public GameObject healthBar;
    public GameObject playerPG;
    public PlayerController playerController;
    public DamageToPlayer dmg;
    public GameObject hitbox; // hitbox object
    //public ParticleSystem BloodShotParticles;

    public float LeftLimit = -124.7f;
    public float RightLimit = 124f;
    public float TopLimit = 9.09f;
    public float BottomLimit = -3.49f;

    /*
    public Animation WalkingAnimation; // animation for walking
    public Animation FightingAnimation; // animation for fighting
    */

    private float losTiempo = 0.0f;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public AudioSource swordAttack;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); 

        playerController = player.GetComponent<PlayerController>(); 
        currentHealth = maxHealth;
    }

    void Update()
    {
        IgnoreEnemyCollisions();
        if (player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            losTiempo += Time.deltaTime;
            animator.SetBool("Attack1", true);
            if (losTiempo >= 0.2f)
            {
                swordAttack.Play();
                if (losTiempo >= 0.6f)
                {
                    hitbox.SetActive(true);
                    dmg.TakeDamage(swordDamage);
                    hitbox.SetActive(false);
                    callCoroutine();
                    losTiempo = 0.0f;
                }
            }
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime);
            losTiempo = 0.0f;

            if (direction.x > 0)
                spriteRenderer.flipX = false;
            else if (direction.x < 0)
                spriteRenderer.flipX = true;

            animator.SetBool("Attack1", false);
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            //   BloodShotParticles.Play();
        }
    }

    void StopAnimation()    
    {
       animator.SetBool("Attack1", false); 
    }

    void IgnoreEnemyCollisions()
    {
        EnemyBehaviorType1[] enemies = FindObjectsOfType<EnemyBehaviorType1>();

        // ignore collisions between enemies
        foreach (EnemyBehaviorType1 enemy in enemies)
        {
            if (enemy != this) // avoid ignoring collision with itself
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>());
            }
        }
        if (playerPG != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerPG.GetComponent<Collider2D>());
        }
    }

    void callCoroutine()
    {
        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {
        animator.SetBool("Attack1", false);
        yield return new WaitForSeconds(attackDelay);
    }
}

