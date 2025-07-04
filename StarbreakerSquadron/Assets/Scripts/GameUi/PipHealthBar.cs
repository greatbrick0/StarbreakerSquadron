using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class PipHealthBar : MonoBehaviour
{
    [SerializeField]
    private GameObject pipObj;
    private List<GameObject> pipRefs = new List<GameObject>();

    private SmallHealth health;

    [SerializeField]
    private int amountPerPip = 20;
    [SerializeField]
    private int maxPipCount = 5;
    [SerializeField]
    private float pipSpacingFactor = 2.0f;
    [SerializeField]
    private float pipScaleFactor = 0.2f;

    public void Initialize(SmallHealth newHealth)
    {
        health = newHealth;
        CreateHealthPips(health.maxHealth);
        ChangePipColour(Color.cyan);
    }

    private void CreateHealthPips(int maxHealthAmount)
    {
        maxHealthAmount = Min(maxHealthAmount, amountPerPip * maxPipCount);

        float pipCount = maxHealthAmount / amountPerPip;

        for (int ii = 0; ii < CeilToInt(pipCount); ii++)
        {
            pipRefs.Add(Instantiate(pipObj));
            pipRefs[ii].transform.parent = transform;
            pipRefs[ii].transform.localScale = Vector3.one * pipScaleFactor;
            pipRefs[ii].transform.localPosition = new Vector3((ii/(Ceil(pipCount) - 1) - 0.5f) * pipSpacingFactor, 0, 0);
        }
    }

    private void ChangePipColour(Color newColour)
    {
        for(int ii = 0; ii < transform.childCount; ii++)
        {
            transform.GetChild(ii).GetChild(1).GetComponent<SpriteRenderer>().color = newColour;
        }
    }
}
