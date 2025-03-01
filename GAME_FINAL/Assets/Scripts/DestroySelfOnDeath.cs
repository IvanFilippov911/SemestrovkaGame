using UnityEngine;
public class DestroySelfOnDeath : MonoBehaviour
{
    [SerializeField] private Health health;



    void Start()
    {
        health.OnDeath += () => { DestroySelf(); };
    }

    private void DestroySelf()
    {
        gameObject.SetActive(false);
    }
}
