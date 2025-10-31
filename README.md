# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Humanoid Animation Rigging

> Quick overview: A lightweight head/aim "look target" helper. Keeps a tracking target in front of the character, mirrors when the real target goes behind, clamps vertical angles, and applies smoothing—ideal for Animation Rigging Multi-Aim/LookAt or Animator LookAt.

A single-component utility that produces a stable, believable look/aim target in front of a character. It mirrors when the real target goes behind, enforces vertical angle limits to avoid neck-breaking poses, and smooths the motion to prevent jitter—perfect as input for Animation Rigging constraints or Animator LookAt.

![screenshot](Documentation/Screenshot.png)

## Features
- Target-follow for head/eyes
  - Follows a source object from a source position (e.g., head/eyes transform)
  - Mirrors when the target moves behind, based on horizontal dot product
  - Maintains a minimum distance to avoid jitter when too close
- Pleasant, stable motion
  - Dual-stage smoothing: internal position smoothing and final blend
  - Tunable smoothing and blend weight
- Vertical angle constraints
  - Clamp min/max vertical angles (degrees) before distance normalization
  - Handles directly-above/below cases robustly
- Simple setup and runtime-friendly
  - Single component: `HeadTrackingTargetFollow`
  - Works with Animator LookAt or Animation Rigging constraints

## Requirements
- Unity Editor 6000.0+
- Runtime assembly included (`UnityEssentials.AnimationRigging.asmdef`)
- Optional: Unity Animation Rigging package if you plan to drive Multi-Aim/Multi-Parent/LookAt constraints

## Usage
1) Create a target object for your rig
   - Create an empty GameObject (e.g., `HeadLookTarget`) and add `HeadTrackingTargetFollow` to it.
   - This object will be used as the LookAt/Aim target for your rig or Animator.

2) Assign references
   - Source Object (`_sourceObject`): the object to follow (e.g., a player/camera/point of interest).
   - Source Position (`_sourcePosition`): the character's head/eyes transform defining position and forward.
   - Optional Offset (`_offset`): offset from the source position used as the origin for targeting.

3) Wire into your rig
   - Animation Rigging: Set your Multi-Aim or LookAt constraint target to the `HeadLookTarget` transform.
   - Animator LookAt: Each frame, pass `HeadLookTarget.transform.position` to `Animator.SetLookAtPosition`.

4) Tune behavior
   - Weight (`_weight`): how strongly to mirror when the target is behind (0 = never mirror, 1 = full mirror).
   - Smoothness (`_smoothness`): final blend to smoothed position (0 = snappy, 1 = smoothest).
   - Vertical Limits: `minVerticalAngle` and `maxVerticalAngle` in degrees.

5) Play and test
   - Move the source object around the character. The look target will remain stable in front, mirror behind, and respect vertical limits.

## How It Works
- Direction analysis
  - Computes flat (XZ) directions for the source forward and the vector to the target.
  - Uses the dot product against a zero threshold; when negative (behind), it mirrors the local target via `Vector3.Reflect`.
  - The mirror intensity is remapped from dot ∈ [0, -1] to [0, 1] and scaled by `_weight`.
- Vertical clamping
  - Converts current elevation to degrees using atan2, clamps to `[minVerticalAngle, maxVerticalAngle]`, and reconstructs the vector.
  - Special-cases directly-above/below to avoid instability.
- Distance and smoothing
  - Enforces a minimum distance (default 1m) after vertical clamp to reduce jitter.
  - Applies two lerps: an internal smoothing toward the final world position, then blends by `(1 - _smoothness)` to set `transform.position`.

## Notes and Limitations
- Required references: `_sourceObject` and `_sourcePosition` must be assigned; otherwise the component does nothing.
- Coordinate spaces: The helper operates in world space using the source position and forward as frame of reference.
- Minimum distance: Hard-coded to 1m in this version; adjust in code if you need a different threshold.
- Mirroring threshold: Fixed at `dot < 0` (directly behind). Expose the threshold if you need earlier/later mirroring.
- Attributes: Uses `Foldout` from UnityEssentials. If you don’t have that attribute in your project, it will compile fine but the inspector grouping won’t apply.

## Files in This Package
- `Runtime/HeadTrackingTargetFollow.cs` – Head/aim tracking target helper with mirroring, clamping, and smoothing
- `Runtime/UnityEssentials.AnimationRigging.asmdef` – Runtime assembly definition

## Tags
unity, animation-rigging, look-at, aim, head-tracking, humanoid, smoothing, mirror, constraints, runtime
