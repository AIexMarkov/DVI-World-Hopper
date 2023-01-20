using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private GameObject redSphere;
    [SerializeField]
    private GameObject greenSphere;

    private bool isCheckpointActive = false;


    private void Update()
    {
        if (isCheckpointActive)
        {
            greenSphere.SetActive(true);
            redSphere.SetActive(false);
        }
        else
        {
            redSphere.SetActive(true);
            greenSphere.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        isCheckpointActive = true;
    }
}
