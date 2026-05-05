using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager instance;

    [SerializeField]
    private List<GameObject> pooledBullets = new List<GameObject>();
    
    private Dictionary<GameObject, Stack<Attack>> pools = new Dictionary<GameObject, Stack<Attack>>();

    private void Awake()
    {
        foreach(GameObject prefab in pooledBullets)
        {
            if (prefab == null) continue;
            pools.Add(prefab, new Stack<Attack>());
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
        if (pools.TryGetValue(bulletObj, out Stack<Attack> pool))
        {
            Attack bulletRef = null;
            
            while (pool.Count > 0)
            {
                bulletRef = pool.Pop();
                if (bulletRef != null && !bulletRef.GetUsed()) break;
                bulletRef = null;
            }

            if (bulletRef == null)
            {
                bulletRef = AddNewInstance(bulletObj);
            }

            bulletRef.transform.position = newPos;
            if (!bulletRef.IsSpawned)
            {
                bulletRef.GetComponent<NetworkObject>().Spawn(true);
            }
            return bulletRef.gameObject;
        }
        else
        {
            GameObject bulletRefGo = Instantiate(bulletObj);
            bulletRefGo.transform.position = newPos;
            bulletRefGo.GetComponent<NetworkObject>().Spawn(true);
            return bulletRefGo;
        }
    }

    public void ReturnToPool(GameObject prefab, Attack instance)
    {
        if (prefab != null && pools.ContainsKey(prefab))
        {
            pools[prefab].Push(instance);
        }
    }

    private Attack AddNewInstance(GameObject bulletObj)
    {
        GameObject bulletGo = Instantiate(bulletObj, transform);
        Attack attack = bulletGo.GetComponent<Attack>();
        attack.pooled = true;
        attack.originPrefab = bulletObj;
        return attack;
    }
}
