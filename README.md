## Card creation and balancing

We have custom, internal tools for our designers to create, test and balance cards for the game. This is done via the [Outlaws website](https://outlaws.ygstr.com/cards). Every part of every card is created and can be tweaked on here.

### Card events

Every card can have events, that can be triggered throughout a game.
Events can later be filled with functions that take one integer parameter.

| Event          | Description                                                                       | Allowes Target function |
| -------------- | --------------------------------------------------------------------------------- | ----------------------- |
| onPlayed       | When a unit is spawned                                                            | No                      |
| everyRound     | Every round a unit is alive for                                                   | No                      |
| onDeath        | When a unit dies                                                                  | No                      |
| onAttacked     | When a unit takes any damage                                                      | No                      |
| onPlayedTarget | When a unit is played, this will allow it one target play                         | Yes                     |
| action         | When a spell is played (Can use target functions if card is of type Target Spell) | Depends                 |

### Card Functions

Functions can be devided into targeted and non-targered. Targeted functions need a target to work,
they can only be placed in a `onPlayedTarget` (minion) or a Target Spell.

| Function              | Description                                        | Value           | Target |
| --------------------- | -------------------------------------------------- | --------------- | ------ |
| damageTarget          | Deals damage to the target                         | Damage amount   | Yes    |
| damageRandomAlly      | Deals damage to a random friendly (unit or player) | Damage amount   | No     |
| damageEveryOpponent   | Deals damage to every opponent (unit and player)   | Damage amount   | No     |
| healTarget            | Heals the target (can exceed max HP)               | Health amount   | Yes    |
| healRandomAlly        | Heals a reandom friendly (unit or player)          | Health amount   | No     |
| healEveryAlly         | Heals every friendly (unit and player)             | Health amount   | No     |
| spawnMinion           | Spawns a friendly minion (no cards used from deck) | Minion card ID  | No     |
| gainMana              | Gives player more mana (cannot exceed max mana)    | Mana amount     | No     |
| drawAmountCards       | Draws cards from deck                              | Amount of cards | No     |
| drawCard              | Draws a card (not from deck)                       | Card ID         | No     |
| damageTargetUnit      | Deals damage to target (can only be a unit)        | Damage amount   | Yes    |
| damageOpponent        | Deals damage to the opponent player                | Damage amount   | No     |
| damageRandomOpponent  | Deals damage to random opponent (unit or player)   | Damage amount   | No     |
| damageRandomEnemyUnit | Deals damage to random enemy unit (not player)     | Damage amount   | No     |
| healPlayer            | Heals the player (friendly, cannot exceed max HP)  | Heal amount     | No     |
| damageAllUnits        | Deals damage to all units on the battlefield       | Damage amount   | No     |
| damageRandomUnit      | Deals damage to random Unit (any team)             | Damage amount   | No     |
| changeTargetAttack    | Changes the target attack value (Only unit)        | Attack change+- | Yes    |
