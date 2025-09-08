using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager instance;

    [SerializeField]
    private List<GameObject> pooledBullets = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

    private void Awake()
    {
        foreach(GameObject ii in pooledBullets)
        {
            pools.Add(ii, new List<GameObject>());
        }
    }

    private void OnEnable()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDisable()
    {
        if (instance == this) instance = null;
    }

    public GameObject GetBullet(GameObject bulletObj, Vector3 newPos = default)
    {
        GameObject bulletRef;
        if (pools.ContainsKey(bulletObj))
        {
            bulletRef = GetAvailableInstance(bulletObj);
            if (bulletRef == null)
            {
                bulletRef = AddNewInstance(bulletObj);
                bulletRef.transform.position = newPos;
                bulletRef.GetComponent<NetworkObject>().Spawn(true);
            }
            else bulletRef.transform.position = newPos;
        }
        else
        {
            bulletRef = Instantiate(bulletObj);
            bulletRef.transform.position = newPos;
            bulletRef.GetComponent<NetworkObject>().Spawn(true);
        }
        
        return bulletRef;
    }

    private GameObject GetAvailableInstance(GameObject bulletObj)
    {
        foreach(GameObject ii in pools[bulletObj])
        {
            if (ii == null) continue;
            if (ii.GetComponent<Attack>().GetUsed() == false && ii.GetComponent<Attack>().timeUnused > 1.0f) return ii;
        }
        return null;
    }

    private GameObject AddNewInstance(GameObject bulletObj)
    {
        GameObject bulletRef = Instantiate(bulletObj, transform);
        Debug.Log(bulletRef.transform.parent.name);
        bulletRef.GetComponent<Attack>().pooled = true;
        pools[bulletObj].Add(bulletRef);
        return bulletRef;
    }
}
