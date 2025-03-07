# Magic Tween
 Extremely fast tween library implemented in Unity ECS

<img src="https://github.com/AnnulusGames/MagicTween/blob/main/MagicTween/Assets/MagicTween/Documentation~/Header.png" width="800">

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JP.md)

## Overview

Magic Tween is a high-performance tweening library implemented in Unity Entity Component System (ECS).

In addition to powerful tweening functionality compatible with traditional components, it also offers even higher-performance APIs for ECS.

## Table of Contents
- [Overview](#overview)
- [Table of Contents](#table-of-contents)
- [Features](#features)
- [Performance](#performance)
- [Samples](#samples)
- [Setup](#setup)
- [Basic Usage](#basic-usage)
- [Tweening Custom Value](#tweening-custom-value)
- [Tween Control](#tween-control)
- [Getting Tween Information](#getting-tween-information)
- [Adding Settings](#adding-settings)
- [Callbacks](#callbacks)
- [DelayedCall / Empty](#delayedcall--empty)
- [Sequence](#sequence)
- [Logging](#logging)
- [Project Settings](#project-settings)
- [TextMesh Pro](#textmesh-pro)
- [UniRx](#unirx)
- [UniTask](#unitask)
- [Implementation for ECS](#implementation-for-ecs)
- [Other Features](#other-features)
- [Experimental Features](#experimental-features)
- [Known Issues](#known-issues)
- [Support](#support)
- [License](#license)

## Features
* High-performance tweening library implemented in ECS.
* Add extension methods compatible with many components.
* Tween anything with Tween.To.
* Apply various settings using method chaining.
* Create complex animations with Sequences.
* Add processing via callbacks.
* Support for tweening TextMesh Pro.
* Support for conversion to Observable with UniRx.
* Support for async/await with UniTask.
* Even higher-performance implementation for ECS.

## Performance

<img src="https://github.com/AnnulusGames/MagicTween/blob/main/MagicTween.Benchmarks/Assets/Documentation~/benchmark_64000_floats.png" width="800">

When tweening float values of regular classes using `Tween.To()`, Magic Tween operates more than 2 to 5 times faster than other libraries.

When tweening float values within ECS components using `Tween.Entity.To()`, it is possible to achieve even higher performance.

For more details on performance, please refer to the [README](https://github.com/AnnulusGames/MagicTween/blob/main/MagicTween.Benchmarks/README.md) in the `MagicTween.Benchmarks` project.

## Samples

The `MagicTween.Samples` project includes several samples implemented using Magic Tween. For more details, please refer to the [README](https://github.com/AnnulusGames/MagicTween/blob/main/MagicTween.Samples/README.md) in the project.

## Setup

### Requirement

* Unity 2022.1 or higher
* Entities 1.0.0 or higher
* Burst 1.8.8 or higher

### Install

1. Open the Package Manager from Window > Package Manager
2. "+" button > Add package from git URL
3. Enter the following to install

```
https://github.com/AnnulusGames/MagicTween.git?path=/MagicTween/Assets/MagicTween
```

or open Packages/manifest.json and add the following to the dependencies block.

```json
{
    "dependencies": {
        "com.annulusgames.magic-tween": "https://github.com/AnnulusGames/MagicTween.git?path=/MagicTween/Assets/MagicTween"
    }
}
```

## Basic Usage

By introducing Magic Tween, numerous extension methods for creating tweens on traditional Unity components are added. Below is an example of animating the position of a Transform using these extension methods:

```cs
// Move from the current position to (1, 2, 3) over 5 seconds
transform.TweenPosition(
    new Vector3(1f, 2f, 3f), // Target value
    5f // Duration of the change
);

// Move from (0, 0, 0) to (1, 2, 3) over 5 seconds
transform.TweenPosition(
    new Vector3(0f, 0f, 0f), // Starting value
    new Vector3(1f, 2f, 3f), // Target value
    5f // Duration of the change
);
```

> **Warning**
> Do not play multiple tweens on the same parameter simultaneously. This can lead to unexpected behavior due to overlapping value changes. (Tweens on different axes, such as TweenPositionX and TweenPositionY, will work.)

> **Warning**
> Creating tweens in the editor is not supported.

### Extension Methods

Magic Tween provides extension methods for most components included in Unity. These extension methods allow for more concise and optimized code compared to `Tween.To()`. It's recommended to use extension methods when available.

You can find a list of available extension methods [here](https://github.com/AnnulusGames/MagicTween/wiki) (a wiki is currently being created).

### Classification

The extension methods added for tweening are classified into several categories:

| Method Name | Behavior |
| - | - |
| Tween... | Creates a tween for a specific field/property. |
| Punch... | Creates a tween that vibrates the value of a specific field/property. |
| Shake... | Creates a tween that randomly shakes the value of a specific field/property. |
| Set... | Adds settings to customize the tween's behavior. |
| On... | Adds callbacks at specific points during the tween. |

## Tweening Custom Value

You can animate custom value using the `Tween.To()` method:

```cs
float foo;

float endValue = 10f;
float duration = 2f;

Tween.To(
    () => foo,
    x => foo = x,
    endValue,
    duration
);
```

If you want to use only values without defining variables, you can use `Tween.FromTo()`:

```cs
float startValue = 0f;
float endValue = 10f;
float duration = 2f;

Tween.FromTo(
    x => Debug.Log("current value: " + x),
    startValue,
    endValue,
    duration
);
```

You can create tweens that follow a curve passing through multiple points using `Tween.Path()`:

```cs
Vector3 foo;

Vector3[] points;
float duration = 2f;

Tween.Path(
    () => foo,
    x => foo = x,
    points,
    duration
);
```

### Avoiding Allocations

The `To` and `FromTo` methods mentioned above cause allocations because they capture external variables. To reduce unnecessary allocations when performing tweens on an object, you can specify the target object as the first argument to avoid lambda expression allocations:

```cs
// A class with a field named 'foo'
ExampleClass target;

float endValue = 10f;
float duration = 2f;

// Pass the object as the first argument to avoid allocations
Tween.To(
    target,
    obj => obj.foo,
    (obj, x) => obj.foo = x,
    endValue,
    duration
);
```

## Tween Control

In general, created tweens play and are destroyed automatically, so there's no need for explicit control. However, there are situations where manual control is necessary, such as when dealing with tweens that loop indefinitely. In such cases, you can control tweens through the `Tween` struct:

```cs
Tween tween = transform.TweenPosition(Vector3.up, 2f);

// Start/resume the tween
tween.Play();

// Pause the tween
tween.Pause();

// Restart the tween from the beginning
tween.Restart();

// Complete the tween
tween.Complete();

// Kill the tween
tween.Kill();

// Complete and kill the tween
tween.CompleteAndKill();
```

You can also perform operations on all playing tweens collectively. If you specify an ID as an argument, you can target only the tweens with matching IDs:

```cs
// Kill all playing tweens
Tween.KillAll();

// Complete all tweens with an ID of 1
Tween.CompleteAll(1);

// Pause all tweens with an ID of "Alpha"
Tween.PauseAll("Alpha");
```

## Getting Tween Information

You can check if a tween is currently active using `IsActive()`. When performing operations on tweens, it's a good practice to check for activity, especially when there's a possibility that the tween may not be active:

```cs
// Kill the tween if it's active
if (tween.IsActive()) tween.Kill();
```

You can also retrieve the duration of a tween using `GetDuration()`:

```cs
float duration = tween.GetDuration();
```

## Adding Settings

You can customize the behavior of a tween using the Set methods. These methods can be chained together for concise code. The following code is an example of applying custom settings to a tween:

```cs
transform.TweenLocalScale(Vector3.one * 2f, 5f)
    .SetEase(Ease.OutSine) // Set the easing function to OutSine
    .SetLoops(3, LoopType.Restart) // Repeat 3 times with restart behavior
    .SetDelay(1f); // Delay the start by 1 second
```

### SetEase

Sets the easing function to use for the tween. You can also use your own easing function by passing an `AnimationCurve`.

### SetLoops

Sets the number of times the tween should loop. By default, it's set to 1. You can create a tween that loops infinitely by setting it to -1. You can also specify the loop behavior using the second argument, `LoopType`.

### SetPlaybackSpeed

Sets the playback speed of the tween. The default is 1, and negative values are not supported.

### SetDelay

Sets a delay in seconds before the tween starts.

### SetIgnoreTimeScale

Ignores the effect of TimeScale.

### SetRelative

Sets the end value as a relative value from the start value.

### SetInvert

Swaps the start and end values. You can adjust the behavior using the `InvertMode`.

| InvertMode | Behavior |
| - | - |
| InvertMode.None | Moves from the start value to the end value as usual. |
| InvertMode.Immediate | Moves to the end value as soon as the tween starts and then transitions towards the start value. |
| InvertMode.AfterDelay | Waits until the tween starts, then moves to the end value and transitions towards the start value. |

### SetId

Assigns an ID to the tween. This allows you to operate on tweens with the same ID in bulk when performing operations like KillAll. You can pass an int or a string with a length of 32 bytes or less as an ID (default is int 0 or an empty string).

### SetLink

Links the tween's lifecycle to a GameObject. You can change the behavior by setting the `LinkBehaviour` as the second argument. However, regardless of the option set, Kill will be called on OnDestroy.

### SetAutoPlay

Sets whether the tween should automatically play (default is true). If set to false, you need to manually call `Play()` to start the tween.

### SetAutoKill

Sets whether the tween should automatically be killed at the end (default is true). If set to false, you need to manually call `Kill()` to remove the tween. This option is useful when you want to reuse the same tween multiple times.

### SetFrequency (Punch, Shake)

Available options for Punch and Shake tweens to set the frequency of vibration (default is 10).

### SetDampingRatio (Punch, Shake)

Available options for Punch and Shake tweens to set the damping ratio of vibration. A value of 1 will completely dampen the vibration at the end, and a value of 0 will result in no damping (default is 1).

### SetRandomSeed (Shake)

Available option for Shake tweens to set the seed value for the random numbers used in vibration. This option must be applied before playback.

### SetPathType (Path)

Available option for Path-based tweens to set how the points are connected.

| PathType | Behavior |
| - | - |
| PathType.Linear | Connects each point using a straight line. |
| PathType.CatmullRom | Connects each point using a Catmull-Rom spline curve. |

### SetClosed (Path)

Available option for Path-based tweens to set whether the path is closed, allowing it to return to the starting point.

### SetRoundingMode (int, int2, int3, int4, long)

Sets the rounding mode for decimal values. This option is applicable only to integer-based types.

| RoundingMode | Behavior |
| - | - |
| RoundingMode.ToEven | Default setting. Rounds the value to the nearest integer, and if the value is midway, it rounds to the nearest even integer. |
| RoundingMode.AwayFromZero | Rounds the value to the nearest integer, and if the value is midway, it rounds away from zero. |
| RoundingMode.ToZero | Rounds the value towards zero. |
| RoundingMode.ToPositiveInfinity | Rounds the value towards positive infinity. |
| RoundingMode.ToNegativeInfinity | Rounds the value towards negative infinity. |

### SetScrambleMode (string)

Allows you to fill unrevealed characters with random characters. This option is only applicable to string tweens.

| ScrambleMode | Behavior |
| - | - |
| ScrambleMode.None | Default setting. Nothing is displayed for unrevealed parts. |
| ScrambleMode.Uppercase | Fills unrevealed parts with random uppercase letters. |
| ScrambleMode.Lowercase | Fills unrevealed parts with random lowercase letters. |
| ScrambleMode.Numerals | Fills unrevealed parts with random numbers. |
| ScrambleMode.All | Fills unrevealed parts with random uppercase letters, lowercase letters, or numbers. |
| (ScrambleMode.Custom) | Fills unrevealed parts with random numbers from the specified string. This option cannot be explicitly set and is determined when passing a string as an argument to SetScrambleMode. |

### SetRichTextEnabled (string)

Enables RichText support, allowing text with RichText tags to be animated with proper character advancement. This option is only applicable to string tweens.

## Callbacks

When you want to perform some actions at specific times, such as the start or completion of a tween, you can use the On-series methods. Callback methods, like other settings, can be written using method chaining.

```cs
transform.TweenPosition(new Vector3(1f, 2f, 3f), 5f)
    .SetLoops(5)
    .OnUpdate(() => Debug.Log("update"))
    .OnStepComplete(() => Debug.Log("step complete"))
    .OnComplete(() => Debug.Log("complete"));
```

> **Note**
> When you set up any callback, the first callback creates an allocation for generating the component. Additionally, enabling one or more callbacks can reduce performance during playback. In most cases, the impact on performance is minimal, but it's recommended to avoid using callbacks when creating a large number of tweens.

### OnPlay

Called when the tween starts playing. Unlike OnStart, it ignores delays set with SetDelay and is also called if Play is invoked after a pause.

### OnStart

Called when the tween begins its operation. If a delay is set with SetDelay, it is called after the delay has passed.

### OnUpdate

Called every frame during the tween's playback.

### OnStepComplete

Called at the end of each loop when SetLoops is configured.

### OnComplete

Called when the tween is completed.

### OnKill

Called when the tween is killed.

## DelayedCall / Empty

You can create a tween that performs a specified action after a certain delay using `Tween.DelayedCall()`.

```cs
// Display a log after 3 seconds
Tween.DelayedCall(3f, () => Debug.Log("delayed call"));
```

Additionally, you can create an empty tween using `Tween.Empty()`.

```cs
// A tween that completes after 3 seconds
Tween.Empty(3f);

// DelayedCall() internally calls the following code
Tween.Empty(3f)
    .OnStepComplete(() => Debug.Log("delayed call"))
    .OnComplete(() => Debug.Log("delayed call"));
```

## Sequence

A Sequence is a feature used to group multiple tweens together. By using Sequences, you can easily create complex animations by combining multiple tweens.

### Creating a Sequence

You can obtain a new Sequence from `Sequence.Create()`:

```cs
// Create a new Sequence
Sequence sequence = Sequence.Create();
```

### Adding Tweens

Next, you add the tweens you want to include in the Sequence. There are various methods available to add tweens to a Sequence. By using these methods, you can combine tweens and build complex animations.

Sequences can be nested regardless of their hierarchy. Options and callbacks such as `SetDelay` and `SetLoops` will also work for the Sequence after it has been added.

### Append

The `Append()` method adds tweens to the end of the Sequence. The added tweens will play in sequence when you play the Sequence.

```cs
// Append a tween to the end
sequence.Append(transform.TweenPosition(new Vector3(1f, 0f, 0f), 2f))
    .Append(transform.TweenPosition(new Vector3(1f, 3f, 0f), 2f));
```

You can use `AppendInterval()` and `AppendCallback()` to add delays or callbacks:

```cs
// Append a delay to the end
sequence.AppendInterval(1f);

// Append a callback to the end
sequence.AppendCallback(() => Debug.Log("Hello!"));
```

### Prepend

If you want to add tweens to the beginning, you can use `Prepend()`. In this case, the tweens already added will move back by the duration of the new prepend tween.

```cs
// Prepend a tween to the beginning
sequence.Prepend(transform.TweenPosition(new Vector3(1f, 0f, 0f), 2f));
```

You can also use `PrependInterval()` and `PrependCallback()`:

```cs
// Prepend a delay to the beginning
sequence.PrependInterval(1f);

// Prepend a callback to the beginning
sequence.PrependCallback(() => Debug.Log("Hello!"));
```

### Join

To concatenate a tween with the ones added before it, you can use `Join()`. Tweens added with `Join()` will play concurrently with the previously added tween.

```cs
sequence.Append(transform.TweenPosition(new Vector3(1f, 0f, 0f), 2f));

// Join with the previous tween
sequence.Join(transform.TweenPosition(new Vector3(1f, 3f, 0f), 2f));
```

### Insert

If you want to insert a tween at an arbitrary point, you can use `Insert()`. The tween added with `Insert()` will operate independently of other tweens and will start playing once it reaches the specified position.

```cs
// Insert a tween at 1 second from the start
sequence.Insert(1f, transform.TweenPosition(new Vector3(1f, 0f, 0f), 2f));
```

You can also insert callbacks using `InsertCallback()`:

```cs
// Insert a callback at 1 second from the start
sequence.InsertCallback(1f, () => Debug.Log("Hello!"));
```

### Implicit Conversion to Tween

A `Sequence` can be implicitly converted to a `Tween`, allowing you to assign it directly:

```cs
Sequence sequence = Sequence.Create();

// Can be assigned directly to a Tween variable
Tween tween = sequence;
```

### Usage Considerations

Here are some important points to keep in mind when using Sequences:

* You cannot add tweens to a Sequence while it is playing.
* You cannot add tweens with infinite loops to a Sequence.
* Once a tween is added to a Sequence, it becomes locked, and you cannot access it individually. Be cautious, as you cannot manipulate the individual tweens within a Sequence.
* You cannot include the same tween in multiple Sequences.

## Logging

If you want to perform logging of Tween callbacks and values for debugging purposes, you can easily achieve this using dedicated extension methods. Note that these logs will only be displayed if `MagicTweenSettings`' `LoggingMode` is set to `Full`.

```cs
using MagicTween;
using MagicTween.Diagnostics; // Enable debug extension methods

// Log specific callback
transform.TweenPosition(Vector3.up, 5f)
    .LogOnUpdate();

// Log all callbacks together
transform.TweenEulerAngles(new Vector3(0f, 0f, 90f), 5f)
    .LogCallbacks();

// You can also assign a name for identification
transform.TweenLocalScale(Vector3.one * 2f, 5f)
    .LogCallbacks("Scale");

float foo;
// You can also log values (current value per frame)
Tween.To(() => foo, x => foo = x, 5f, 10f)
    .LogValue();
```

## Project Settings

You can customize Tween's initial settings and logging preferences.

### Creating MagicTweenSettings

Create a `MagicTweenSettings` asset to store your configuration by navigating to `Assets > Create > Magic Tween > Magic Tween Settings`.

> **Note**
> The created `MagicTweenSettings` will be automatically added to the project's Preload Assets. If the settings are not being loaded, ensure that `MagicTweenSettings` is included in the Preload Assets.

### Logging Mode

Set whether logging is enabled or not.

| LoggingMode | Behavior |
| - | - |
| LoggingMode.Full | Display all logs, including Log-related extension methods, in the Console. |
| LoggingMode.WarningsAndErrors | Display only warnings and errors in the Console. |
| LoggingMode.ErrorsOnly | Display only errors in the Console. |

### Capture Exceptions

When set to "On," exceptions that occur internally in Tweens will be logged as warnings. When set to "Off," exceptions will be logged as regular exceptions.

### Default Tween Parameters

You can modify the default settings for Tweens.

### Changing Settings from Script

You can access these settings from the `MagicTweenSettings` class in your script.

```cs
// Change Logging Mode from script
MagicTweenSettings.loggingMode = LoggingMode.ErrorsOnly;
```

## TextMesh Pro

Magic Tween supports TextMesh Pro (TMP) and allows you to tween text characters individually using the `TweenChar` extension methods. Here's how you can use it:

```cs
TMP_Text tmp;

// GetCharCount retrieves the number of tweenable characters
for (int i = 0; i < tmp.GetCharCount(); i++)
{
    tmp.TweenCharScale(i, Vector3.zero).SetInvert().SetDelay(i * 0.07f);
}
```

You can stop the tweens associated with TMP_Text and reset the text's decoration to its initial state using `ResetCharTweens()`:

```cs
// Stop the character tweens and reset the decoration to the initial state
tmp.ResetCharTweens();
```

Character tweens are powered by the `TMPTweenAnimator` class internally, which you can access using `GetTMPTweenAnimator()`:

```cs
// Get the internal TMPTweenAnimator
TMPTweenAnimator tmpAnimator = tmp.GetTMPTweenAnimator();

// Extension methods for TMP_Text use methods from TMPTweenAnimator internally
tmpAnimator.TweenCharOffset(0, Vector3.up);

// You can directly set parameters for each character using SetChar**
tmpAnimator.SetCharScale(1, Vector3.one * 2f);
tmpAnimator.SetCharColor(1, Color.red);

// ResetCharTweens() equivalent
tmpAnimator.Reset();

// GetCharCount() equivalent
tmpAnimator.GetCharCount();
```

## UniRx

By integrating UniRx, you can convert Tween callbacks and Tween operations into Observables.

### Converting Callbacks to Observables

You can use methods like `OnUpdateAsObservable()` to convert Tween callbacks into Observables. For example:

```cs
float foo;

Tween.To(() => foo, x => foo = x, 10f, 10f)
    .OnUpdateAsObservable()
    .Subscribe(_ =>
    {
        Debug.Log("update!");
    });
```

### Converting Tweens to Observables

You can use `ToObservable()` to transform a Tween into an Observable that emits values each frame. For example:

```cs
Tween.FromTo(0f, 10f, 10f, null)
    .ToObservable()
    .Where(x => x >= 5f)
    .Subscribe(x =>
    {
        Debug.Log("current value: " + x);
    });
```

## UniTask

By integrating UniTask, you can use async/await to handle Tween waiting operations.

```cs
var tween = transform.TweenPosition(Vector3.up, 2f);

// You can directly await the Tween (waits until the Tween is killed)
await tween;
```

You can use `AwaitForKill()` and pass a `CancellationToken` to handle cancellation.

```cs
// Create a CancellationTokenSource
var cts = new CancellationTokenSource();

// Wait until the Tween is killed, passing the CancellationToken
await transform.TweenPosition(Vector3.up, 2f)
    .AwaitForKill(cancellationToken: cts.Token);
```

You can also wait for other events like completion using `AwaitForComplete()` or `AwaitForPause()`.

```cs
// Wait until the Tween completes
await transform.TweenPosition(Vector3.up, 2f).AwaitForComplete();
```

Furthermore, you can specify the `CancelBehaviour` to determine the behavior on cancellation.

```cs
var cts = new CancellationTokenSource();

// On cancellation, call Complete and throw OperationCanceledException
await transform.TweenPosition(Vector3.up, 2f)
    .AwaitForComplete(CancelBehaviour.CompleteAndCancelAwait, cts.Token);
```

| CancelBehaviour | Behavior on Cancellation |
| - | - |
| CancelBehaviour.Kill | Calls the Kill method. |
| CancelBehaviour.Complete | Calls the Complete method. |
| CancelBehaviour.CompleteAndKill | Calls both Complete and Kill methods. |
| CancelBehaviour.CancelAwait | Throws an OperationCanceledException without calling Complete or Kill. |
| CancelBehaviour.KillAndCancelAwait | Default behavior. Calls Kill and throws an OperationCanceledException. |
| CancelBehaviour.CompleteAndCancelAwait | Calls Complete and throws an OperationCanceledException. |
| CancelBehaviour.CompleteAndKillAndCancelAwait | Calls both Complete and Kill methods and throws an OperationCanceledException. |

## Implementation for ECS

Magic Tween provides APIs for implementing Tween for ECS (Entity Component System), allowing you to create high-performance Tweens compared to regular ones.

### Creating a Translator

To Tween the values of specific Components, you need to create a `Translator` for the target Component, along with a System to make it work. 

As an example, let's create a Translator for the following Component:

```cs
public struct ExampleComponent : IComponentData
{
    public float value;
}
```

First, define a structure that implements `ITweenTranslator`, like this:

```cs
public struct ExampleTranslator : ITweenTranslator<float, ExampleComponent>
{
    // A property to track the target Entity, used in the System
    public Entity TargetEntity { get; set; }

    // Apply the value to the Component
    public void Apply(ref ExampleComponent component, in float value)
    {
        component.value = value;
    }

    // Return the current value of the Component
    public float GetValue(ref ExampleComponent component)
    {
        return component.value;
    }
}
```

Next, create a System class that inherits from `TweenTranslationSystemBase`, providing the Translator you created as a type argument. Keep the internal of the derived class empty, as the base class already implements the necessary logic.

```cs
public partial class ExampleTweenTranslationSystem : TweenTranslationSystemBase<float, ExampleComponent, ExampleTranslator> { }
```

Now, you're prepared to Tween values.

### Tweening Component Values

To Tween values using the Translator you created, you can use `Tween.Entity.To()` or `Tween.Entity.FromTo()`. Provide the type of your Translator as type arguments.

```cs
var entity = EntityManager.CreateEntity();
EntityManager.AddComponent<ExampleComponent>(entity);

// Tween the value of ExampleComponent's 'value' field to 5 over 10 seconds
Tween.Entity.To<ExampleTranslator>(entity, 5f, 10f);
```

Just like regular Tweens, you can chain methods to add settings:

```cs
Tween.Entity.FromTo<ExampleTranslator>(entity, 0f, 5f, 10f)
    .SetEase(Ease.OutSine)
    .SetLoops(3, LoopType.Restart)
    .SetDelay(1f);
```

You can also add these Tweens to a Sequence:

```cs
var entity1 = EntityManager.CreateEntity();
var entity2 = EntityManager.CreateEntity();
EntityManager.AddComponent<ExampleComponent>(entity1);
EntityManager.AddComponent<ExampleComponent>(entity2);

var tween1 = Tween.Entity.To<ExampleTranslator>(entity1, 5f, 10f);
var tween2 = Tween.Entity.To<ExampleTranslator>(entity2, 5f, 10f);

var sequence = Sequence.Create()
    .Append(tween1)
    .Append(tween2);
```

> **Warning**
> Avoid applying multiple concurrent Tweens with the same Translator to the same Entity. This may lead to overlapping value modifications and unexpected behavior.

### Built-in Translators

Under `MagicTween.Translators`, you can find built-in Translators for ECS components.

Currently, there's a Translator that works with `LocalTransform`.

If you have the Entities Graphics package installed, you'll find Translators for tweening Material properties.

### Limitations

Creating and manipulating Tweens/Sequences is only supported on the main thread. You cannot create new Tweens or perform operations like Kill or Complete from a Job. To alleviate this limitation, there's ongoing development to introduce functionality for creating/operating Tweens using a dedicated CommandBuffer.

## Other Features

### EaseUtility

The easing functions used internally by Tween can be accessed via EaseUtility.

```cs
float value1 = EaseUtility.Evaluate(0.5f, Ease.OutQuad);
float value2 = EaseUtility.InOutQuad(0.5f);
```

## Experimental Features

The `MagicTween.Experimental` namespace contains features that are currently under development. These features are available for use but come with no guarantees, and there may be breaking changes without notice.

## Known Issues

### Performance Drop in the Editor

ECS performs numerous checks to enhance safety, which leads to decreased performance within the editor. This performance degradation is particularly noticeable when creating Tweens and, in some cases, can result in processing times several times longer than usual.

It's important to note that these safety checks are disabled in the build, so performance measurements should always be conducted in the build environment.

### Performance Drop in WebGL

While it's possible to use ECS in WebGL, limitations in WebGL's specifications, such as the absence of multi-threading and SIMD, disable optimizations like the Job System and Burst. ECS achieves its high performance through the Job System and Burst, making performance degradation unavoidable in WebGL (hence, there are few advantages to using ECS on WebGL currently).

Magic Tween optimizes the calculation part of Tweens using the Job System and Burst. Therefore, on WebGL, performance suffers for the reasons mentioned above. While these effects are typically not noticeable, please be mindful of this when creating a large number of Tweens.

## Support

Forum: https://forum.unity.com/threads/magic-tween-extremely-fast-tween-library-implemented-in-ecs.1490080/

## License

[MIT License](LICENSE)