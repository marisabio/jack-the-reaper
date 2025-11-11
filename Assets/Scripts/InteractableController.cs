using UnityEngine;
using UnityEngine.Events;

public class InteractableController : MonoBehaviour
{
    public UnityEvent onInteract;

    public void Interact()
    {
        onInteract.Invoke();
    }
    
    
}
