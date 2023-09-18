using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool : MonoBehaviour
{
    private GameObject _templatePrefab;
    private List<GameObject> _objectPool;

    public void Initiate(GameObject templatePrefab, int initSize)
    {
        _templatePrefab = templatePrefab;
        _objectPool = new List<GameObject>();

        for (int i = 0; i < initSize; i++)
        {
            GetObjectFromPool();
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

        GameObject newObj = Instantiate(_templatePrefab, transform);
        newObj.SetActive(false);
        _objectPool.Add(newObj);
        return newObj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        if (_objectPool.Contains(obj))
        {
            obj.SetActive(false);
        }
    }
}
