using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    public int foodAmount = 10; // Example value

    public void Eat(int amount)
    {
        foodAmount -= amount;
        if (foodAmount <= 0)
        {
            // Handle the bush being depleted (e.g., disable the bush, play an animation)
            Destroy(gameObject);
        }
    }
}
