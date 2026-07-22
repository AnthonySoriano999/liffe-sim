using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private float drinkAmount = 999f;

    public float Drink()
    {
        return drinkAmount;
    }
}
