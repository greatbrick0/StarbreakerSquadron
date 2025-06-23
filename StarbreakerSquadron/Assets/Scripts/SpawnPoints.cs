using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    [field: SerializeField]
    public Teams team { get; protected set; }
    [SerializeField]
    private Transform pointHolder;
    [SerializeField]
    private Transform visuals;
    [SerializeField]
    private Sprite platformSprite;

    private void Start()
    {
        switch (team)
        {
            case Teams.Environment:
                break;
            case Teams.Green:
                if (pointHolder.childCount == 0) break;
                if (ClientManager.instance == null)
                {
                    AddPlatforms();
                }
                else
                {
                    for (int ii = 0; ii < pointHolder.childCount; ii++)
                        ClientManager.instance.AddSpawnSpot(pointHolder.GetChild(ii));
                }
                break;
            case Teams.Yellow:
                break;
        }
    }

    private void AddPlatforms()
    {
        for (int ii = 0; ii < pointHolder.childCount; ii++)
        {
            GameObject platform = new GameObject("platform" + ii.ToString());
            platform.transform.parent = visuals;
            platform.transform.localPosition = pointHolder.GetChild(ii).transform.localPosition;
            platform.transform.localScale = Vector3.one * 0.2f;
            platform.AddComponent<SpriteRenderer>().sprite = platformSprite;
            platform.GetComponent<SpriteRenderer>().sortingLayerName = "Spawner";
        }
    }
}
