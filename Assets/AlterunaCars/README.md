# Alteruna Cars
Alteruna cars is a complete physics-based driving/racing template with sounds.\
You can access the Alteruna documentation [here](https://alteruna.github.io/au-multiplayer-api-docs).

## Requirements
The following is a list of requirements to make the template run.
* Alteruna SDK 1.2.0 or above.
* Unity 2020.3 or above.
* `Run In Bacground` set to enable in project settings.

## How it works

### Rooms
Rooms are managed by the Room menu prefab from the Alteruna package.\
From events in the multiplayer object hides that prefab when joining a room.

### Camera
The camera has a script called `CameraFollow` that will follow a target,\
that target is assigned in the `CarReset` script using the following code.
```cs
CameraFollow.Instance.Target = transform;
```

### Input
Inputs are synced using the [`Alteruna.InputSynchronizable`](https://alteruna.github.io/au-multiplayer-api-docs/html/T_Alteruna_InputSynchronizable.htm) component in `CarController`.\
See following example for how to implement InputSynchronizable using `SyncedKey` and `SyncedAxis`.
```cs
public InputSynchronizable MyInputManager;

private SyncedKey _handbrake;
private SyncedAxis _steering;
private SyncedAxis _targetTorque;

void Start()
{
	if (MyInputManager == null)
	{
		MyInputManager = GetComponent<InputSynchronizable>();
	}

	_handbrake = new SyncedKey(MyInputManager, KeyCode.Space);
	_steering = new SyncedAxis(MyInputManager, "Horizontal");
	_targetTorque = new SyncedAxis(MyInputManager, "Vertical");
}
```

### Car physics
Car physics is controlled with wheel colliders, making the feel of the vehicles very customizable.\
You can further customize the feel of the vehicle using its rigidbody and the settings in the `WheelController`.

The `CarController` handles input and sound. It also sends processes inputs to all `WheelController` that then handles all movement.

### Audio
The audio of the cars is shifted in pitch to simulate speed and engine rev.

### Syncronizables
The vehicles are synced by having both a `RigidbodySyncronizable` and `InputSynchronizable` to properly sync. If you are concerned about the data usage, you can lower how often the fixed update occurs.