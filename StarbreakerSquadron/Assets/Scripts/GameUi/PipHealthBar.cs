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

    [Header("Display Colours")]
    [SerializeField]
    private Color ownedColour = Color.green;
    [SerializeField]
    private List<Color> teamColours = new List<Color>();
    [SerializeField]
    private List<string> teamToColourMapping = new List<string>();
    private Color colour;

    [Header("Display Values")]
    [SerializeField]
    private int amountPerPip = 200;
    [SerializeField]
    private int maxPipCount = 5;
    [SerializeField]
    private float pipSpacingFactor = 2.0f;
    [SerializeField]
    private float pipScaleFactor = 0.2f;
    private Vector2 pipFillPrevSize = Vector2.one;
    [SerializeField]
    private float borderWidth = 0.09f;

    public void Awake()
    {
        pipFillPrevSize = pipObj.transform.GetChild(1).GetComponent<SpriteRenderer>().size;
        transform.localScale = Vector3.one * pipScaleFactor;
    }

    public void SetColourData(string teamColour, bool owned)
    {
        if(owned)
        {
            colour = ownedColour;
            return;
        }

        colour = teamColours[teamToColourMapping.IndexOf(teamColour)];
    }

    public void Initialize(SmallHealth newHealth)
    {
        health = newHealth;
        UpdateHealthBarMax(health.maxHealth);
    }

    private void CreateHealthPips(int maxHealthAmount)
    {
        maxHealthAmount = Min(maxHealthAmount, amountPerPip * maxPipCount);

        float pipCount = (float)maxHealthAmount / amountPerPip;

        for (int ii = 0; ii < CeilToInt(pipCount); ii++)
        {
            pipRefs.Add(Instantiate(pipObj));
            pipRefs[ii].transform.parent = transform;
            pipRefs[ii].transform.localScale = Vector3.one;
            pipRefs[ii].transform.localPosition = new Vector3((ii - (Ceil(pipCount) / 2) + 0.5f) * pipSpacingFactor / pipScaleFactor, 0, 0);

            if (pipCount % 1 > 0 && ii == CeilToInt(pipCount) - 1)
            {
                ModifyPipBackplateWidth(pipRefs[ii], pipCount % 1);
            }
        }
    }

    private void ModifyPipBackplateWidth(GameObject pipRef, float percentage)
    {
        SpriteRenderer renderer = pipRef.transform.GetChild(0).GetComponent<SpriteRenderer>();
        Vector2 prevSize = renderer.size;
        renderer.size = new Vector2((prevSize.x - borderWidth) * percentage + borderWidth, prevSize.y);
    }

    private void ModifyPipFillWidth(GameObject pipRef, float percentage)
    {
        pipRef.transform.GetChild(1).GetComponent<SpriteRenderer>().size = new Vector2(pipFillPrevSize.x * percentage, pipFillPrevSize.y);
    }

    private void ChangePipColour(Color newColour)
    {
        for(int ii = 0; ii < transform.childCount; ii++)
        {
            transform.GetChild(ii).GetChild(1).GetComponent<SpriteRenderer>().color = newColour;
        }
        GetComponent<SpriteRenderer>().color = (newColour * 0.6f).ChangeAlpha(1);
    }

    public void UpdateHealthBar(int newValue)
    {
        for(int ii = 0; ii < pipRefs.Count; ii++)
        {
            Debug.Log(newValue - ii * amountPerPip);
            float output = newValue - ii * amountPerPip;
            output = Clamp01(output / amountPerPip);
            ModifyPipFillWidth(pipRefs[ii], output);
        }
    }

    public void UpdateHealthBarMax(int newMax)
    {
        for (int ii = transform.childCount - 1; ii >= 0; ii--)
        {
            Destroy(transform.GetChild(ii).gameObject);
        }
        pipRefs.Clear();
        CreateHealthPips(newMax);
        ChangePipColour(colour);
    }
}
