using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;
    public float enlargedFontSize = 36f;
    public Color hoverColor = new Color(1f, 1f, 1f, 1f); 

    private float originalFontSize;
    private Color originalColor;

    private void Start()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>();
        }

        originalFontSize = buttonText.fontSize;
        originalColor = buttonText.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.fontSize = enlargedFontSize;
        buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.fontSize = originalFontSize;
        buttonText.color = originalColor;
    }
}