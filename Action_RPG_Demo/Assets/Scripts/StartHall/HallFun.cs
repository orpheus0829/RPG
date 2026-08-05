using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HallFun : MonoBehaviour
{
    public RawImage Im;
    public List<Texture> Images;
    public float switchInterval = 5f;
    public float timer;
    public int index;

    public void Awake()
    {
        Im = GetComponent<RawImage>();
        Im.texture = Images[0];
        index = 0;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0;
            index++;
            Im.texture = Images[index % Images.Count];
        }
    }
}