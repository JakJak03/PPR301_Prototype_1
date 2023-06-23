using UnityEngine;

public class KnockBack 
{
    public static void Begin(Rigidbody2D rb, int direction, float force)
    {
        if (direction == -1) // Knock Left
        {
            rb.AddForce(force * -Vector2.right, ForceMode2D.Impulse);
        }
        else if (direction == 1) // Knock Right
        {
            rb.AddForce(force * Vector2.right, ForceMode2D.Impulse);
        }
        else
            return;
    }
}
