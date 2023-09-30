using UnityEngine;

public class ItemView : MonoBehaviour
{
    #region Public properties
    public delegate void HitEvent(ItemView self, Collider other);
    public event HitEvent OnHit;
    public event HitEvent OnLeave;
    #endregion
    
    #region Public methods
    public Bounds Bounds {
        get
        {
            if (_bounds == null)
            {
                _bounds = GetComponent<Collider>().bounds;
            }
            return _bounds.Value;
        }
    }
    public Rigidbody Rigidbody {
        get
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            return _rigidbody;
        }
    }
    #endregion
    
    #region Private properties
    private Bounds? _bounds;
    private Rigidbody _rigidbody;
    #endregion

    #region Unity functions
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