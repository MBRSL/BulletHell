using UnityEngine;

public class ItemView : MonoBehaviour
{
    #region Public properties
    public delegate void HitEvent(ItemView self, Collider other);
    public event HitEvent OnHit;
    public event HitEvent OnLeave;
    #endregion
    
    #region Public methods
    public Bounds Bounds { get; private set;}
    public Rigidbody Rigidbody {get; private set; }
    public void Initialize()
    {
        Bounds = GetComponent<Collider>().bounds;
        Rigidbody = GetComponent<Rigidbody>();
    }
    #endregion
    
    #region Unity functions
    private void Start()
    {
        Bounds = GetComponent<Collider>().bounds;
        Rigidbody = GetComponent<Rigidbody>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (OnHit != null)
        {
            OnHit(this, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnLeave != null)
        {
            OnLeave(this, other);
        }
    }
    #endregion
}