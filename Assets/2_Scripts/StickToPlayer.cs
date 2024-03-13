using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToPlayer : MonoBehaviour
{
    public Transform FollowTarget;

    private void Start()
    {
        transform.SetParent(null);
    }
    private void Update()
    {
        transform.position = FollowTarget.position;
    }
}
