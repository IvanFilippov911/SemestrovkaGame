using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityAction<float> OnChange;
    public UnityAction OnDeath;

    public float MaxValue { get => maxValue; }
    public float NormalizedValue { get => value / maxValue; }
    public float Value { get => value; }

    [SerializeField] private float maxValue;

    private float value;

    void Start()
    {
        value = maxValue;

        FindAnyObjectByType<PlayerStatsUI>().AddHealthToUI(this);
    }
    void Update()
    {
        if (transform.position.y < -100)
            TakeDamage(100000);
    }


    public void TakeDamage(float damage)
    {
        if (value < damage)
            damage = value;

        value -= damage;
        OnChange?.Invoke(value);

        if (value <= 0)
        {
            value = 0;
            OnDeath?.Invoke();
        }
    }
}