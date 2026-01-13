using UnityEngine;

/// <summary>
/// Death zone that kills player on contact.
/// Attach this to planes or colliders that should kill the player.
/// </summary>
public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("DeathZone: Oyuncu oldu!");
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
            else
            {
                /* Try parent */
                player = other.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    player.Die();
                }
            }
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        /* Fallback for CharacterController */
        if (hit.gameObject.CompareTag("Player"))
        {
            PlayerController player = hit.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}
