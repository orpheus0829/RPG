using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadIconUpdate : MonoBehaviour
{
    public Image HeadIcon;
    public Player player;
    public void Awake()
    {
        HeadIcon = GetComponent<Image>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    public void Update()
    {
        HeadIcon.sprite = player.CurAC.Character.Roledata.RoleIcom;
    }
}
