# Alteruna Multiplayer Racing Template

## Overview
Alteruna cars is a complete physics-based multiplayer driving/racing template with tire squeaking and engine rev sound effects.\
Tweak the physics in the car body to get the game feeling the way you want.

## Requirements
The following is a list of requirements to make the template run.
* Alteruna SDK 1.2.0 or above.
* Unity 2020.3 or above.

## Documentation

For detailed documentation and guides on how to use the Alteruna Multiplayer, refer to the [Alteruna documentation](https://alteruna.github.io/au-multiplayer-api-docs). The documentation provides step-by-step instructions and explanations of various features.

## Support and Community

If you encounter any issues or have questions about the template or Alteruna Multiplayer, you can reach out for support. Visit [our Discord server](https://discord.gg/QT8KTe2Hzk) to access the support resources and engage with the community. The community forums and channels are excellent places to connect with other developers, share your experiences, and collaborate on projects.

## Contributions

Contributions to the [Alteruna Multiplayer Racing Template](https://github.com/Alteruna/Multiplayer-Racing-Template) are welcome! If you have ideas for improvements or would like to contribute code. Your contributions can help enhance the template and benefit the entire community of developers.

## Acknowledgments

The Alteruna Multiplayer Racing Template is built upon the collective knowledge and efforts of many developers and contributors. We would like to express our gratitude to all those who have shared their expertise, resources, and code to make this template possible.

## Disclaimer

The Alteruna Multiplayer Racing Template is provided as-is, without any warranty or guarantee of its suitability for any particular purpose. While we strive to maintain the template and provide support, we cannot be held responsible for any issues or damages that may arise from its use. Always thoroughly test and verify the template in your specific project environment.

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