# What is HidVanguard?
A tool for configuring and automating [HidGuardian](https://github.com/ViGEm/HidGuardian).

# Why would I want that?
Here's my specific use-case.

I have two Thrustmaster joysticks and a set of pedals designed for racing. I want my space sim game to see the joysticks as a single device, to see the accelerator and clutch pedals as a single axis for moving forwards and backwards, and the break pedal as an axis AND a button. While HidVanguard will not do this for you, [Joystick Gremlin](https://whitemagic.github.io/JoystickGremlin/) will.

Unfortunately, Joystick Gremlin is not capable of hiding the real devices from games; your game will see input both from the real stick and from the virtual ones, which can cause problems. This is where HidGuardian comes in. But HidGuardian is difficult to set up, and requires being told about Joystick Gremlin every time it launches.

While Joystick Gremlin can manage HidGuardian itself, it needs to be run as Administrator in order to do this, which means you can't easily set it to run at Windows startup. Which is a shame, because it has a very nice feature that causes it to automatically swap to a suitable configuration when a game runs.

HidVanguard has two components:
* HidVanguard.Service: A Windows service that watches for when programs like Joystick Guardian launch, and automatically tells HidGuardian to allow said program to see hidden devices.
* HidVanguard.Config: A simple configuration tool that allows you to both set up which devices HidGuardian hides, and also which programs HidVanguard.Service watches for. **Note:** HidVanguard.Config DOES require admin rights to run, but you should only have to run this when your setup changes in some way.

# How do I use it?
tl;dr install it, run HidVanguard Config from the start menu. Double-click a device to swap it from visible to hidden. Add a process and click the ... button select a program (e.g. C:\Program Files (x86)\H2ik\Joystick Gremlin\joystick_gremlin.exe) to fill in the details automatically.

You can remove some of the details to make things easier if you wish:

* If you just enter a name, HidVanguard.Service will tell HidGuardian to allow any program that runs with that filename, regardless of where it is installed on your computer. This could potentially be dangerous, I'd not recommend it personally.
* If you enter a name and location, HidVanguard.Service will only tell HidGuardian to allow a program if it matches that name and location. If that location is in Program Files, you should be pretty safe on most machines.
* If you enter a name, location and hash, HidVanguard.Service will check the program's hash matches before telling HidGuardian to allow it. This should be the safest option, but if the program is updated you'll have to run HidVanguard Config again to update the hash.
