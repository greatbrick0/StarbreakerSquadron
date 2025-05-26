using UnityEngine;
using TMPro;


public class ShipSelectDisplay : MonoBehaviour
{
    [SerializeField]
    Transform shipHolder;
    [SerializeField, Display]
    private int selectedIndex = 0;
    [SerializeField]
    private float shipSpacing = 400;
    [SerializeField]
    private GameObject leftArrow, rightArrow;
    [SerializeField]
    private TMP_Text shipNameLabel;
    [Header("Animation")]
    [SerializeField]
    private AnimationCurve curve;
    [SerializeField, Display]
    private float animRemaining = 0.0f;
    private int animDirection = 0;
    [SerializeField]
    private float animSpeed = 1.0f;

    private void Start()
    {
        MoveSelectedIndex(0);
    }

    private void Update()
    {
        if (animRemaining > 0.0f)
        {
            animRemaining -= animSpeed * Time.deltaTime;
            PositionShips();
        }
    }

    public void MoveSelectedIndex(int direction)
    {
        selectedIndex += direction;
        animDirection = direction;
        animRemaining = 1.0f;
        leftArrow.SetActive(selectedIndex != 0);
        rightArrow.SetActive(selectedIndex != shipHolder.childCount - 1);
        shipNameLabel.text = shipHolder.GetChild(selectedIndex).name;
    }

    private void PositionShips()
    {
        for (int ii = 0; ii < shipHolder.childCount; ii++)
        {
            float offset = shipSpacing * (ii - selectedIndex + CurveAmount());
            shipHolder.GetChild(ii).localPosition = Vector3.right * offset;
            shipHolder.GetChild(ii).localScale = Vector3.one * Mathf.Min(shipSpacing / (Mathf.Abs(offset) + shipSpacing), 0.9f);
        }
    }

    private float CurveAmount()
    {
        return curve.Evaluate(1 - animRemaining) * animDirection;
    }
}
