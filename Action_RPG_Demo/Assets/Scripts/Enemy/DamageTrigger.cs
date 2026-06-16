using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public SphereCollider sc;
    public Enemy em;
    public List<Player> WaitHurt = new List<Player>();
    public void Awake()
    {
        sc = GetComponent<SphereCollider>();
        em = GetComponentInParent<Enemy>();
        sc.isTrigger = true;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && !WaitHurt.Contains(player))
            {
                WaitHurt.Add(player);
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && WaitHurt.Contains(player))
            {
                WaitHurt.Remove(player);
            }
        }
    }
}
