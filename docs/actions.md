# Tile Actions
This document goes over every tile action available in DMT.

Each one will have a short description and an overview of the accepted values.

---

## Add Layer
Add a layer to the current map.

### Key
"DMT/addLayer"

### Accepted Value
The id of the layer as a string, must be unique

### Example
"DMT/addLayer_Enter": "DMT/TestLayer"

---

## Add Tilesheet
Add a tilesheet to the current map.

### Key
"DMT/addTilesheet"

### Accepted Value
A comma (',') delimited string of the id of the tilesheet (must be unique), and the path to the image tilesheet image

### Example
"DMT/addTilesheet_Enter": "DMT/TestTilesheet,Assets/Maps/Tilesheets/paths.png"

---

## Change Index
Change the index of the tile currently under the player.

### Key
"DMT/changeIndex"

### Accepted Values
* An empty string, this will remove the tile from the layer.
* A number representing a different index in the same tilesheet as the current tile.
* A combination of an existing tilesheet id (string) and an index in said tilesheet (number) split by a forward slash ('/').
* A collection of option 2 or 3 to create an animated tile split by a space (' ') followed by comma (',') and the interval between frames in milliseconds

### Examples
* "DMT/changeIndex_On": ""  
* "DMT/changeIndex_On": "137"  
* "DMT/changeIndex_On": "indoors/57"  
* "DMT/changeIndex_On": "indoors/1741 indoors/1773 indoors/1805 indoors/1773 indoors/1741,250"

---

## Change Multiple Indexes
Change multiple tile indexes on the current map.

### Key
"DMT/changeMultipleIndex"

### Accepted Values
Every accepted value for this works the same as [Change Index](#change-index), however they should be prefixed with the layer and tile coordinates.

### Examples
This example mixes some available values from [Change Index](#change-index), you can mix and match the variants from there as you need them here, as long as they are prefixed by '\{layer id\} \{x position of tile\} \{y position of tile\}=...', each one must bu split by a bar ('|').

"DMT/changeMultipleIndex_Enter": "Buildings 12 27=indoors/472|Buildings 12 28=indoors/473|Buildings 12 29=indoors/474|Buildings 13 27=indoors/504|Buildings 13 28=indoors/505|Buildings 13 29=indoors/506|Buildings 14 27=indoors/536|Buildings 14 28=indoors/537|Buildings 14 29=indoors/538"

---

## Change Properties
Change the properties of a tile.

### Key
"DMT/changeProperties"

### Accepted Value
A bar ('|') delimited string of key=value pairs.

**Note** if the "=value" is ommited, the property will be removed instead.

### Example
"DMT/changeProperties_On": "TouchAction|Action=kitchen"

---

## Change Multiple Properties
Change the properties of multiple tiles.

### Key
"DMT/changeMultipleProperties"

### Accepted Values
A bar ('|') delimited string of key=value pairs.  
Unlike with [Change Properties](#change-properties), here the key is split up into multiple parts separated by comma's (',').  
These parts are formatted like:
* (optional) The tile layer id to select the tile from
* The tile x position
* The tile y position
* The actual tile property

**Note** if the "=value" is ommited, the property will be removed instead.

### Example
"DMT/changeMultipleProperties_Once_On": "Buildings,17,8,Action=MindMeltMax.YouWishThisExisted/CouponMachine|Buildings,17,8,Passable|17,8,NoFurniture=T"  
"DMT/changeMultipleProperties_Enter": "NoFurniture=T|TouchAction=MindMeltMax.WeirdEvents/BossFight7"

---

## Explosion
Triggers an explosion.

### Key
"DMT/explosion"

### Accepted value
A space (' ') delimited string with multiple options to use when triggering the explosion. These options are as follows in order:
* The tile x position (must set y too)
* The tile y position (must set x too)
* The radius of the explosion
* Whether to damage the farmer
* A custom amount of damage
* Whether to destroy objects
* The sound of the explosion

**Note** all values are technically optional, and any one can be ommited by just leaving the value blank (see examples below).

### Example
"DMT/explosion_MonsterSlain(Mummy)": "7 15 3   thunder"  
"DMT/explosion_On": "  5 false 120 true"

---

## Push (Experimental)
Allow a tile to be pushed around.

### Keys
"DMT/push"
"DMT/pushable"

### Accepted Value
A string of tile positions in the form of '{x} {y}' representing the allowed destination tiles, separated by comma's (',').

**Note** this property is only applicable to the "Buildings" layer.

### Example
"DMT/push_On": "7 15, 7 17"  
"DMT/pushable_Explode": "3 3, 3 5, 2 4, 4 4"

---

## Push Also (Experimental)
Allow a tile to be pushed when a neighbor tile is pushed.

### Key
"DMT/pushAlso"

### Accepted Value
A string of tile offsets with the layer id to push the tile from in the form of '{layer Id} {x offset} {y offset}' representing the tile offset on the specified layer, relative to the original pushed tile. Each part must be separated by comma's (',').

**Note** this property can push tiles on layers other than "Buildings".  
**Note** also, this property does not accept triggers and can only be actived when a neighboring tile is pushed.

### Example
"DMT/pushAlso": "Front 0 -1, Front 0 -2, Buildings 0 -3"

---

## Push Others (Experimental)
Specify other tiles to be pushed.

### Key
"DMT/pushOthers"

### Accepted Value
A string of tile positions with the layer id to push the tile from in the form of '{layer Id} {x} {y}' representing the tile position on the specified layer. Each part must be separated by comma's (',').

**Note** this property can push tiles on layers other than "Buildings".
**Note** also, tiles pushed by this property will activate tiles with the [Push Also](#push-also-experimental) property.

### Example
"DMT/pushOthers_On": "Buildings 5 15, Front 4 16, Front 5 16, Front 6 16"

---

## Play Sound
Play one or more sounds.

### Key
"DMT/sound"

### Accepted Value
A bar ('|') delimited string of sound names with optional delays (split by commas (',')).

**Note** that when playing more than one sound, each sound after the first is delayed by (300 * index of sound) milliseconds.

### Example
"DMT/sound_Item((O)335)": "hammer"  
"DMT/sound_Item((O)335)": "hammer,150"  
"DMT/sound_Item((O)337-5)": "pickupItem|money|money,450"

---

## Teleport
Teleport the player to a specified pixel coordinate.

### Key
"DMT/teleport"

### Accepted Value
A space (' ') delimited string with the X and Y pixel coordinates.

### Example
"DMT/teleport_On": "516 1272"

---

## Teleport Tile
Teleport the player to a specified tile coordinate.

### Key
"DMT/teleportTile"

### Accepted Value
A space (' ') delimited string with the X and Y tile coordinates.

### Example
"DMT/teleportTile_Off": "6 13"

---

## Give Item
Give the player a specified item (or money)

### Key
"DMT/give"

### Accepted Values
* A string beginning with "Money/" followed by the amount of money to be added.
* A single qualified item id.
* A comma (',') delimited string with: the qualified item id, the stack size, (optional) the quality; In that order.

### Example
"DMT/give_Once_MonsterSlain": "Money/2500"  
"DMT/give_Talk(Abigail)": "(O)66"  
"DMT/give_Talk(Willy)": "(O)131,1,4"

---

## Take Item
Take a specified item (or money) from the player.

### Key
"DMT/take"

### Accepted Values
* A string beginning with "Money/" followed by the amount of money to be added.
* A single qualified item id.
* A comma (',') delimited string with: the qualified item id, the stack size, (optional) the quality; In that order.

### Example
"DMT/take_Once_Enter": "Money/500"  
"DMT/take_Talk(Piere)": "(O)MindMeltMax.YouWishThisExisted/DiscountCoupon"  
"DMT/take_Talk(Abigail)": "(O)66,5"

---

## Spawn Chest
Spawn a chest with item's (and/or money).

### Key
"DMT/chest"

### Accepted Value
A string with multiple parts, defined as follows:
* The tile position as a space (' ') delimited string, followed by an equals sign ('=').
* (optional) The non-qualified item id of the chest followed by a bar ('|').
* A space (' ') delimited string of either items or money as defined in the formats for [give](#give-item) and [take](#take-item).

**Note** that since the 1.6 update, a chest can not directly contain money. The workaround for this is that DMT will add the new Gold coin object with the stack size being the amount of money, each gold coin is worth 250 money.
**Note** also that the game currently does not handle stacked gold coins, this will be fixed as of Stardew Valley 1.6.9, there is currently no work around for this implemented.

### Example
"DMT/chest_Once_MonsterSlain": "7 12=BigChest|Money/10 (O)336,5 (O)384,17 (O)125"  
"DMT/chest_Once_On": "15 15=(O)349,3 (O)773,3 (O)772" 

---

## Show Message
Show a message box.

### Key
"DMT/message"

### Accepted Values
This action accepts a plain string (which can be split by a hashtag ('#') to separate into multiple consecutive dialogues).  
However another accepted value starts with a boolean value ("True" or "False") followed by a bar ('|') and then either a regular dialogue string (if false) or a string with mail formatting (if true).  
**Note** that regular dialogue works also when true, however mail formatting won't be applied when false.

**Note** also that some triggers which make menus appear (like Talk), will not work well with this action, use of it is not adviced.

### Example
"DMT/message_Item((O)289)": "T-This is huge!#What kind of chickens do you have on that farm!"  
"DMT/message_MonsterSlain": "True|It is done...^The ritual is complete...^Now all that rests you is to claim the loot in the last dungeon.^I promise it's the last this time...^I think...  %item quest MindMeltMax.EndlessDungeon/TotallyTheLastOne true %% [textcolor red][#]Found Letter #237"

---

## Play Event
Start an event.

### Key
"DMT/event"

### Accepted Values
This action accepts a single string with the entire event script to be played.

However this can now also be loaded directly from the asset.  
The format for this is as follows, each part must be split by a bar ('|'):
* The event script, in case you just want to include the assetName / event id for the event.
* The name of the asset, if loading from this, also include the asset key in this format: "AssetName#AssetKey"
* The event id to add it to the players seen events.

### Example
"DMT/event_Once_Enter": "|MindMeltMax.WeirdEvents#GoopAliens|goopAlienLanding"  
"DMT/event_Once_On": "I need to write an event script here..."

---

## Set Mail Flag
Add a mail flag from the player if they don't have it yet.

### Key
"DMT/mail"

### Accepted Value
The mail flag you wish to set, if not added already.

### Example
"DMT/mail_Once_Talk(Haley)": "MindMeltMax.BestGirl/MetTheQueen"

---

## Remove Mail Flag
Remove a mail flag from the player if they have it.

### Key
"DMT/mailRemove"

### Accepted Value
The mail flag you wish to remove, if it's known by the player.

### Example
"DMT/mailRemove_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/FriendOfTheGoop"

---

## Add Mail To Mailbox
Add mail for tomorrow to the players mailbox.

### Key
"DMT/mailbox"

### Accepted Value
The id of the item in ``Data\\Mail`` you wish to add to the mailbox, if it's not already added.

### Example
"DMT/mailbox_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/MYSONHOWCOULDYOU"

---

## Invalidate Asset
Invalidate an asset from the game's asset cache, forcing it to be reloaded when next requested.

### Key
"DMT/invalidate"

### Accepted Value
The name of the asset you want to invalidate, may be split by bars ('|') to invalidate multiple assets

### Example
"DMT/invalidate_Enter": "Data/Locations"  
"DMT/invalidate_On": "Data/Objects|Strings/Objects|Strings/BigCraftables|Data/BigCraftables"

---

## Play Music
Play an existing music track.

### Key
"DMT/music"

### Accepted Value
The name of a known music track. This does accept mod added songs in the same way.

### Example
"DMT/music_Enter": "winter1"

---

## Change Health
Add or remove health from the player.

### Key
"DMT/health"

### Accepted Value
A number value in string form. Can be either positive to add health, or negative to remove health.

### Example
"DMT/health_On" "-12"  
"DMT/health_MonsterSlain": "2"

---

## Change Stamina
Add or remove stamina from the player.

### Key
"DMT/stamina"

### Accepted Value
A number value in string form. Can be either positive to add health, or negative to remove health.

**Note** that the value accepts whole and decimal values.

### Example
"DMT/stamina_Item((O)306)": "25"  
"DMT/stamina_Talk(Piere)": "-5"

---

## Change Health Every Second
Add or remove health from the player every second for a duration.

### Key
"DMT/healthPerSecond"

### Accepted Value
2 number values in string form split by a bar ('|'). The first value specifies the number of seconds to update health for, the second the amount of health restored/taken.

### Example
"DMT/healthPerSecond_On": "10|2"  
"DMT/healthPerSecond_Off": "15|-1"

---

## Change Stamina Every Second
Add or remove stamina from the player every second for a duration.

### Key
"DMT/staminaPerSecond"

### Accepted Value
2 number values in string form split by a bar ('|'). The first value specifies the number of seconds to update stamina for, the second the amount of stamina restored/taken.

**Note** that the second number (the actual stamina value) accepts whole and decimal values.

### Example
"DMT/staminaPerSecond_Once_Enter": "60|1.5"  
"DMT/staminaPerSecond_Explode": "5|-2"

---

## Add Buff
Add a buff or debuff to the player.

### Key
"DMT/buff"

### Accepted Value
A string with multiple id's of items in ``Data\\Buffs`` separated by bars ('|'). Each id can also have a custom display source specified when followed by a comma (',').

### Example
"DMT/buff_Once_On": "22|27"  
"DMT/buff_On": "MindMeltMax.WeirdEvents/Gooped,Sludge Tile"

---

## Set Speed
Set a speed multiplier for the player.

### Key
"DMT/speed"

### Accepted Value
A decimal number value in string form.

### Example
"DMT/speed_Enter": "1.2"

---

## Move (Experimental)
Move the player in a direction.

### Key
"DMT/move"

### Accepted Value
2 decimal number values in string form split by a space (' '). These numbers represent the direction the player should move in, where **1** is 1 pixel moved.  
The first number is the x position, setting this to negative makes the player move left, while a positive value makes the player move right.  
The second number is the y position, same logic applies here: negative = down, positive = up.

**Note** that the general rule is 64 pixels = 1 tile, so to make the player move one tile right, the first number needs to be 64.

### Example
"DMT/move_On": "256, 77"  
"DMT/move_Off": "-122, 320"

---

## Emote
Make the player perform an emote.

### Key
"DMT/emote"

### Accepted Value
The numerical id of the emote in string form. This can be found by opening ``Tilesheets\\emotes`` and picking the emote you want. The id will be: (4 * The row of the emote) + 4; the + 4 is added because the first row in the texture is skipped.

### Example
"DMT/emote_Talk(Abigail)": "52"

---

## Slippery (Experimental)
Make the player slide across tiles.

### Key
"DMT/slippery"

### Accepted value
A decimal number in string form representing the speed at which to slide the player.

**Note** this value stacks when surrounding tiles also have this property.
**Note** also, this property does not accept triggers and will only activate when stepped on.

### Example
"DMT/slippery": "1.2"

---

## Animation (Experimental)
Yea, if only it was that easy. Animations are such complex objects (even with the new format), that they deserve their own file, which you can find [here]()...  
Hey! did I mention I completely re-wrote the animation format!  
![Please help I'm going insane and can't take this anymore](https://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExbDdoeTV6aDV6NWFyYnRydm5kN3EzMnp2eTFpeXdkYWFsOXdxYXFldyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/UKF08uKqWch0Y/giphy.gif)

---
