using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    Checkpoint[] checkpointList;
    PlayerController player;

    private void Awake()
    {
        checkpointList = FindObjectsOfType<Checkpoint>();
        player = FindObjectOfType<PlayerController>();

        int i = 0;
        foreach (Checkpoint checkpoint in checkpointList)
        {
            checkpoint.ReceiveIndex(i);
            i++;
            checkpoint.DeactivateCheckpoint();
        }
    }

    public void UpdateCheckpoints(int index)
    {
        for (int i = 0; i < checkpointList.Length; i++)
        {
            if (i == index)
            {
                checkpointList[i].ActivateCheckpoint();
            }
            else
            {
                checkpointList[i].DeactivateCheckpoint();
            }
        }
    }

    public void RespawnPlayer()
    {
        int activeCheckpointIndex = 0;
        for (int i = 0; i < checkpointList.Length; i++)
        {
            if (checkpointList[i].IsActive())
            {
                activeCheckpointIndex = i;
                break;
            }
        }

        Transform playerRespawner = checkpointList[activeCheckpointIndex].ReturnPlayerSpawner();

        var cc = player.gameObject.GetComponent<CharacterController>();

        cc.enabled = false;
        player.transform.position = playerRespawner.position;
        cc.enabled = true;
    }
}
