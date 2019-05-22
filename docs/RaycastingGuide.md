# Raycasting guide
This guide assumes that the developer is already able to acquire and set the virtual position of the subject within virtual maze and also able to synchronize the gaze data with the time and position of the subject.

## VirtualMaze Setup
The current setup of the cue and subject is as follows.

The image cues exists as image components residing in a Canvas in World Space and this canvas is set as a child of the gameobject representing the subject. This setup allows the cues to follow the subject easily.

## Full Code
Full code can be found at ScreenSaver.cs.

Functions of interest are:
  - ProcessSessionData()
  - LogGazeObjectInScene()

## Overview of Method (Pseudocode)
```
For each frame from Session Logs{

  - Move subject to the position recorded in the session logs

  For each gaze data belonging to the frame from EDF file (eyelink){

    - Fire 2D ray cast on the canvas (GraphicRaycaster) originating from
     the camera

    if( the ray hits a cue which is on the canvas) {

      - Process the data and return or print the information needed
      - Continue with the next gaze data point

    }else{

      - Fire 3D ray cast into the scene originating from the camera
      - Process the ray cast hit and return or print required data

    }
  }
}
```

## Method
### GraphicRaycaster

Uses:
- [PointerEventData](https://docs.unity3d.com/ScriptReference/EventSystems.PointerEventData.html)
- [GraphicRaycaster](https://docs.unity3d.com/ScriptReference/UI.GraphicRaycaster.Raycast.html)

GraphicRaycaster is used by canvas objects to detect what has been clicked/selected. GraphicRaycaster.Raycast() requires a PointerEventData and will return a list of objects that the raycast hits.


```Csharp
//Prepare empty list for the results of the graphic ray cast
List<RaycastResult> results = new List<RaycastResult>();

//Create a new PointerEventData
PointerEventData data = new PointerEventData(EventSystem.current);

//Update the position to the gaze data
data.position = sample.RightGaze;

//fire the graphic ray cast. Function will fill the list
cueCaster.Raycast(data, results);

if (results.Count > 0) {
    //process and record data

    //get image's position on the screen
    Vector2 objPosition =   RectTransformUtility.WorldToScreenPoint(viewport, results[0].gameObject.transform.position);

    //process hit
    print($"t:{sample.time}, name:{results[0].gameObject.name}, 2d:{objPosition - results[0].screenPosition}, objhit:{results[0].gameObject.transform.position}, pointHit: WIP");

    print(results[0].screenPosition);
    }
    //end the graphic raycast for this gaze data point
    return;
}
//continues to a 3D raycast in the scene
```

Since the cues are at the front of everything, it is recommended to do a GraphicRaycast.Raycast() first before a Physics.Raycast().


### Physics Raycast

Uses:
- [Physics.Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html)

```Csharp
//create a ray from the camera
//RightGaze is a 2D Vector containing the x and y position of the gaze data
Ray r = viewport.ScreenPointToRay(sample.RightGaze);

//Raycast
if (Physics.Raycast(r, out RaycastHit hit)) {
  lineRenderer?.SetPositions(new Vector3[] { viewport.transform.position, hit.point });

  //acquire the transform of the hit.
  Transform objhit = hit.transform;

  //process and record the hit
  print($"t:{sample.time}, name:{hit.transform.name}, 2d:{hit.point - objhit.position}, objhit:{objhit.position}, pointHit:{hit.point}");
}
//continues to the next gaze data point

```
