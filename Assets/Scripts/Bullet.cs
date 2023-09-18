using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public event Action<Bullet> OnExitingSpace;

    private Collider _activeSpace;

    public void Init(Collider activeSpace)
    {
        _activeSpace = activeSpace;
    } 

    private void OnTriggerExit(Collider other)
    {
        if (other == _activeSpace)
        {
            OnExitingSpace(this);
        }
    }
}
