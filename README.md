# AR Circuits Lab

An augmented reality app for iPhone where students build simple electric circuits on a real table and see how they behave.

## Features
- Place a virtual lab bench on a real surface using AR
- Build circuits with a battery, switch, bulbs, resistor and wires
- Five guided experiments: light a bulb, dim a bulb, series bulbs, parallel bulbs, fix a broken circuit
- Free build mode
- Score, mistakes and time for each session, with a history screen
- Sound effects and volume setting

## Built With
- Unity 6 (6000.4.7f1)
- AR Foundation with ARKit
- Addressables
- Universal Render Pipeline
- TextMesh Pro and the Input System

## How It Works
- **Simulation:** the circuit is solved with Ohm's law. Bulb brightness depends on power, and too much current causes a short circuit.
- **OOP:** all parts inherit from an abstract `CircuitComponent` class and override `GetResistance`.
- **Design patterns:** State (app screens), Observer (`GameEvents`) and Object Pooling (electron flow).
- **Addressables:** lab prefabs load when entering the lab and are released when leaving.
- **Data:** sessions are saved to a JSON file on the device.

## How to Run
1. Clone the repository and open it in Unity 6 (6000.4.7f1).
2. Open `Assets/Scenes/Lab.unity`.
3. To test in the editor, press Play (XR Simulation).
4. To build for iPhone, use **AR Circuits Lab › Build iPhone (Xcode project)**, then run it from Xcode on a device.

Requires an iPhone that supports ARKit.

## Project Structure
- `Assets/Scripts` – game code (AR, Circuit, Tasks, States, Events, AssetLoading, Data, UI, Audio)
- `Assets/Data` – part and experiment settings
- `Assets/Prefabs` – lab bench and circuit parts
- `Assets/Editor` – tools to build the scene, UI and iPhone build
