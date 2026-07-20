using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private float drinkAmount = 25f;

    public float Drink()
    {
        return drinkAmount;
    }
}
