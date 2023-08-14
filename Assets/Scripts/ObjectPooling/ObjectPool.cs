using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject Prefab;
    private int Size;
    private List<PoolableObject> AvailableObjectsPool;
    private List<PoolableObject> UnavailableObjectsPool;
    private List<PoolableObject> DisableObjectsHolder;

    private ObjectPool(PoolableObject Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObject>(Size);
        UnavailableObjectsPool = new List<PoolableObject>(Size);
        DisableObjectsHolder = new List<PoolableObject>(Size);
    }

    public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
    {
        ObjectPool pool = new ObjectPool(Prefab, Size);

        GameObject poolGameObject = new GameObject(Prefab + " Pool");
        pool.CreateObjects(poolGameObject);

        return pool;
    }

    private void CreateObjects(GameObject parent)
    {
        for (int i = 0; i < Size; i++)
        {
            PoolableObject poolableObject = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.Parent = this;
            poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
        }
    }

    public PoolableObject GetObject()
    {
        PoolableObject instance;

        // No more available objects
        if (AvailableObjectsPool.Count == 0)
        {
            instance = UnavailableObjectsPool[0];
            UnavailableObjectsPool.RemoveAt(0);
            UnavailableObjectsPool.Add(instance);
            instance.gameObject.SetActive(true);

            return instance;
        }

        instance = AvailableObjectsPool[0];
        AvailableObjectsPool.RemoveAt(0);
        UnavailableObjectsPool.Add(instance);
        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        AvailableObjectsPool.Add(Object);
        UnavailableObjectsPool.Remove(Object);
    }

    public void DisableAll()
    {
        /*AvailableObjectsPool.AddRange(UnavailableObjectsPool);
        UnavailableObjectsPool.Clear();

        Debug.Log("AvailableObjectsPool.Count = " + AvailableObjectsPool.Count);
        Debug.Log("UnavailableObjectsPool.Count = " + UnavailableObjectsPool.Count);*/

        DisableObjectsHolder.Clear();
        DisableObjectsHolder.AddRange(UnavailableObjectsPool);

        Debug.Log(DisableObjectsHolder.Count);

        DisableObjectsHolder.ForEach(p => p.gameObject.SetActive(false));
    }
}