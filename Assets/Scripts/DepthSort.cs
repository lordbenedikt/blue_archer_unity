using UnityEngine;

public class DepthSort : MonoBehaviour
{
    private float startDepth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startDepth = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        var sortedDepth = startDepth + (0.0001f * pos.y);
        transform.position = new Vector3(pos.x, pos.y, sortedDepth);
    }
}
