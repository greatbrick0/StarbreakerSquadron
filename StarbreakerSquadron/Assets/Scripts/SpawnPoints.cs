using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    [field: SerializeField]
    public Teams team { get; protected set; }
    [SerializeField]
    private Transform pointHolder;

    void Start()
    {
        switch (team)
        {
            case Teams.Environment:
                break;
            case Teams.Green:
                if (pointHolder.childCount == 0) break;
                if (ClientManager.instance == null) break;
                for (int ii = 0; ii < pointHolder.childCount; ii++)
                    ClientManager.instance.AddSpawnSpot(pointHolder.GetChild(ii));
                break;
            case Teams.Yellow:
                break;
        }
    }
}
