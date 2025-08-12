using UnityEngine;
using TMPro;

public class ChatMessage : MonoBehaviour
{
    [SerializeField]
    private RectTransform textHolder;
    [SerializeField]
    private TextMeshProUGUI userText;
    [SerializeField]
    private TextMeshProUGUI messageText;
    [SerializeField]
    private float spacing = 20.0f;
    [SerializeField]
    private Color noOwnerColour = Color.yellow;

    public void SetValues(string user, string message)
    {
        userText.text = user;
        messageText.text = message;
        if (user == string.Empty)
        {
            messageText.fontStyle = FontStyles.Italic;
            messageText.color = noOwnerColour;
            spacing = 0;
        }
        AdjustMessageWidth();
    }

    [ContextMenu("Adjust Message Width")]
    private void AdjustMessageWidth()
    {
        float totalWidth = textHolder.rect.width;
        float userWidth = userText.preferredWidth;
        messageText.rectTransform.sizeDelta = new Vector2(totalWidth - userWidth - spacing, 0);
    }

    public float GetPreferredHeight()
    {
        return Mathf.Max(messageText.preferredHeight, userText.preferredHeight);
    }
}
