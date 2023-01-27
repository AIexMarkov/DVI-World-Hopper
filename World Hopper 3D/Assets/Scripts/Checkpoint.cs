using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Important: Add Checkpoint Manager into the scene")]

    [SerializeField]
    private GameObject redSphere;
    [SerializeField]
    private GameObject greenSphere;

    private CheckpointManager checkpointManager;
    private bool isCheckpointActive = false;
    private int index = 0;
    private Transform playerSpawner;


    private void Awake()
    {
        checkpointManager = FindObjectOfType<CheckpointManager>();
        playerSpawner = transform.Find("SpawnPlayer");
    }

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
            checkpointManager.UpdateCheckpoints(index);
        }
    }

    public void ActivateCheckpoint()
    {
        isCheckpointActive = true;
    }

    public void DeactivateCheckpoint()
    {
        isCheckpointActive = false;
    }

    public void ReceiveIndex(int indx)
    {
        index = indx;
    }

    public Transform ReturnPlayerSpawner()
    {
        return playerSpawner;
    }

    public bool IsActive()
    {
        return isCheckpointActive;
    }
}
