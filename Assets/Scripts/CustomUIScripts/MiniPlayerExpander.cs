using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MiniPlayerExpander : MonoBehaviour {
    [SerializeField]
    private Camera minicam = null;
    [SerializeField]
    private CanvasGroup UICanvas = null;

    private Rect original;
    private Rect expanded = new Rect(Vector2.zero, Vector2.one);
    private bool expand = false;

    private void Start() {
        original = minicam.rect;
    }


    public void ToggleSize() {
        if (!expand) {
            minicam.rect = expanded;
            UICanvas.SetVisibility(false);
        }
        else {
            minicam.rect = original;
            UICanvas.SetVisibility(true);
        }
        expand = !expand;
        //RaycasExample();
    }

    //private void RaycasExample() {
        //int numCast = 10000;
        //// Perform a single raycast using RaycastCommand and wait for it to complete
        //// Setup the command and result buffers
        //var results = new NativeArray<RaycastHit>(numCast, Allocator.TempJob);

        //var commands = new NativeArray<RaycastCommand>(numCast, Allocator.TempJob);

        //// Set the data of the first command
        //Vector3 origin = Vector3.forward * -10;

        //Vector3 direction = Vector3.forward;

        //for (int i = 0; i < numCast; i++) {
        //    commands[i] = new RaycastCommand(origin, direction + (Vector3.one * (i/10)));
        //}

        //// Schedule the batch of raycasts
        //JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default);

        //// Wait for the batch processing job to complete
        //handle.Complete();

        //// Copy the result. If batchedHit.collider is null there was no hit
        //RaycastHit batchedHit = results[0];

        //int counter = 0;
        //for (int i = 0; i < results.Length; i++) {
        //    if (results[i].collider != null) {
        //        Debug.DrawLine(origin, results[i].point, Color.green, 10000);
        //        counter++;
        //    }
        //    else {
        //        Debug.DrawRay(origin, commands[i].direction, Color.red, 10000, true);
        //    }
        //}

        //// Dispose the buffers
        //results.Dispose();
        //commands.Dispose();

        //Debug.LogError($"DONE RAYCASTS {counter}/{numCast}");
    //}
}
