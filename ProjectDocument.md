# Game Basic Information #

## Summary ##

**Octo**

Created by Dominic Quintero, Jackie Trinh, Edward John, Yudi Lai, Abhi Sohal, Atharav Samant, Jared Givens, Catherine Win

In our sci-fi top-down multiplayer shooter, you and your team assume the role of skilled "Mercenaries" tasked with clearing out spaceships for bounties. Whether you venture solo, embracing the high-risk, high-reward nature, or join forces with friends, you'll explore abandoned or over-ridden spaceships. The core gameplay revolves around earning experience points (XP) and gold to enhance your character's abilities, stats, and acquire cosmetic items or temporary power-ups, enriching your runs. The key attraction lies in the race against time and efficiency. The faster and more effectively you navigate through a spaceship, the greater the rewards. As you progress, the challenge escalates, with each subsequent floor of the ship presenting tougher adversaries and obstacles. Players can opt to leave with their loot or risk going deeper for better rewards. Eventually, the ship will end and you will come in contact with a boss. Once you beat the boss the players will be rewarded with customized loot that matches the boss they killed.

## Gameplay Explanation ##

**In this section, explain how the game should be played. Treat this as a manual within a game. It is encouraged to explain the button mappings and the most optimal gameplay strategy.**


**If you did work that should be factored in to your grade that does not fit easily into the proscribed roles, add it here! Please include links to resources and descriptions of game-related material that does not fit into roles here.**

**Dominic Quintero**: [IInteractable](https://github.com/abhiss/symmetrical-octo-waffle/blob/BranchJustForDocument/Assets/Scripts/Environment/IInteractable.cs) and [InteractableTerminal](https://github.com/abhiss/symmetrical-octo-waffle/blob/BranchJustForDocument/Assets/Scripts/Environment/TerminalInteractable.cs). We were unable to incorporate and expand upon this system in our game, but I created an intercatable system so when a user pressed their `UseKey`, they will activate events that are linked to the Terminal script.

**Yudi Lai**: [Item Spawner](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Environment/ItemSpawner.cs#L6) in `Team Fortress 2` style, a disk that spawn item on top, and spawn item only when the timer is reached and there are no item on top.

**Catherine Win**: Networking - Responsible for network setup like starting the game as a host and establishing connections between clients, host and server, manage the spawning of networked GameObjects like players, enemy spawner so that a player GameObject is automatically spawned for each client in the game.

# Main Roles #

Your goal is to relate the work of your role and sub-role in terms of the content of the course. Please look at the role sections below for specific instructions for each role.

Below is a template for you to highlight items of your work. These provide the evidence needed for your work to be evaluated. Try to have at least 4 such descriptions. They will be assessed on the quality of the underlying system and how they are linked to course content.

*Short Description* - Long description of your work item that includes how it is relevant to topics discussed in class. [link to evidence in your repository](https://github.com/dr-jam/ECS189L/edit/project-description/ProjectDocumentTemplate.md)

Here is an example:
*Procedural Terrain* - The background of the game consists of procedurally-generated terrain that is produced with Perlin noise. This terrain can be modified by the game at run-time via a call to its script methods. The intent is to allow the player to modify the terrain. This system is based on the component design pattern and the procedural content generation portions of the course. [The PCG terrain generation script](https://github.com/dr-jam/CameraControlExercise/blob/513b927e87fc686fe627bf7d4ff6ff841cf34e9f/Obscura/Assets/Scripts/TerrainGenerator.cs#L6).

You should replay any **bold text** with your relevant information. Liberally use the template when necessary and appropriate.

## Multiplayer & Networking - Abhi Sohal
**This roles main task is setting up the game to be multiplayer and ensuring that all the other subsystems work in the context of a multiplayer game. This task was related to the general theme of following design patterns and writing modular encapsulated code, which is very important when the code is running in different execution environments at the same time.**

Octo is a multiplayer game written in Unity using the brand new "Netcode for Gameobjects" library provided by Unity https://docs-multiplayer.unity3d.com/netcode/current/about/. This library provided me with common mechanisms for implementing multiplayer games like RPCs, networked events, replication, and session management. RPCs and networked events are mechanisms for explicit communication between client and server. I used this extensively for synchronizing audio and visual effects, enemy AI logic, spawning objects like grenades and enemies, and much more. Replication is a more implicit way of communication between server and clients where you can set properties to be synchronized for everyone and the library will handle the synchronization in the background. This was used for storing state information in NetworkVariables and synchronizing transforms and other properties for basically every dynamic object.

We knew that multiplayer in video games is a very complex topic and we didn't want that to be a burden on everyone on our team or to limit our pace of development. Because of this, I took on the role of being the "multiplayer guy" that made sure all the subsystems work in mutliplayer so the rest of the team didn't have to worry about it. I was also responsible for fixing bugs that came up that were related to multiplayer or networking code. The idea was that another team would design and implement a subsystem, then I could go in and network it to make sure it works in the multiplayer game.

### Synchronizing enemies
The core code for the enemies was written by the EnemyAI sub-team, but they didn't write it with multiplayer in mind, so I went through all the relevant scripts and modified them to work in a multiplayer game.

The first problem with running AI in a multiplayer game is that the code of AI can run on the server or on any of the clients. My approach to networking the enemies was to ensure that all enemies were "owned" by the server. This means that the code in the enemy script was executing on the server. This avoids running the same code in multiple execution environments which would be extremely confusing to reason about. It also avoids the problem of "how do we handle objects when the client that owns it disconnects?". Having all the enemies owned by the server simplified the implementation. By default, NetworkObjects are owned by the execution environment that spawned them, so if an enemy was spawned by a client, the enemy would be owned by that client.

Lucky, we had a EnemySpawner object that spawned enemies in waves. The key was to only run the EnemySpawner logic on the server. There's 2 ways to make a NetworkObject only run on the server. 1 way is to register it with the NetworkManager, which will ensure it's replicated and synchronized between clients and server. The other way is to give all the clients and the server their own version of the NetworkObject, but only run the specific logic on the server. In this case, I opted for the second option because the EnemySpawners were already being placed in the scene and because registering too many objects with NetworkManager is an anti-pattern. I did this by only running the spawning logic on the server while keeping a "dumb" version of the object on each of the clients. [Here](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Enemy/EnemySpawner.cs#L52) is the code for separating the client and server and [here](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Enemy/EnemySpawner.cs#L94) is the code for spawning enemies. Note that this isn't a normal Instantiate call, rather it calls the Spawn() method on a NetworkObject to spawn it across all the clients and on the server.

The actual enemies followed a similar pattern where the actual logic of the enemies, like where to go, who to attack, etc. was done on the server and we use the NetworkManager component to replicate their transforms and animators.

### Synchronizing Players
The clients own their own player objects, but every client also needs to run code for all the other players for visual and audio effects because those aren't synchronized by unity automatically like transforms and animators are. The problem was that when a client triggers an audio or visual effect, it only triggers it on it's own instance of the player, but the other clients won't see the audio/visual effect. The solution was to implement a "broadcast" system using RPCs. When 1 client triggers an effect on thier owned player, they tell all the other clients to trigger the same effect on their instance of that player. The problem is that clients can't call RPCs on other client, only on the server. So, I used the following pattern: the client triggering the effect calls a server RPC that calls a client RPC on all the client telling them to trigger the effect. [Here](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Abilities/JetPack.cs#LL148C18-L148C41) is an example of this used for the Jetpack's visual and audio effects. OnLaunch runs on 1 client, which calls OnJetpackStateServerRpc, which calls OnJpStateClientRpc, which triggers the jetpack effect. This solution had a latency issue for the client that triggered it because the effects lagged behind the input since it had to go through the server to trigger it's own particle. I simply solved this by separating the audio/visual effects into -Inner() methods that are called by the ServerRPC and immediately by the client that triggered the effect.

I used this same pattern for several game mechanics like
[jetpack](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Abilities/JetPack.cs#L147),
[shooting](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Character/CharacterShooting.cs#L154),
[dashing](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Abilities/Dash.cs#LL64C10-L64C10),
[turrets](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Hazards/Turret.cs#L58),
[explosive barrels](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Hazards/ExplosiveBarrel.cs#L21),
[grenades](https://github.com/abhiss/symmetrical-octo-waffle/blob/0f91e379656c097a249e7d305a00d911e83118fb/Assets/Scripts/Abilities/GrenadeExplosion.cs#L19) and more.

## EnemyAI - Yudi Lai & Edward John

**This Role's main task is designing and implementing the logical behavior of enemies. The following information descirbe the design approach and the key implementation each user did. This role's task is based on the Game AI protions of the course.**


Enemy basic logic: Our enemy patrol around until detects player, then it will attack or chase if not in attack range.


Yudi decide to use a `state machine` for the core structure behind the enemy behavior. Original implementation of the statemachine was abstracted and decoupled in several script and component. It uses a controller script on the main gameobject that switches states every update, and there are individual child component for each states. There were only 3 basic states: idle states where the enemy wondering to a random destination within a range, chasing states where enemy chase plaer, and attack state with unfinished attack logic. Each states will handle which is the next state script not just exceute the action but also determine the next transition (link found in Yudi's contribution below). This impementation was quickly abandoned in the early state before refactoring and was replace by Domonic's implementation of the statemachine that fits everything in one script - an implementation that handles the states as enum and each enum comes with a handlerfunction due to the concern for future networking difficulty(link see Domonic's contribution below). Edward the decouple the wonder/patrol behavior into its own states. Our final enemy uses 4 states: `idle`, `patrol`, `chase`, `attack`.(addition state might added due to network requirement, see network section).


We decide to use unity's navigator package that includes a navmesh for enemy movement path finding. This requires the map to have either `navmesh surface`or `navmesh obstacle` component, while the enemy will have a `navmesh agent` component. By giving a desitination in `Vector3` in the navmesh agent, the `GameObject` equip with the navmesh agent will path find its way to the desination. Our enemy chase and patrol logic utilize navmesh agent extensively. In patrol, we give navmesh agent a destination ramdomized within the enemy game object's sphereical radius; In chase, we constantly update the destination with the chasing target's position to create this chaising/homing effect on the target. However, sometimes we want to turn off the agent so that it's not moving. For example when the enemy is idling or attacking. When enemy attack we want the agent to stop (either disabled or have velocity of 0) because we want to avoid the problem where the enemy is stuck in attack animation but kept chasing the player which lead to this awkward sliding. Ideally the enemy movment should, We put in a lot of effort trying to solve this syncing issue with animator and main controller script, but the end result is still not perfect.


We also choose to use the Animation events to implement attack states after consulting with Professor Mccoy. to call on the function in controller script independent to the `Update()` in script that update. Without the animation event, attack damage was dealt immediately and have to work we a internal cool down system(see striker logic below). This has an issue with the enemy attack as soon as attack state execute completely out of sync with animation. With the animator event, the controller only need to trigger the animator for attack animation, then animation will exectue the `Attack()`. this no longer need a internal cool down, the animation time for one cycle is the internal cool down.


Our enemy will react towards damage taken. Our health system uses observer pattern so when the health script updates(damage taken), therefore we can add hit sound, spawn hit effect such as `electrical spark`.We use this to implement behavior on agro. Normally enemy only detect player when player walk within it's detection radius. However when the player deal enemy outside of the enemy detection range, the enemy will target the attacking player and start chasing (close the distance for attack state). This prevent player cheat their way out by assasinate enemy outside of their detection range.


Multiplyer logic: We make sure the enemy still behave accordingly when the current target/player dies. When the target is null. We also design a target system that will work with multiple player (see detail in individual enemies) The mentioned network behavior was tested with a half working network system with 2 player at the time. When Ahbi merge everyone's feature to work on the networking. There might be changes, see detail in networking role section by Ahbi.

**Final enemy that made into the game:**

Yudi's implementation on the AssultEnemy(range enemy) uses same statemachine structure from early unrefactored version of Edward's melee enemy. The patrol and idle is basically the same, where idle immediately goes to patrol and the enemy wonder around. The enemy shoots bullets at the player, when the player is air born the projectile travel 3 times the normal speed because air born player is harder to hit and it looks cooler. When player is out of sight, the Assult Enemy will enter the chase state to reposition until having a clear signline once again. The sightline can only be blocked by the wall and not other enemy, since the bullet doesn't collide with other enemy.The sightline is check using `SphereCast` while normal detection uses `OverlapSphere` so it keeps targeting player in air. To adjust for multiplayer the Assult/Range Enemy have the following behavior: The enemy immediately goes to idle then patrol once the player dies and start the detection cycle all over again. The Enemy will also target the closest player during attack and
chase stage.


**Yudi's Contribution:**
Development Timeline: statemachine(unused) -> stiker(new feature:attack and dealing damage to player)(unused) -> Titan(new feature: agro on player when hit and use animator event for attack)(unused) ->|player now have jackpack and knowckback system broke| ->  AssultEnemy( new feature: target on closest player, reposition when player go out of sight, target enemy in air, attack in range)

[Original State machine abstract class](https://github.com/abhiss/symmetrical-octo-waffle/blob/b8a8c090a95175cd3142bfb7757cf7ac053b8a63/Assets/Scripts/Enemy/State.cs#L6)

[Original State machine](https://github.com/abhiss/symmetrical-octo-waffle/blob/b8a8c090a95175cd3142bfb7757cf7ac053b8a63/Assets/Scripts/Enemy/EnemyBehaviour.cs#L7)

[Original States example](https://github.com/abhiss/symmetrical-octo-waffle/blob/b8a8c090a95175cd3142bfb7757cf7ac053b8a63/Assets/Scripts/Enemy/ChaseState.cs#L5)
The implementation was cut short after other the group choose to, it was really rough and unfinished

[Striker Enemy Attack logic](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/DominicAI/BaseEnemy.cs#L12)

[Titan Enemy logic](can't find the permalink, but basically Striker logic with animator event)

The implementation for striker and titan was cut short and unfinished after other people said we don't want like the model, but part of the logic and implementation is reused and help build on the final range enemy. Both had knock back mechanic applied to the player, but physic system was change after jackpack was added to the player so knockback no longer works.

[Assult Enemy logic](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/RangeEnemyController.cs#L11)

We end up making the decign choose to only have robot/mecha type enemy for our space themed game. This means that we won't have organic monster type like `Titan`. we also choose to remove `Striker` enemy that's to tiny for player to see and it's poorly animated.


We want the player to be able to react to the bullets so we made the enemy range attack in projectile instead of hitscan in raycast similar to player. This way the player will have fun dodging it. We also add the bullet trail so it's easier for player to see and dodge accordingly. When the bullet collide with most object, we made it destory itslef and spawn a spark effect. This add onto the hint that help player knows it's being hit if they see a spark effect on their character. We also made the bullet avoid collision between themselves and any other enemy on purpose.

**Contribution from Yudi Lai**
[EnemyBullet](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/EnemyBullet.cs#L7)

**Dominic's Conttibution:**
[State machine in one script with states in enum and handler function](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/DominicAI/BaseEnemy.cs#L12)
[Interface for the the statemachine](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/DominicAI/IEnemyMachine.cs#L3)

**Edward's Contribution:**
[Refactored and bugfixed AI](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/Enemy.cs)
[AI for Explo Droid](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/ExploDroid.cs)
[AI for Melee Droid](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/MeleeDroid.cs)

## Enemies: Edward John
I am responsible for the MeleeDroid, ExploDroid, and Enemy Spawner. Earlier in development, Yudi was responsible for AI while I worked on the enemy prefabs, models, animators, sfx, vfx, and interaction logic (taking damage, dealing damage, basically core game logic you'd see in a base enemy script). However, in the later stages of development, we ended up just working on our own enemies because we wanted to implement our own enemies. We ended up learning from each other and borrowing each others responsibilities here and there.

Enemy Logic:
---
My primary responsibility in enemy logic was to handle how the enemy interacted with the player. It called the right functions when taking damage or dying, handled target acquisitions through a hitbox made by OverlapCube, detected targets using OverlapSphere, and slowed down or sped up based on its state. Some of these sound like AI concepts, which is because they are; Yudi and I iteratively built on the AI, and many of the changes I made to AI were only able to be made due to his previous implementations of them. Anything that has to do with enemy movement can be attributed to him, and much of the targeting logic and state transition logic was based off his work.

An Enemy's life generally follows this process.
1. Spawn from EnemySpawner
2. Patrol around a patrol radius (OR immediately start chasing the enemy if the option is enabled)
3. Chase the player if they're in detection radius
4. Attack the player if they're in our hitbox created by overlapcube ()
    - If we're still in range, keep attacking
    - If we're not in range, start chasing
5. Die in an explosion now that the player shot us to death
OR
5. Kill the player and start patrolling again, looking for the next target

**Scripts**
- [Enemy.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/Enemy.cs)
- [MeleeDroid.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/MeleeDroid.cs)
- [ExploDroid.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/ExploDroid.cs)


Enemy SFX/VFX:
---
There was no dedicated sound manager for our enemies, but I still went ahead and implemented sounds. I went into Audacity for the majority of these sound effects to trim them, adjust their pitch, change their tempo, or boost their amplitude. You'll get to hear a sound when a droid dies, a droid gets hurt, when an explo droid detonates, when a melee droid hits, and when a melee droid misses. Our sound design was unpolished at the end so many of these sounds kinda get overshadowed by the player sounds, but when you hear it, it definitely gives some audio feedback for the player's actions.

I also implemented the death explosion upon droid death and hit sparks. The death explosion was just a modified version of the regular explosion prefab with a less showy particle system. Hit sparks were instantiated on enemies as a take damage event from a health system. A bit of particle system tweaking was required to make sure that hit sparks were visible (but not distracting), lasted for the right amount of time, and emitted for the right amount of time.


Enemy Models/Animations
---
We used droid assets from the unity asset store. I set up the prefabs and animators for titan (unimplemented), melee droid, and explodroid. This included setting up components and linking animator logic to our state machine logic (a LOT of debugging went into this to make it look good). We also ended up using animation events for our enemy attacks over coroutines after Yudi got the idea from an office hours session.

- [Assault Droids](https://assetstore.unity.com/packages/3d/characters/humanoids/sci-fi/battle-droids-pack-74088)

Enemy Spawner
---
The enemy spawner works by randomly spawning enemies based on a spawn chance in intervals. There are public methods available to remove and add enemy spawns during gameplay, but these were never used in practice. The enemy spawner normalizes spawn rates to make sure they're always logical values in proper ratios, so even a misinput on an enemy spawner spawn rate would fix itself.

- [EnemySpawner.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Enemy/EnemySpawner.cs)


## Game Design - Catherine Win & Jackie Trinh

### vFx Particles - Catherine Win

I have implemented a captivating game design that combines immersive particle effects and interactive mechanics to enhance the player experience. Using Blender, I have designed visually stunning particles such as muzzle flash, smoke, and various other effects that add a realistic touch to the gameplay.

First, I constructed a muzzle cone and planes to ensure that the effect could be viewed from all angles within the game. Next, I crafted a series of particle systems that simulate the smoke, sparks, and flashes typically seen when a weapon is fired. To fine-tune the visual elements, I utilized Unity's VFX graph, which allowed me to easily adjust parameters such as particle size, angle, and lifespan. With these adjustments, I successfully achieved the desired aesthetic effects for the muzzle.

[Muzzle](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Materials/Meshes/Muzzle.blend)

[Muzzle Flash vfx](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Prefabs/MuzzleFlash.vfx)
![muzzleflash](https://github.com/abhiss/symmetrical-octo-waffle/assets/129975299/f6d178dc-c45d-474c-9d9e-7e31d0af6da0)


Aside from muzzle effects, I also created various types of smoke particles that can be seen when a player dashes or jumps. Similarly, I constructed the design of the smoke using Blender and imported them into Unity where I utilized the VFX graph system. Then, I can adjust the parameters like spawn rate, angle alignment, lifespan and more to achieve the desired visual outcomes. Additionally, I have created the smoke trail and landing particle effect which have not been integrated into the game yet.

[Smoke](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Materials/Meshes/smoke.blend)  [JetPack vfx](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Prefabs/JetPack.vfx)    [DustCloud](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Prefabs/DustCloud.prefab)

[Smoke vfx](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Prefabs/Smoke.vfx)
![smoke](https://github.com/abhiss/symmetrical-octo-waffle/assets/129975299/67f32907-c582-43a8-8516-55eeb46e4295)

### Weapon Pickup and Switching System - Catherine Win & Jackie Trinh

Furthermore, we implemented a system for weapon pick up, drop, and inventory management to enhance the gameplay experience as well as shooting and reloading. Players can interact with weapons scattered throughout the game world, picking them up and adding them to their inventory. We designed a script that enables smooth weapon transitions, allowing players to seamlessly switch between different weapons. The script also manages the dropping of weapons when the player decides to discard them or replace them with a different weapon from their inventory.

When a player enters the trigger zone of the game object and remains inside, [CharacterWeaponPickup.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/Player/Assets/Scripts/Character/CharacterWeaponPickUp.cs) detects it through the OnTriggerStay function. It sets a boolean flag, isPlayerInside, to indicate that a player is present. Additionally, it retrieves the CharacterShooting component from the player, storing it in the characterShooting variable. If the player exits the trigger zone, the OnTriggerExit function is called. Here, the isPlayerInside flag is reset to 'false', and the characterShooting variable is cleared. During each frame update, the script applies two visual effects to the game object. First, it creates a hovering effect by slightly adjusting the object's vertical position using a sine wave calculation. This is done to make the pickup visually appealing. Second, it rotates the object around the Y-axis at a specified speed, causing it to spin. If a player is inside the trigger zone and presses the "E" key, it assumes that the player has a CharacterShooting component and calls its AddWeapon function. The AddWeapon function is responsible for switching the active weapon of the character. It allows the player to switch between different weapons during gameplay by calling the SetActiveWeapon function from [CharacterShooting.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/Player/Assets/Scripts/Character/CharacterShooting.cs). This function finds the game object representing the new weapon by searching for a child transform within the WeaponDrawer transform that matches the name of the new weapon. Once the new weapon game object is found, it is activated by setting its SetActive property to true. This makes the new weapon visible in the character's hand. After the weapon change, the script destroys the pickup object, removing it from the scene.

The player can also switch the weapons that are in the inventory by using the "X" key. The SwitchWeapon function keeps track of the current weapon in use within the game and cycles through the weapons in the inventory. For example, if the inventory contains three weapons and the current index is 2 (referring to the third weapon), the next index will be calculated as (2 + 1) % 3 = 0, representing the first weapon in the inventory. It also updates the necessary properties of the selected weapon, such as the clip size and remaining ammo, and then activates the new weapon for use.

In summary, it enables players to pick up a weapon by interacting with the game object. When a player is nearby and presses the designated key, their current weapon is swapped with the new weapon and always allows them to switch between different weapons. The object also features visual effects such as hovering and spinning to make it visually appealing.



## User Interface

**Describe your user interface and how it relates to gameplay. This can be done via the template.**

## Movement/Physics - Dominic Quintero
Our movement code is unique as we use the `CharacterController` component within Unity. We do not use the built-in physics system `(RigidBody)` provided by Unity; we are only provided collisions. This component essentially takes in a velocity and provides the following: collisions, climbing stairs, and a `isGrounded` variable, along with some of the standard unity collision functions such as `OnTriggerEnter()`. Nothing else is provided, which allows us to have a lot more freedom in implementing the player. The code in [CharacterMotor.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Character/CharacterMotor.cs) can be summarized as creating a velocity vector that reflects forces we create: gravity and user input.

### Input Vector
A common issue character controllers have is going down slopes. This is because you can't simply take the user's input directly as a velocity, it needs to be projected onto the floor of the player. The [ProcessInput()](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterMotor.cs#LL89C11-L89C11) function is where we correct this issue. To summarize, it created a plane with the ground surface normal, which allows us to properly project the input vector onto the surface.

### Force Vector
The force vector is how we simulate gravity when we have no gravity by default. The [Gravity()](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterMotor.cs#L192) function will process this vector and apply the current gravity settings to the player. When the player is grounded, the gravity must be set to '-0.5f' as a grounded value in order for the character controller component to properly detect if the player is grounded or not. An issue arises when we want to modify this vector; however, you cannot set the gravity to the grounded value on the same frame you want to do events such as jumping or knockback. To fix this, when someone wants to set the force velocity, we will skip the grounding statements so the character controller can process that the player is no longer grounded. The next frame will have an accurate IsGrounded value, and we can continue our calculations from there.

### Velocity (Finalization)
Velocity is the finalized vector once the frame has completed its calculations. The finalized vector is `InputVelocity + ForceVelocity`. We then send this vector to the [Move()](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterMotor.cs#L84) function provided by the component. Once the component receives this vector, it will perform the collision calculations for us, allowing the players to move as if they were a rigidbody. However now we have a great degree of control and can expand upon the movement code when needed.

### Jetpack & Sliding
These "abilities" display the pay off of using the Unity's `CharacterController` component with the addition of our [CharacterMotor.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Character/CharacterMotor.cs) component.

The [Jetpack.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/BranchJustForDocument/Assets/Scripts/Abilities/JetPack.cs) uses Kinematic equations (SUVAT), derived with help of [Sebastian Lague's Kinematic Equations Video](https://www.youtube.com/watch?v=IvT8hjy6q4o). With the help of the video, we were able to create the static [Trajectory.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Misc/Trajectory.cs) class. Each trajectory can return an arc, but we weren't able to fully display this system. When the player presses `SpaceBar`, the player will jump to the cursors `WorldPosition` and land on the target.

The sliding (intenerally named as [Dash.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Abilities/Dash.cs)) component displays the input projection working as planned. Without the input projection onto the plane, the player would slide off forward and off sloped surfaces rather than sliding down the slope as one would expect. The `CharacterMotor` has an option for scripts to [override the input](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterMotor.cs#LL10C43-L10C60), which is what the slide script uses.

## Animation and Visuals

**Assets Used:**
- Player:        [Robot Police Animation Set](https://assetstore.unity.com/packages/3d/animations/robot-police-animation-set-227367)
- Enemies:       [Battle Droids Pack](https://assetstore.unity.com/packages/3d/characters/humanoids/sci-fi/battle-droids-pack-74088)
- Weapons:       [Sci-Fi Weapons](https://devassets.com/assets/sci-fi-weapons/)
- Map:           [Big Star Station](https://www.unrealengine.com/marketplace/en-US/product/big-star-station)
- Particles:     [Particle Pack (Made by Unity)](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325#content)
- Main Menu:
    - [Power Supply](https://www.turbosquid.com/3d-models/free-sci-fi-power-supply-3d-model/1141036)
    - [Searchlight](https://www.turbosquid.com/3d-models/free-max-mode-beam-emitter/675838)
    - [Helmet](https://assetstore.unity.com/packages/3d/characters/humanoids/sci-fi/free-animated-space-man-61548)
- Interactables: [Sci-fi Terminal](https://sketchfab.com/3d-models/sci-fi-terminal-9a9343dd76d848318d09badba8606a63)

**Player - Dominic Quintero**
We wanted the player to be extremely reactive and reflect the players input. We use the animations within this asset to tie toghether the various states of the player.
We have an animation abstraction, [CharacterAnimator.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Character/CharacterAnimator.cs) that is a component listens to all the various character events that need animations. We try as much as possible to keep animation code and gameplay code seperate, as we never planned on the player code being heavily reliant on the animator states.

## Input - Dominic Quintero

### Input Configuration
The game can only be played with a keyboard and mouse. The current systems in place are incredibly `MousePosition` to `WorldPosition` heavy. We use this to our advantage by allowing the player to aim vertically up and down.

We had plans for input reconfiguration via a settings menu via [InputListener.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/BranchJustForDocument/Assets/Scripts/Character/InputListener.cs), but we were not able to implement this feature in time.

### Controls:
- W - Forward
- A - Left
- S - Backward
- D - Right
- R - Reload
- G - Grenade
- Spacebar - Jetpack
- Shift - Slide
- Left Click - Fire
- X - Weapon Switching
- E - Pick Up Weapon

## Game Logic

**Document what game states and game data you managed and what design patterns you used to complete your task.**

# Sub-Roles

## Camera / Aiming / Shooting & Reloading - Dominic Quintero 

### Camera
We didn't want the camera to be at a fixed location and follow the player. We wanted to immerse our players more by having the camera move where they aim, but this provided a challenge as one-to-one camera position + camera offset to the mouse's world position did not yield the desired results, and we wanted to keep the player within the camera at all times. Our approach, [CharacterCamera.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/main/Assets/Scripts/Camera/CharacterCamera.cs) allows for a radius of "play" within the player's radius. We calculate the normalized direction between the `PlayerPosition` and the `MouseWorldPosition` We get the current distance between these points and multiply the directional vector by this clamped distance, giving us our radius of play and our camera's point position. We can then customize this by setting the cameras local position and rotation within the player prefab with ease, the local position will be used as the cameras offset, giving us `CameraPosition = CameraPoint + CameraOffset` With this implementation, if we set the plane's position to the player's position, we can easily have the camera raise and fall with the player. Effectively keeping the player within the camera at all times, giving the player a region of "play", and adding that extra layer of immersion.

### Aiming
Since the game is 3D, we wanted to take advantage of the verticality 2D games normally don't have the luxury of. Depending on the cursors world position the player can aim up and down vertically. To visualize the vertical aiming we have two visual aids, the players circular half cursor and the lasersight. Within [CharacterShooting.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterShooting.cs#L212) We have a height tollerance that enables and disables vertiical aiming. This is so when the player is aiming on a flat surface, the game plays as you would expect any other 2D shooter game to play. Once this we exceed this flat threshold, vertical aiming takes over.

The circular cursor serves as a subtle hint of when the player is vertically aiming or not. When they're not vertically aiming, the laser guide will go through the center of the cursor, signifying they currently can shoot directly infront of them. When vertical aiming is active, the laser guide will go straight to the center of the circular cursor. The cursor image was hand drawn.

Vertical aiming brings about many edge cases that need to be solved inorder for clean and satisfying control of the player. We have a subtle aim assist that recalculates the aim direction to target what the cursor is pointing at. We also have to worry about the player being obstructed by walls because of the 3D verticallity, and so we fade colliders that are set to the Layer `Wall`. It attaches a component [Obstructable.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/BranchJustForDocument/Assets/Scripts/Environment/Obstructable.cs) which fades the walls and changes the walls layer to a layer the camera ray will ignore, allowing for the player to play like normal and bring a "Diablo" feel to the game. The shader graph for the fade logic can be found within [Assets/Materials/Shaders/Fade.shadergraph](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Materials/Shaders/Fade.shadergraph).

### Shooting & Reloading
The system in place allows for quick and easy swapping between weapons and their individual ammo values, intervals, and SFX. Functionally, [CharacterShooting.cs](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterShooting.cs) functionally, it works as you would expect it to work in any other shooter game. You can't reload or shoot while reloading; shooting consumes ammo, and when reloading, you subtract from your available ammo pool and add it to your gun. To do this, the [Reload()](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterShooting.cs#L158) function will calculate how many bullets the player currently has, and it will add the bullets within the player's clip upon reload + the ammo pool, preventing the loss of ammo during reloading. After reloading is finished, the `ScriptableObject` will have its ammo values updated. It is designed this way so players can drop guns to each other that have their own individual ammo counts and bullets within the clip. An example scenario: Giving a friend a weapon you reloaded and slightly used and then giving it to them. They will have the left-over bullets within the clip.

## CameraShake / Weapon Laser / Grenade System - Jackie Trinh

### CameraShake
The [CameraShake](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Camera/CharacterCamera.cs#L73) method is responsible for initiating the camera shake. It takes two parameters: `intensity`, which determines the magnitude of the shake, and `duration`, which determines how long the shake should last. When this method is called, it sets the `_shakeIntensity` to the provided intensity and computes the `_shakeDecay` as the ratio of intensity to duration. This decay rate ensures that the shake will gradually decrease until it stops completely at the end of the specified duration.

The shaking effect is processed in the `rocessShake(ref Vector3 cameraPosition)` method. If the _shakeIntensity is greater than zero, the camera's position is altered by adding a random vector. This vector is calculated by `Random.insideUnitSphere * _shakeIntensity`, which generates a random point within a sphere of radius equal to `_shakeIntensity`. This modification of the camera's position gives the shaking effect.

The `_shakeIntensity` is then reduced in each frame by subtracting `_shakeDecay * Time.deltaTime`. This creates a gradual decrease in shaking intensity over time, creating a smooth transition from a heavy shake to no shake. This process continues until _shakeIntensity reaches zero, at which point the shaking effect stops completely.

The `StopShake(float duration)` method is a coroutine that waits for the specified duration and then sets `_shakeIntensity` to zero. This method ensures that the shaking effect doesn't last longer than the desired duration.

In summary, the camera shake mechanic is an effective way to convey certain in-game events, such as an `explosion`, to the player by shaking the camera view. It's controlled by specifying the intensity and duration of the shake, creating a powerful visual effect. The shaking effect gradually diminishes over time due to the decay process, which smoothly reduces the shake intensity until it stops, ensuring a smooth transition from the shaking effect back to the regular camera view.
### Weapon Laser
To implement the [Weapon Laser](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Character/CharacterShooting.cs#L286) functionality, I turned to Unity's native `LineRenderer` component. This handy tool can draw lines in 3D space, following a sequence of points. In the Start method, I created a new LineRenderer instance and added it to the game object. Then, I set its properties. I gave it a distinct look using a predefined material, the LaserMaterial, to style the laser. I also ensured the laser maintained a consistent width from start to end by setting its startWidth and endWidth properties.

Initially, I had planned to make the laser visible only when the player right-clicks the mouse, but for the sake of a more intuitive game feel, I decided to have it continuously visible whenever the player is aiming. Throughout the game, I continuously monitor the player's actions in the update loop, updating the laser's state accordingly. If the player is aiming, and is not in the process of reloading or have their input disabled, the laser gets drawn. In the DrawLaser method, I calculate the end position of the laser line. This position is based on where the player's character is aiming, multiplied by the maximum distance of the weapon. However, if the laser comes into contact with an object before reaching its maximum distance, I adjust the laser to stop at the hit point, keeping it realistic. By updating these positions each frame, I've managed to simulate a real-time laser sight for the player's character. I believe this has significantly enhanced the shooting experience for players.
### Grenade System

This system includes various components: the grenade-throwing mechanism, the grenade pickup objects, the grenade object itself, and the subsequent explosion caused by a grenade.

[GrenadeThrower](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Abilities/GrenadeThrower.cs#L5): This script is primarily responsible for the grenade-throwing action. First, it ensures that the script is only active for the owner of the network object to avoid any undesired actions from non-owners. It then listens for the specific input (via an `InputListener` component) from the player to initiate a grenade throw. Importantly, the system incorporates a cooldown mechanism to prevent immediate subsequent throws, which is managed using Unity's Time.time and a `GrenadeThrowCooldown` variable. The grenade count is tracked as well, and a throw can only be initiated if the player possesses at least one grenade. For the throwing action, the script calculates a trajectory towards the player's current cursor position on the screen, offering the player a sense of control over the grenade's direction. It also connects with the game's animation system, setting a "IsThrowingGrenade" boolean to true to trigger the throwing animation.

[GrenadePickUp](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Abilities/GrenadePickup.cs#L4): The GrenadePickup script manages the actions surrounding the player's interaction with grenade pickups found in the game world. Each pickup object is associated with a specific `GrenadeStock` count, which represents the number of grenades the player receives upon pickup. This script also incorporates a rotation animation to make the grenade pickup object visually dynamic, creating an appealing user experience. If a player's collider enters the pickup object's collider, the system checks whether the player's grenade count has reached the maximum limit. If not then the player's grenade count is incremented by the grenade `stock value`, and a pickup audio is played. To ensure each pickup is only usable once, a `_oneShot` boolean is used to track if the pickup has been collected.

Grenade: Attached to the grenade object, this script handles the `lifecycle` of a thrown grenade. It initiates a coroutine upon the grenade's creation that waits for a specific duration `_detonationTime` before triggering the detonation. The detonation process spawns an explosion prefab at the grenade's current location, creating a visual and auditory indication of the explosion, and then destroys the grenade object itself.

[Explosion](https://github.com/abhiss/symmetrical-octo-waffle/blob/cfe0206d7430113f01e44b3a2926e4c01a8503fa/Assets/Scripts/Hazards/Explosion.cs#L7): The explosion script comes into play when a grenade detonates. It plays an explosion sound effect via an AudioSource component for auditory feedback and instigates a camera shake for visual feedback, providing a more immersive experience for the player. The explosion also includes a mechanism for dealing damage to targets within a specific radius. By using the Physics.OverlapSphere function, the script gathers all colliders within the explosion's radius and checks if they are valid targets based on their tags `"Player", "Enemy", or "Destructible"`. If valid, the `HealthSystem` component of the target is accessed, and damage is applied. After a specified duration, the explosion object is destroyed, clearing it from the scene. After the explosion happens and if it hits a valid target, this script shakes the main character's camera. This is where it calls the ShakeCamera method from the CharacterCamera class. It's a neat little touch that adds a lot to the game's feel, making the explosion more intense and real. So, when an explosion goes off near the player, the camera shakes, giving the player a sense of impact from the explosion.

## Hazards - Edward John
I implemented Explosive Barrel, Floor Trap, and Turret (along with its energy projectile). Many of the particles came from the asset store and just needed some minor tweaking, but sounds required a bit of online shopping and some audio editing. SFX that fit the object they're associated with it are crucial for game feel. A floor trap in space will play a laser-like sound effect, and a turret will plan a beeping detection noise while a player is in its radius then shoot an energy projectile that explodes on collision or after a travel time.
- Explosive Barrel: Spawns an explosion after dying.
- Floor Trap: Deals damage to players while they stay on it.
- Turret: Shoots projectiles at players in its detection radius with a cooldown. Accounts for scenarios with multiple players, focusing on a target even if another target might enter its radius.

**Scripts**
- [Floor Trap](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Hazards/FloorTrap.cs)
- [Turret](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Hazards/Turret.cs)
- [Energy Projectile](https://github.com/abhiss/symmetrical-octo-waffle/blob/feature/enemy/Assets/Scripts/Hazards/EnergyProjectile.cs)

**Model Sources**
- [Floor Trap](https://assetstore.unity.com/packages/3d/props/interior/dungeon-floor-traps-77765)
- [Turret](https://assetstore.unity.com/packages/3d/props/guns/laser-turret-36177)
- [Explosion](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325)
 
## Cross-Platform

**Describe the platforms you targeted for your game release. For each, describe the process and unique actions taken for each platform. What obstacles did you overcome? What was easier than expected?**

## Audio

**List your assets including their sources and licenses.**
- [Main Menu Theme](https://freesound.org/people/rhodesmas/sounds/321723/)
- [Menu Click](https://freesound.org/people/nfrae/sounds/625808/)
- [JetPack Loop](https://freesound.org/people/Mozfoo/sounds/458377/)
- [JetPack Launch](https://freesound.org/people/Jarusca/sounds/521377/)
- [Sci-Fi Reload](https://freesound.org/people/SonoFxAudio/sounds/679546/)
- [Bullet Shot](https://pixabay.com/sound-effects/semiautorifleshotwav-14725/)
- [Ammo Pickup](https://freesound.org/people/Dpoggioli/sounds/213607/)

**Enemy SFX Sources**
- [Droid Death](https://freesound.org/people/bareform/sounds/218721/)
- [Droid Hurt](https://freesound.org/people/Hanbaal/sounds/178659/)
- [MeleeDroid Hit](https://freesound.org/people/ethanchase7744/sounds/441666/)
- [MeleeDroid Miss](https://freesound.org/people/EminYILDIRIM/sounds/541210/)
- [ExploDroid Detonate](https://freesound.org/people/snakebarney/sounds/138108/)

**Hazard SFX Sources**
- [Floor Trap](https://freesound.org/people/The-Sacha-Rush/sounds/657817/ )
- [Turret](https://freesound.org/people/NicholasJudy567/sounds/673841/)
- [Electric Projectile](https://freesound.org/people/FeliUsers/sounds/682068/)
- [Explosion](https://freesound.org/people/SuperSouper/sounds/684754/)

**Describe the implementation of your audio system.**

**Document the sound style.**

## Gameplay Testing

**Add a link to the full results of your gameplay tests.**

**Summarize the key findings from your gameplay tests.**

## Narrative Design - Catherine Win

“Octo” is an action-packed sci-fi top-down multiplayer shooter set in a vast universe filled with abandoned and overrun spaceships. As a team of skilled mercenaries, players take on perilous missions to cleanse these derelict vessels of threats and secure valuable bounties. With the option to venture solo or team up with friends, players will navigate treacherous environments, level up their characters, and acquire powerful loot while facing escalating challenges and formidable boss encounters.

Storyline: In the distant future, humanity has colonized various star systems, establishing trade routes and expanding its reach across the cosmos. However, this expansion has left behind a trail of abandoned and decaying spaceships that have become breeding grounds for deadly creatures and rogue artificial intelligences. To maintain order and ensure the safety of future explorers, an elite group of mercenaries emerges.

Gameplay Mechanics:
1. Team Coordination: Players can assemble a team up to tackle missions together, strategizing and coordinating their efforts to maximize efficiency and survival chances.

2. Risk vs. Reward: Mercenaries can choose to venture alone, embracing high-risk scenarios for potentially greater rewards. Alternatively, teaming up with friends provides additional firepower and support but comes with a more balanced risk profile.

3. Character Progression: As players successfully complete missions, they earn experience points (XP) and gold, which can be used to level up their characters. Leveling up unlocks new abilities, enhances existing skills, and improves stats, making mercenaries more formidable in combat.


5. Procedurally Generated Ships: Each spaceship presents a dynamically generated layout, ensuring a unique experience with every playthrough. As players reach deeper into the ship's levels, the difficulty ramps up, demanding greater skill and coordination from the team.

6. Boss Battles and Loot: At the heart of each vessel lies a formidable boss encounter. Successfully defeating these challenging adversaries grants players access to unique, customized loot, matching the boss's thematic design and providing powerful enhancements or rare items.

Octo offers an engaging narrative-driven multiplayer experience set in a vast sci-fi universe. By blending cooperative gameplay, character progression, and high-stakes decision-making, the game offers challenging and rewarding gameplay loops. Explore abandoned spaceships, face off against menacing bosses, and acquire powerful loot.

## Press Kit and Trailer

**Include links to your presskit materials and trailer.**

**Describe how you showcased your work. How did you choose what to show in the trailer? Why did you choose your screenshots?**

## Game Balacing - Yudi Lai

**This subrole focus on making the game feel more balanced to play**
Basic balancing:

The melee enemy should have higher health than range enenmy, while the explosion enemy should have least amount of health. The player will have 3 different weapon: assult rifle, shotgun, sniper rifle. In that order. Sniper will have the most damage, slowest fire rate, and smallest magazine size; the assult rigle have the least damage, fastest fire rate, and largest magazine size. When picking up ammo, we add 2 magazine worth of ammo for each gun instead of picking up fixed ammount of ammo for the obvious reason. 20 bullet in sniper rille kills worth 4 magazine and can kill 20 range enemy while 20 bullets in assult rifle only kill 2 assult enemy(if landed all) and less than half of magazine.

since the game's goal is finding an exit. We want the player to be able to speed run  without completely ignore the enemy. So we made player harder to get hit by the enemy and allow them to move pass the enemy. However we also want to punish player for not killing the enemy and prevent they just keep running, we made the enmy not losing agro, and once detected they will forever chase the player. So if the player choose not to kill the enemy, they will pile up and constantly apply pressure to the player, one wrong turn might cost them life. We also made the slding no invinsible and the player can't phase through enemy so the player can't easily move around the enemy once their is a horde.

To make the play feel more challenging:

We desgin our game to have no health packs so the player only have one life.

Since the player have jackpack and can be air born, I made the enemy shoot 3 times faster when attacking the air born player, this pressure the player to move wisely when in air as well.

To make the game play feel more fair:

I make sure the enemy cannot attack player while chasing, the enemy have to stop when attacking, this makes the enemy such as explosion enemy and melee enemy more balanced. Especially when we design the game such that player only have limited health. We have to make sure the enemy doesn't hit player too easily.


## Main Menu

**Dominic's Conttibution:**
Visual design and UI layout.

## Game Feel

**Document what you added to and how you tweaked your game to improve its game feel.**
