using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteBodyController : MonoBehaviour
{
   public Animator remoteBodyAnimator;

    [SerializeField]
    private GameObject[] meshObjects;

    public void ForceMesh(bool enable)
    {
        foreach(GameObject o in meshObjects)
        {
            o.SetActive(enable);
        }
    }
}
