using System.Collections;
using TMPro;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
    [SerializeField] private float textSpeed;
    [TextArea] public string insertText;
    private TMP_Text _displayText;

    void Start()
    {
        _displayText = GetComponent<TMP_Text>();
        
        StartCoroutine(TypingEffect(insertText));
    }
    
    IEnumerator TypingEffect(string text)
    {
        string textBuffer = null;
        foreach (char c in text)
        {
            textBuffer += c;
            _displayText.text = textBuffer;
            yield return new WaitForSeconds(1/textSpeed);
        }
    }
}
