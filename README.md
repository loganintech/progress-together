# Progress Together - TShock Plugin

Progress Together is a TShock plugin that requires a configurable list of players be online for bosses to spawn. This makes sure that all players are present for boss fights, and that no one misses out on the fun or gets too far ahead. If you use this in tandem with server side characters, you can ensure that everyone is on the same pace.

## Usage

The plugin provides the following commands:

- `/progress add <player>`: Adds a player to the list of players who are required for boss spawns..
- `/progress remove <player>`: Removes a player from the list of players who are required for boss spawns.
- `/progress status`: Displays whether if plugin is enabled, whether bosses will spawn, and who is missing if they won't spawn.
- `/progress enable`: Enables the functionality of Progress Together.
- `/progress disable`: Disables the functionality of Progress Together.
- `/progress reload`: Reloads the configuration file for Progress Together.

Please note that the plugin commands require administrator or appropriate permission to use.

## Configuration

The config of Progress Together is stored in a JSON file named `progress-together.json`. Ex:

```json
{
  "enabled": true,
  "entries": [
    {
      "name": "CharacterName",
      "uuid": "UUID of Player (is always included when using /progress add)"
    }
  ]
}
```

Explanation of the configuration fields:

- `"enabled"`: A boolean value that determines whether the Progress Together functionality is active (`true`) or inactive (`false`). If set to `true`, the plugin will check the status of players in the list before allowing bosses to spawn.
- `"entries"`: An array of player entries, each containing the following fields:
    - `"name"`: The name of the player.
    - `"uuid"`: The Universally Unique Identifier (UUID) of the player.

A combination of name and uuid are used to determine player identity. If an entry has no UUID, any player on the server with that name matches. If an entry has a UUID, both the UUID AND name must match.

In the example configuration above, the plugin is enabled (`enabled: true`) and contains one entry with the player information for "CharacterName." This means that bosses will not spawn in the game if the player with the specified Name and UUID is not online.

[## Installation

To install the Progress Together plugin, follow these steps:

1. Make sure you have TShock set up and running on your Terraria server.
2. Download the latest release of the Progress Together plugin.
3. Place the downloaded plugin dll into the `ServerPlugins` folder of your TShock server.
4. Restart or reload the server to load the plugin.
]()