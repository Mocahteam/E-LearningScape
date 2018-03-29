# VRKeyboard
Tiny and lightweight VR Keyboard Gazing Control solution. 
Tested on Oculus and GearVR.

### Steps:
1. Drag Keyboard Prefab in your scene.
2. Drag GazeRaycaster to your camera, you might need to adjust the position.
3. (Highly recommended) You can remove the GazeRaycaster script in GazeRaycaster object, 
and put the script on your camera. In that way, the start position of ray will be your camera position.

### Note:
1. If you want to use GazeRaycaster to trigger other event, 
simply add your gameobject with VRGazeInteractable tag, then add an Button click event to your object. 
Because the idea behind the GazeRaycaster is to invoke an Button.onClick() event.

### Adjustment:
In Keyboard > KeyboardManager, you can set
1. Maximum input length.
2. If keyboard are uppercase at initialization. 

In GazeRaycaster,
You can adjust the loaidng time (ie. circle filling time).

If you have any issues, please contact yunhn.lee@gmail.com.

