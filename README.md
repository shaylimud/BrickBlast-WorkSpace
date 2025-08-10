# BrickBlast-WorkSpace

Codex is a Unity 6 project targeting Android builds.
It connects two projects:
Your base project — lives in /MyCode, defines the architecture, services, and coding principles.
Downloaded game — BrickBlast, lives in /BrickBlast, to be integrated into your setup.
Goal: Make BrickBlast run using MyCode’s logic and standards — without breaking your structure.
🚦 Prime Directive
MyCode is the source of truth.
Always read, follow, and extend MyCode principles first. Integrations must adapt to MyCode, not the other way around.
📚 Rules for Codex
Always follow MyCode’s patterns
Use existing services like Ads, GameState, Data, Firestore, etc.
Match MyCode’s naming conventions and structure.
Keep BrickBlast separate
BrickBlast files stay in /BrickBlast.
Do not mix BrickBlast scripts directly into MyCode.
Integration happens in its own space
Create adapters/bridges in /MyCode.Integration/BrickBlast/.
All communication between BrickBlast and MyCode happens here.
Minimal changes to BrickBlast
Remove duplicate managers already in MyCode.
Add hooks or events only when needed for integration.
Never break MyCode
If something is missing, extend MyCode cleanly without changing existing behavior.
🗺️ Quick Decision Guide
Need ads, saving, remote config? → Use MyCode services.
BrickBlast uses its own manager? → Replace with a MyCode adapter.
BrickBlast bootstraps itself? → Disable it and use MyCode bootstrap.
New file location:
Adapter/bridge → /MyCode.Integration/BrickBlast/
BrickBlast asset/script → /BrickBlast/
Reusable service/contract → /MyCode/
📋 Integration Plan
Setup
Open Unity 6 project.
Make sure Android Build Support is installed.
Import BrickBlast
Place it in /BrickBlast.
Fix any Unity package conflicts.
Connect Services
Replace BrickBlast’s internal Ads, Data, or GameState calls with MyCode equivalents.
Wire UI & Scenes
Redirect UI events and gameplay triggers to MyCode handlers.
Remove any redundant systems.
Testing
Test in Unity Play Mode.
Build & run on Android device after each major change.
Optimization
Remove unused BrickBlast assets.
Profile for performance and build size.
Documentation
Keep this README and in-code comments updated.
✅ Pre-Build Checklist
 No direct BrickBlast → MyCode script references.
 All integrations go through /MyCode.Integration/BrickBlast/.
 Android build runs without errors.
 MyCode drives all core game systems.
