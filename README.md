## Card creation and balancing

We have custom, internal tools for our designers to create, test and balance cards for the game. This is done via the [Outlaws website](https://outlaws.ygstr.com/cards).

Every part of every card is created and can be tweaked on here. Card stats, events and functions are loaded from the server to the game client and always up to date with the server.

This allows us to tweak and balance the game while people are playing it without
issuing a new game build. The card images are downloaded via a [script](https://outlaws.ygstr.com/api/assets) and included in the game build.

There are three types of cards: Minion, Target Spell & AOE (Area Of Effect) Spell

**Minion** or Unit as they are referred to in-game _(for legal reasons)_ can be spawned to the battlefield. They have a mana cost as all cards, damage and health. Minions have a lot more events since they persists in the game for longer. Efter round after their spawn, they can attack once. If they have a `onPlayedTarget` event they will be able to use it immediately when they spawn.

All cards can any element: Lunar, Solar, Zenith, Nova. Some cards are Neutral, Neutral with Rush or Neutral with Taunt. If the card is not neutral it will give you a buff in a certain way depending on it's element upon being sacrificed.

If a unit with **Taunt** is present, the enemy can only attack the units with taunt.

If a unit has **Rush** they can attack immediately on being spawned.

![Card editor minion](https://user-images.githubusercontent.com/6502251/117049038-f6b56900-ad13-11eb-9b0a-61d52e827bfa.png)

**Spells** The spells do note have damage or HP, and they are only played once, then they disappear. AOE spells are played like a minion but only run their functions. The Target spell acts like an attack when played and can be used to target any minion or player on the battlefield.

![Card editor spell](https://user-images.githubusercontent.com/6502251/117049361-4dbb3e00-ad14-11eb-98fe-4c1c5bf8e04f.png)

### Card events

Every card can have events, that can be triggered throughout a game.
Events can later be filled with functions that take one integer parameter.

| Event          | Description                                                                       | Allowes Target function |
|----------------|-----------------------------------------------------------------------------------|-------------------------|
| onPlayed       | When a unit is spawned                                                            | Depends                 |
| everyRound     | Every round a unit is alive for                                                   | Depends                 |
| onDeath        | When a unit dies                                                                  | Depends                 |
| onAttacked     | When a unit takes any damage                                                      | Depends                 |
| onPlayedTarget | When a unit is played, this will allow it one target play                         | Yes                     |
| action         | When a spell is played (Can use target functions if card is of type Target Spell) | Depends                 |

### Card Functions

Functions can be devided into targeted and non-targered. Targeted functions need a target to work,
they can only be placed in a `onPlayedTarget` (minion) or a Target Spell.

| Function              | Description                                         | Value           | Target |
|-----------------------|-----------------------------------------------------|-----------------|--------|
| damageTarget          | Deals damage to the target                          | Damage amount   | Yes    |
| damageRandomAlly      | Deals damage to a random friendly (unit or player)  | Damage amount   | No     |
| damageEveryOpponent   | Deals damage to every opponent (unit and player)    | Damage amount   | No     |
| spawnMinion           | Spawns a friendly minion (no cards used from deck)  | Minion card ID  | No     |
| gainMana              | Gives player more mana (cannot exceed max mana)     | Mana amount     | No     |
| drawAmountCards       | Draws cards from deck                               | Amount of cards | No     |
| drawCard              | Draws a card (not from deck)                        | Card ID         | No     |
| damageTargetUnit      | Deals damage to target (can only be a unit)         | Damage amount   | Yes    |
| damageOpponent        | Deals damage to the opponent player                 | Damage amount   | No     |
| damageRandomOpponent  | Deals damage to random opponent (unit or player)    | Damage amount   | No     |
| damageRandomEnemyUnit | Deals damage to random enemy unit (not player)      | Damage amount   | No     |
| damageRandomAllyUnit  | Deals damage to random friendly unit (not player)   | Damage amount   | No     |
| healTarget            | Heals the target (respects max HP)                  | Health amount   | Yes    |
| healEveryAlly         | Heals every friendly (unit and player)              | Health amount   | No     |
| healRandomAlly        | Heals a reandom friendly (unit or player)           | Health amount   | No     |
| healPlayer            | Heals the player (friendly, respects max HP)        | Heal amount     | No     |
| changeTargetAttack    | Changes the target attack value (Only unit)         | Attack change+- | Yes    |
| changeTargetMaxHp     | Buffs or nerfs the targets max HP                   | Health amount+- | Yes    |
| changeAllyUnitsMaxHp  | Changes (not heals) the max HP for all allies units | MaxHP +-        | No     |
| damageAllUnits        | Deals damage to all units on the battlefield        | Damage amount   | No     |
| damageRandomUnit      | Deals damage to random Unit (any team)              | Damage amount   | No     |
| changeAlliesMaxHp     | NEEDS DOCUMENTATION                                 | Health amount   | No     |
| damageRandomAnything  | Damange any player or unit on the board             | Damage amount   | No     |