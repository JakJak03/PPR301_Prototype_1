using UnityEngine;

public class KnockBack 
{
    public static void Begin(Rigidbody2D rb, Vector2 direction, float force)
    {
            rb.AddForce(force * direction, ForceMode2D.Impulse);
    }
}
