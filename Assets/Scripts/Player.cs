using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    CircleCollider2D playerCollider;
    public LayerMask collisionMask;
    public LayerMask collisionMaskStatic;

    private Controller gameController;

    public float walkSpeed = 5f;

    private Vector2 moveInput;
    private bool isAtExit = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
        gameController = FindFirstObjectByType<Controller>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += (Vector3)moveInput * walkSpeed * Time.fixedDeltaTime;
        Physics2D.SyncTransforms();
        SolveOverlaps();
    }

    void SolveOverlaps()
    {
        if (playerCollider == null)
            return;

        SolveOverlapsForHits(GetOverlapsLayer(collisionMask));
        SolveOverlapsForHits(GetOverlapsLayer(collisionMaskStatic));
    }

    Collider2D[] GetOverlapsLayer(LayerMask layerMask)
    {
        return Physics2D.OverlapBoxAll(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f,
            layerMask
        );
    }

    void SolveOverlapsForHits(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit == playerCollider || hit.isTrigger)
                continue;

            // Get penetration info
            ColliderDistance2D dist = playerCollider.Distance(hit);

            if (dist.isOverlapped)
            {
                // Move this object by the penetration vector to separate colliders
                Vector3 correction = (Vector3)(dist.normal * dist.distance);
                transform.position += correction;
            }
            Physics2D.SyncTransforms();
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnInteract()
    {
        Debug.Log("Interact pressed");
        if (isAtExit)
            gameController.NextLevel();
    }

    void Update()
    {
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
                Debug.Log($"Joystick button {i} pressed");
        }
    }

    public void OnFire()
    {
        Debug.Log("Fire pressed");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Exit"))
        {
            isAtExit = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Exit"))
        {
            isAtExit = false;
        }
    }
}
