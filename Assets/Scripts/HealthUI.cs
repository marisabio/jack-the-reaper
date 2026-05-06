using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    float playerHealth;
    TMP_Text healthText;

    void Awake()
    {
        healthText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        playerHealth = PlayerPrefs.GetFloat("health");
        int health = Mathf.FloorToInt(playerHealth);
        healthText.text = health.ToString();
    }
}