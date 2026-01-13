using UnityEngine;

/// <summary>
/// Checkpoint that saves the player's respawn position when touched.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    private Renderer rend;
    private bool isActivated = false;
    
    private static Checkpoint lastActiveCheckpoint;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateVisual();
    }
    
    /// <summary>
    /// Activate this checkpoint and deactivate the previous one.
    /// </summary>
    public void Activate()
    {
        if (isActivated) return;
        
        /* Deactivate previous checkpoint */
        if (lastActiveCheckpoint != null && lastActiveCheckpoint != this)
        {
            lastActiveCheckpoint.Deactivate();
        }
        
        isActivated = true;
        lastActiveCheckpoint = this;
        UpdateVisual();
        
        /* Save this position to GameManager */
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(transform.position + Vector3.up, transform.rotation);
        }
    }
    
    private void Deactivate()
    {
        isActivated = false;
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        if (rend != null)
        {
            rend.material.color = isActivated ? activeColor : inactiveColor;
        }
    }
}
