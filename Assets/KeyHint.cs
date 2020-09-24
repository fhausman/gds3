using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyHint : MonoBehaviour
{
    private Player playerRef;
    private GameObject currentButton = null;

    [SerializeField]
    private Sprite psButton = null;

    [SerializeField]
    private Sprite xboxButton = null;

    [SerializeField]
    private string keyboardButton = "";

    [SerializeField]
    private GameObject hints = null;

    private Image uiImage = null;
    private TextMeshProUGUI uiText = null;
    private bool active = false;

    void Start()
    {
        playerRef = GameObject.Find("Player").GetComponentInChildren<Player>();
        uiImage = hints.GetComponentInChildren<Image>();
        uiText = hints.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!active)
            return;

        switch(playerRef.CurrentControls)
        {
            case "Keyboard":
                SetKeyboard();
                break;
            case "Playstation":
                SetPS4();
                break;
            case "Xbox":
                SetXbox();
                break;
            default:
                SetKeyboard();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            active = true;
            hints.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            active = false;
            hints.SetActive(false);
        }
    }

    void SetPS4()
    {
        uiImage.gameObject.SetActive(true);
        uiImage.sprite = psButton;
        uiText.gameObject.SetActive(false);
    }

    void SetXbox()
    {
        uiImage.gameObject.SetActive(true);
        uiImage.sprite = xboxButton;
        uiText.gameObject.SetActive(false);
    }

    void SetKeyboard()
    {
        uiImage.gameObject.SetActive(false);
        uiText.gameObject.SetActive(true);
        uiText.text = keyboardButton;
    }
}
