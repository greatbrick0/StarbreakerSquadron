using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class VersionLabel : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = Application.version;
    }
}
