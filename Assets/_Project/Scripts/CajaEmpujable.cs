using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class CajaEmpujable : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 3.5f;
        rb.linearDamping = 2.0f;
        rb.angularDamping = 3.0f;
        rb.freezeRotation = true;
    }
}
