using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public delegate void HitEvent(Bullet self, Collider other);
    public event HitEvent OnHit;
    public event HitEvent OnLeave;

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