using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsDisplay : MonoBehaviour
{
    [SerializeField]
    private Text textName;
    
    [SerializeField]
    private Text textHealth;

    private Health healthTarget;
    public void Initiate(Health health, string name)
    {
        textHealth.text = health.Value.ToString();
        healthTarget = health;
        healthTarget.OnChange += (float changed) => { textHealth.text = changed.ToString(); };
        textName.text = name;
    }

}
