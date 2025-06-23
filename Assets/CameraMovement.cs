using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            return;
        }

        var positionSum = players
            .Select(ply => ply.transform.position)
            .Aggregate(Vector3.zero, (current, position) => current + position);
        var averagePosition = positionSum / players.Length;
        transform.position = new Vector3(
            averagePosition.x,
            averagePosition.y,
            transform.position.z
        );
    }
}
