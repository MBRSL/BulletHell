using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool : MonoBehaviour
{
    #region Private properties
    private GameObject _templatePrefab;
    private List<GameObject> _objectPool;
    #endregion

    #region Public functions
    public void Initialize(GameObject templatePrefab, int initSize)
    {
        _templatePrefab = templatePrefab;
        if (_objectPool == null)
        {
            _objectPool = new List<GameObject>();
        }
        for(int i = _objectPool.Count-1; i >= 0; i--)
        {
            Destroy(_objectPool[i]);
            _objectPool.RemoveAt(i);
        }
        for (int i = 0; i < initSize; i++)
        {
            _Initiate();
        }
    }

    public GameObject GetObjectFromPool()
    {
        for (int i = 0; i < _objectPool.Count; i++)
        {
            if (!_objectPool[i].activeInHierarchy)
            {
                return _objectPool[i];
            }
        }

        return _Initiate();
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        if (_objectPool.Contains(obj))
        {
            obj.SetActive(false);
        }
    }
    #endregion

    #region Private functions
    private GameObject _Initiate()
    {
        GameObject newObj = Instantiate(_templatePrefab, transform);
        newObj.SetActive(false);
        _objectPool.Add(newObj);
        return newObj;
    }
    #endregion
}