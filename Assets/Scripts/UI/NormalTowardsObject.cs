using System;
using UnityEngine;

public class NormalTowardsObject : MonoBehaviour
{
    [SerializeField] private RectTransform uiRectTransform;

    private void Update()
    {
        AngleTowardsOwner();
    }

    private void AngleTowardsOwner()
    {
        GameObject mainCamera;
        try
        {
            mainCamera = Camera.main.gameObject;
        }
        catch (Exception e)
        {
            return;
        }
            
        uiRectTransform.LookAt(mainCamera.transform.position);
    }
    
    
}
