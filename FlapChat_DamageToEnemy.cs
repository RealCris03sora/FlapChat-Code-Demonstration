using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    // using an enum is the easiest way to handle different types of enemies
    // as we plan to introduce at least 3 different types that are currently
    // under planning and development
    public enum EnemyType
    {
        Type1,
        Type2,
        Type3,
        Mannequin,
    }

    public float maxHealthType1 = 50.0f;
    public float maxHealthType2 = 150.0f;
    public float maxHealthType3 = 10.0f;
    public float mannequinHealth = 10000.0f;

    public float currentHealthT;

    public EnemyType enemyType; 
    public PlayerController playerController;

    public GameObject player;

    // Needed to build Rizz Armor on hit:
    public DamageToPlayer currentArmor;

    public AudioSource damage1;
    public AudioSource damage2;
    public AudioSource damage3;

    public SimpleFlash flash;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        switch (enemyType)
        {
            case EnemyType.Type1:
                currentHealthT = maxHealthType1;
                break;

            case EnemyType.Type2:
                currentHealthT = maxHealthType2;
                break;

            case EnemyType.Type3:
                currentHealthT = maxHealthType3;
                break;

            case EnemyType.Mannequin:
                currentHealthT = mannequinHealth;
                break;

            default:
                currentHealthT = maxHealthType1; // in case i forget the select type
                break;
        }
    }

    void Update()
    {
        enemyDeath();
    }

    public void TakeDamage(float damage)
    {
        damage = Mathf.Max(0f, damage); // damage can't be negative
        switch (enemyType)
        {
            case EnemyType.Type1:
                damage1.pitch = Random.Range(0.7f, 1.3f);
                flash.Flash();  
                damage1.Play();
                break;

            case EnemyType.Type2:
                damage2.pitch = Random.Range(1f, 1.5f);
                flash.Flash();
                damage2.Play();
                break;

            case EnemyType.Type3:
                damage3.pitch = Random.Range(0.6f, 1f);
                flash.Flash();
                damage3.Play();
                break;

            case EnemyType.Mannequin:
                flash.Flash();
                break;

            default:
                damage1.pitch = Random.Range(0.7f, 1.3f);
                damage1.Play();
                flash.Flash();
                break;
        }
        currentHealthT -= damage; 
        FlapChat_Rizz.rizzScore += 2;
        if (player != null && DamageToPlayer.currentArmorPlayer <= 100.0f)
        {
            DamageToPlayer.currentArmorPlayer += 2;
        }
    }

    public void enemyDeath()
    {
        if (currentHealthT <= 0.0f)
        {
            HandleDestruction();

            KillCounter.killCounter += 1;
            FlapChat_Rizz.rizzScore += 5;
            if (player != null && DamageToPlayer.currentArmorPlayer <= 100.0f)
            {
                DamageToPlayer.currentArmorPlayer += 5;
            }
        }
    }

    private void HandleDestruction()
    {
        if (enemyType == EnemyType.Mannequin)
            return; 

        Destroy(gameObject);      
    }
}