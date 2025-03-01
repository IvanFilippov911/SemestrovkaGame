using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField]
    private Text winnerText;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject playerStatPrefab;

    private List<Health> healths = new List<Health>();

    private int count = 0;

    public void AddHealthToUI(Health health)
    {
        healths.Add(health);
        PlayerStatsDisplay playerStatsDisplay = Instantiate(playerStatPrefab, canvas.transform).GetComponent<PlayerStatsDisplay>();
        playerStatsDisplay.Initiate(health, health.gameObject.name);
        playerStatsDisplay.transform.localPosition = Vector3.down * 100 * count++;
        health.OnDeath += CheckWinner;
    }

    private void CheckWinner()
    {
        int living = 0;
        foreach (Health health in healths)
        {
            if (health.Value > 0)
                living++;
        }

        if (living == 1)
        {
            string livingName = "";

            foreach (Health health in healths)
                if (health.Value > 0)
                    livingName = health.gameObject.name;

            winnerText.text = livingName;
            winnerText.enabled = true;
        }
    }
}
