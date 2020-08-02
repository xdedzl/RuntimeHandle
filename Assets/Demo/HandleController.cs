using UnityEngine;
using xdedzl.RuntimeHandle;

public class HandleController : MonoBehaviour
{
    private RuntimeHandle runtimeHandle;

    void Start()
    {
        runtimeHandle = GetComponent<RuntimeHandle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            runtimeHandle.SetMode(TransformMode.Position);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            runtimeHandle.SetMode(TransformMode.Rotation); ;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            runtimeHandle.SetMode(TransformMode.Scale);
        }
    }
}
