using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonEnemy : MonoBehaviour
{
    [SerializeField]
    private float balloonRespawnTime = 4f;
    PlayerController player;
    MeshRenderer meshRenderer;
    SphereCollider sphereCollider;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        sphereCollider = gameObject.GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && player.ReturnDashing())
        {
            player.ResetDash();
            meshRenderer.enabled = false;
            sphereCollider.enabled = false;
            StartCoroutine(ReEnableComponents());
        }
    }

    IEnumerator ReEnableComponents()
    {
        yield return new WaitForSeconds(balloonRespawnTime);
        meshRenderer.enabled = true;
        sphereCollider.enabled = true;
    }
}
