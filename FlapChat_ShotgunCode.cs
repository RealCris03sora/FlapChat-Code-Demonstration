using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static PlayerController;

public class ShotgunScript : MonoBehaviour
{
    public GameObject Player;
    public GameObject Shotgun;
    public SpriteRenderer sr;
    public Rigidbody2D rb;
    public GameObject Projectile;
    public ProjectilesBehavior scriptA;
    public PlayerController playerController;
    public Transform rotationPoint;
    private Transform playerTransform;
    public float projectileOffset = 1.0f;

    private float recoilForce = 8.0f;
    private float chargedRecoilForce = 16.0f;
    private Vector2 direction;

    void Start()
    {
        scriptA = Projectile != null
             ? Projectile.GetComponent<ProjectilesBehavior>()
             : null;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (rotationPoint == null)
        {
            rotationPoint = transform;
        }

        transform.localPosition = new Vector3(-0.5f, 0f, 0f);
    }

    void Update()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }

        MoveShotgunWithPlayer();
        RotateShotgunTowardsMouse();
    }

    void RotateShotgunTowardsMouse()
    {
        if (playerController.currentState == playerState.Moving && Shotgun != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)rotationPoint.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotationPoint.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));

            bool shouldFlip = (direction.x < 0);
            sr.flipX = shouldFlip;
            Shotgun.GetComponent<SpriteRenderer>().flipY = shouldFlip;

            float offset = (sr.flipX) ? -0.0001f : 0.0001f;
            Shotgun.transform.position = new Vector2(transform.position.x, Shotgun.transform.position.y - 0.3f);
        }
    }

    void MoveShotgunWithPlayer()
    {
        if (Shotgun != null)
        {
            // match the position of the shotgun to the player's position
            if (direction.x > 0.0f)
            {
                Shotgun.transform.position = new Vector2(transform.position.x - 0.1f, transform.position.y - 0.5f);

            }
            else if (direction.x < 0.0f)
            {
                Shotgun.transform.position = new Vector2(transform.position.x - 0.1f, transform.position.y + 0.3f);
            }
        }
    }


    public void RotateShotgunDown()
    {
        Shotgun.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    public void ShootWithRecoil()
    {
        FireAimedShot(6, 30f, 5, 10, recoilForce * 2f);
    }

    public void ChargedShot()
    {
        FireAimedShot(10, 10f, 5, 15, chargedRecoilForce);
    }

    public void ShootDown()
    {
        if (Shotgun == null || Projectile == null || scriptA == null)
        {
            return;
        }

        rb.AddForce(Vector2.up * (recoilForce * 1.1f), ForceMode2D.Impulse);

        float projectileSpeed = Random.Range(10, 20);
        SpawnPellets(Vector2.down, 6, 30f, projectileSpeed);
    }

    private void FireAimedShot(
        int pelletCount,
        float spreadDegrees,
        int minSpeed,
        int maxSpeedExclusive,
        float recoilImpulse)
    {
        Camera aimCamera = Camera.main;

        if (Shotgun == null || Projectile == null || scriptA == null || aimCamera == null)
        {
            return;
        }

        Vector2 mousePosition = aimCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mousePosition - (Vector2)rotationPoint.position;

        if (aimDirection.sqrMagnitude < 0.0001f)
        {
            aimDirection = rotationPoint.right;
        }

        aimDirection.Normalize();

        rb.AddForce(-aimDirection * recoilImpulse, ForceMode2D.Impulse);

        float speedBonus = scriptA.speedBonus * playerController.currentPlayerSpeed;

        SpawnPellets(
            aimDirection,
            pelletCount,
            spreadDegrees,
            speedBonus,
            minSpeed,
            maxSpeedExclusive);
    }

    private void SpawnPellets(
        Vector2 shotDirection,
        int pelletCount,
        float spreadDegrees,
        float baseSpeed,
        int minExtraSpeed = 0,
        int maxExtraSpeedExclusive = 0)
    {
        float shotAngle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;
        float halfSpread = spreadDegrees * 0.5f;
        Vector3 shotOrigin = Shotgun.transform.position;

        for (int i = 0; i < pelletCount; i++)
        {
            float pelletAngle = shotAngle + Random.Range(-halfSpread, halfSpread);
            float radians = pelletAngle * Mathf.Deg2Rad;
            Vector2 pelletDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            float pelletSpeed = baseSpeed + Random.Range(minExtraSpeed, maxExtraSpeedExclusive);
            Vector3 spawnPosition = shotOrigin + (Vector3)(pelletDirection * projectileOffset);

            GameObject projectile = Instantiate(Projectile, spawnPosition, Quaternion.identity);
            scriptA.MoveProjectile(projectile, pelletDirection, pelletSpeed);
        }
    }
}