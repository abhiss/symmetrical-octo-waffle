# Game Basic Information #

## Summary ##

**A paragraph-length pitch for your game.**

## Gameplay Explanation ##

**In this section, explain how the game should be played. Treat this as a manual within a game. It is encouraged to explain the button mappings and the most optimal gameplay strategy.**


**If you did work that should be factored in to your grade that does not fit easily into the proscribed roles, add it here! Please include links to resources and descriptions of game-related material that does not fit into roles here.**
**Yudi Lai**: [Item Spawner](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Environment/ItemSpawner.cs#L6) in `Team Fortress 2` style, a disk that spawn item on top, and spawn item only when the timer is reached and there are no item on top.

# Main Roles #

Your goal is to relate the work of your role and sub-role in terms of the content of the course. Please look at the role sections below for specific instructions for each role.

Below is a template for you to highlight items of your work. These provide the evidence needed for your work to be evaluated. Try to have at least 4 such descriptions. They will be assessed on the quality of the underlying system and how they are linked to course content. 

*Short Description* - Long description of your work item that includes how it is relevant to topics discussed in class. [link to evidence in your repository](https://github.com/dr-jam/ECS189L/edit/project-description/ProjectDocumentTemplate.md)

Here is an example:  
*Procedural Terrain* - The background of the game consists of procedurally-generated terrain that is produced with Perlin noise. This terrain can be modified by the game at run-time via a call to its script methods. The intent is to allow the player to modify the terrain. This system is based on the component design pattern and the procedural content generation portions of the course. [The PCG terrain generation script](https://github.com/dr-jam/CameraControlExercise/blob/513b927e87fc686fe627bf7d4ff6ff841cf34e9f/Obscura/Assets/Scripts/TerrainGenerator.cs#L6).

You should replay any **bold text** with your relevant information. Liberally use the template when necessary and appropriate.

## EnemyAI - Yudi Lai & Edward John

**This Role's main task is designing and implementing the logical behavior of enemies. The following information descirbe the design approach and the key implementation each user did. This role's task is based on the Game AI protions of the course.**


Enemy basic logic: Our enemy patrol around until detects player, then it will attack or chase if not in attack range.


Yudi decide to use a `state machine` for the core structure behind the enemy behavior. Original implementation of the statemachine was abstracted and decoupled in several script and component. It uses a controller script on the main gameobject that switches states every update, and there are individual child component for each states. There were only 3 basic states: idle states where the enemy wondering to a random destination within a range, chasing states where enemy chase plaer, and attack state with unfinished attack logic. Each states will handle which is the next state script not just exceute the action but also determine the next transition (link found in Yudi's contribution below). This impementation was quickly abandoned in the early state before refactoring and was replace by Domonic's implementation of the statemachine that fits everything in one script - an implementation that handles the states as enum and each enum comes with a handlerfunction due to the concern for future networking difficulty(link see Domonic's contribution below). Edward the decouple the wonder/patrol behavior into its own states. Our final enemy uses 4 states: `idle`, `patrol`, `chase`, `attack`.(addition state might added due to network requirement, see network section).


We decide to use unity's navigator package that includes a navmesh for enemy movement path finding. This requires the map to have either `navmesh surface`or `navmesh obstacle` component, while the enemy will have a `navmesh agent` component. By giving a desitination in `Vector3` in the navmesh agent, the `GameObject` equip with the navmesh agent will path find its way to the desination. Our enemy chase and patrol logic utilize navmesh agent extensively. In patrol, we give navmesh agent a destination ramdomized within the enemy game object's sphereical radius; In chase, we constantly update the destination with the chasing target's position to create this chaising/homing effect on the target. However, sometimes we want to turn off the agent so that it's not moving. For example when the enemy is idling or attacking. When enemy attack we want the agent to stop (either disabled or have velocity of 0) because we want to avoid the problem where the enemy is stuck in attack animation but kept chasing the player which lead to this awkward sliding. Ideally the enemy movment should, We put in a lot of effort trying to solve this syncing issue with animator and main controller script, but the end result is still not perfect.


We also choose to use the Animation events to implement attack states after consulting with Professor Mccoy. to call on the function in controller script independent to the `Update()` in script that update. Without the animation event, attack damage was dealt immediately and have to work we a internal cool down system(see striker logic below). This has an issue with the enemy attack as soon as attack state execute completely out of sync with animation. With the animator event, the controller only need to trigger the animator for attack animation, then animation will exectue the `Attack()`. this no longer need a internal cool down, the animation time for one cycle is the internal cool down.


Our enemy will react towards damage taken. Our health system uses observer pattern so when the health script updates(damage taken), therefore we can add hit sound, spawn hit effect such as `electrical spark`.We use this to implement behavior on agro. Normally enemy only detect player when player walk within it's detection radius. However when the player deal enemy outside of the enemy detection range, the enemy will target the attacking player and start chasing (close the distance for attack state). This prevent player cheat their way out by assasinate enemy outside of their detection range.


Multiplyer logic: We make sure the enemy still behave accordingly when the current target/player dies. When the target is null. We also design a target system that will work with multiple player (see detail in individual enemies) The mentioned network behavior was tested with a half working network system with 2 player at the time. When Ahbi merge everyone's feature to work on the networking. There might be changes, see detail in networking role section by Ahbi.

**Final enemy that made into the game:**

--Edward add melee and explosion  enemy logic here------


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
**Domonic's Conttibution:**
[State machine in one script with states in enum and handler function](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/DominicAI/BaseEnemy.cs#L12)
[Interface for the the statemachine](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/DominicAI/IEnemyMachine.cs#L3)
**Edward's Contribution:**

## Enemy Design - Yudi Lai & Edward John


**This Role's main task is designing and implementing the logical behavior of enemies. This role's work is related to the topic related to game feel topic of the class.**


We end up making the decign choose to only have robot/mecha type enemy for our space themed game. This means that we won't have organic monster type like `Titan`. we also choose to remove `Striker` enemy that's to tiny for player to see and it's poorly animated. 


We want the player to be able to react to the bullets so we made the enemy range attack in projectile instead of hitscan in raycast similar to player. This way the player will have fun dodging it. We also add the bullet trail so it's easier for player to see and dodge accordingly. When the bullet collide with most object, we made it destory itslef and spawn a spark effect. This add onto the hint that help player knows it's being hit if they see a spark effect on their character. We also made the bullet avoid collision between themselves and any other enemy on purpose.

**Contribution from Yudi Lai**
[EnemyBullet](https://github.com/abhiss/symmetrical-octo-waffle/blob/1b37cf62ea5483326a6de48fcd46aedc5267e652/Assets/Scripts/Enemy/EnemyBullet.cs#L7)

## User Interface

**Describe your user interface and how it relates to gameplay. This can be done via the template.**

## Movement/Physics

**Describe the basics of movement and physics in your game. Is it the standard physics model? What did you change or modify? Did you make your movement scripts that do not use the physics system?**

## Animation and Visuals

**List your assets including their sources and licenses.**

**Describe how your work intersects with game feel, graphic design, and world-building. Include your visual style guide if one exists.**

## Input

**Describe the default input configuration.**

**Add an entry for each platform or input style your project supports.**

## Game Logic

**Document what game states and game data you managed and what design patterns you used to complete your task.**

# Sub-Roles

## Cross-Platform

**Describe the platforms you targeted for your game release. For each, describe the process and unique actions taken for each platform. What obstacles did you overcome? What was easier than expected?**

## Audio

**List your assets including their sources and licenses.**

**Describe the implementation of your audio system.**

**Document the sound style.** 

## Gameplay Testing

**Add a link to the full results of your gameplay tests.**

**Summarize the key findings from your gameplay tests.**

## Narrative Design

**Document how the narrative is present in the game via assets, gameplay systems, and gameplay.** 

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




## Game Feel

**Document what you added to and how you tweaked your game to improve its game feel.**
