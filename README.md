# Progress Together - TShock Plugin

Progress Together is a TShock plugin that requires a configurable list of players be online for bosses to spawn. This makes sure that all players are present for boss fights, and that no one misses out on the fun or gets too far ahead. If you use this in tandem with server side characters, you can ensure that everyone is on the same pace.

## Usage

The plugin provides the following commands:

- `/progress add <player>`: Adds a player to the list of players who are required for boss spawns..
- `/progress remove <player>`: Removes a player from the list of players who are required for boss spawns.
- `/progress status`: Displays whether if plugin is enabled, whether bosses will spawn, and who is missing if they won't spawn.
- `/progress list`: Lists all players who are required for bosses to spawn.
- `/progress enable`: Enables the functionality of Progress Together.
- `/progress disable`: Disables the functionality of Progress Together.
- `/progress reload`: Reloads the configuration file for Progress Together.

Please note that the plugin commands require administrator or appropriate permission to use.

## Configuration

The config of Progress Together is stored in a JSON file named `progress-together.json`. Ex:

```json5
{
  "Settings": {
    "enabled": true,
    "logBossSpawns": true,
    "addOnLogin": false,
    "sendMissedBossesOnJoin": true,
    "missedBosses": {
      // This will send a message to any player who joins named "Character Name" saying they missed the King Slime boss
      "Character Name": [
        {
          "name": "King Slime",
          "netId": 50
        },
      ],
      // This will send a message to any player who joins named "Character Name" AND a UUID of REALLYLONGUUIDHERE saying they missed the King Slime boss
      "Character Name:REALLYLONGUUIDHERE": [
        {
          "name": "King Slime",
          "netId": 50
        },
      ]
    },
    "entries": [
      {
        "name": "CharacterName",
        // It's safer to add UUIDs, that way players can't impersonate your name.
        // However if you play from multiple devices on the same character, this will block progress if you're not on your main device
        "uuid": "UUID of Player (is always included when using /progress add)"
      }
    ],
    // These are bosses that are allowed to spawn regardless of progression
    "uncheckedBosses": [
      {
        "name": "Eye of Cthulhu",
        "netId": 4
      },
      {
        "name": "Skeletron",
        "netId": 35
      },
      {
        "name": "Moon Lord",
        "netId": 396
      }
      // netId's are not required, but if they're included they'll be used to check first
      // Otherwise, we'll check the name
      // For a full list of boss names and their net IDs, see https://terraria.wiki.gg/wiki/NPC_IDs
      // If `sendMissedBossesOnJoin` is enabled, the plugin will still send a message to players who missed these bosses
    ],
  },
}
```

Explanation of the configuration fields:

- `"enabled"`: A boolean value that determines whether the Progress Together functionality is active (`true`) or inactive (`false`). If set to `true`, the plugin will check the status of players in the list before allowing bosses to spawn.
- `"logBossSpawns"`: A boolean value that, when set to `true`, instructs the plugin to log boss spawns for the first time or when a boss spawn is blocked to a file.
- `"addOnLogin"`: A boolean value that, when set to `true`, automatically adds newly joined players (by name only) to the progress requirement list. This means that players who join the server will be included in the tracking system without any additional action.
- `"entries"`: An array of player entries, where each entry contains the following fields:
  - `"name"`: The name of the player. This field holds the player's in-game character name.
  - `"uuid"`: The Universally Unique Identifier (UUID) of the player. This field provides a unique identification for the player. It's recommended to use UUIDs, especially to prevent impersonation of player names. However, using UUIDs might cause issues if a player logs in from multiple devices using the same character name, as it could block progress on non-main devices.
- `"missedBosses"`: A dictionary where each key represents the name of a player, and the associated value is an array of bosses that the player has missed. Each boss in the array is defined by:
  - `"name"`: The name of the boss.
  - `"netId"`: (Optional) The network identifier (netId) of the boss. If included, this netId is used to check boss status; otherwise, the boss's name is checked. This section is used to send a message to players who join and have missed specific bosses.
- `"uncheckedBosses"`: An array of bosses that are allowed to spawn regardless of progression. Each boss in the array is defined by:
  - `"name"`: The name of the boss.
  - `"netId"`: (Optional) The network identifier (netId) of the boss. If included, this netId is used to check boss status; otherwise, the boss's name is checked. Bosses listed here are exceptions and can spawn even if the progression-required players are not online. Messages are still sent to players who join and have missed these bosses if `"sendMissedBossesOnJoin"` is enabled.


A combination of name and uuid are used to determine player identity. If an entry has no UUID, any player on the server with that name matches. If an entry has a UUID, both the UUID AND name must match.

In the example configuration above, the plugin is enabled (`enabled: true`) and contains one entry with the player information for "CharacterName." This means that bosses will not spawn in the game if the player with the specified Name and UUID is not online.

## Installation

To install the Progress Together plugin, follow these steps:

1. Make sure you have TShock set up and running on your Terraria server.
2. Download the latest release of the Progress Together plugin.
3. Place the downloaded plugin dll into the `ServerPlugins` folder of your TShock server.
4. Restart or reload the server to load the plugin.
