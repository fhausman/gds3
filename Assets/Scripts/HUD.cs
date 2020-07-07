using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private Player _player = null;

    [SerializeField]
    private TextMeshProUGUI _hudText = null;

    private void Update()
    {
        _hudText.text = string.Format("Health: {0}\nLens equipped: {1}", _player.CurrentHealth, _player.IsHoldingObject ? "YES" : "NO");
    }
}
