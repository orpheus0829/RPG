using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;
    [Header("环绕距离")]
    public float distance = 4f;
    [Header("高度偏移")]
    public float height = 1.5f;
    [Header("灵敏度")]
    public float sensitivity = 150f;
    [Header("上下限制角度")]
    public float minAngle = -30f;
    public float maxAngle = 85f;

    private float rotX;
    private float rotY;
    public void Awake()
    {
        GameObject pl = GameObject.FindGameObjectWithTag("Player");
        target = pl.GetComponent<Transform>();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        rotY = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        float mX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        rotY += mX;
        rotX -= mY;
        rotX = Mathf.Clamp(rotX, minAngle, maxAngle);
        Quaternion cameraRotation = Quaternion.Euler(rotX, rotY, 0);
        Vector3 cameraDir = cameraRotation * Vector3.back;
        Vector3 cameraPos = target.position + cameraDir * distance;
        cameraPos.y += height;
        transform.position = cameraPos;
        transform.rotation = cameraRotation;
    }
}
