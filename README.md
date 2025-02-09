# AetherLink

![AetherLink Banner](image_url)

AetherLink is a plugin that bridges in-game chat in Final Fantasy XIV with Discord, allowing you to receive in-game messages from a Discord bot and reply to them via in-game commands.

## Features
- Receive Discord messages in FFXIV chat
- Reply to Discord messages using in-game commands
- Supports Free Company, Linkshell, and Party chat integration

## Installation

### Requirements
- Final Fantasy XIV installed
- Discord bot set up with necessary permissions

### Steps
1. Open Dalamud settings in-game.
2. Navigate to the **Experimental** tab and add the following repository:
   - `https://your-repository-link.com`
3. Click **Save**.
4. Open the **Plugin Installer** in Dalamud.
5. Search for **AetherLink** and click **Install**.

## Setting Up a Discord Bot

To use AetherLink, you need a Discord bot with the correct permissions.

### Creating a Discord Bot
1. Go to the [Discord Developer Portal](https://discord.com/developers/applications)
2. Click **New Application** and enter a name
3. Navigate to the **Bot** tab and click **Add Bot**
4. Copy the **Token** (you'll need this later)

![Discord Bot Setup](image_url)

### Adding the Bot to Your Server
1. Go to the **OAuth2** tab and select **URL Generator**
2. Under **Scopes**, select `bot`
3. Under **Bot Permissions**, enable:
   - Read Messages
   - Send Messages
   - Manage Messages (optional)
4. Copy the generated URL and paste it in your browser
5. Select your server and authorize the bot

![Bot Invitation](image_url)

## Getting Your Discord User ID
To configure the plugin, you'll need your Discord user ID. Here's how to find it:
1. Open Discord and go to **User Settings**.
2. Navigate to **Advanced** and enable **Developer Mode**.
3. Right-click your username in any server or Direct Messages.
4. Click **Copy ID** – this is your Discord user ID.

## Configuration
Open the AetherLink plugin settings in-game and enter:
- **Bot Token**: Paste your bot token here.
- **User ID**: Enter your Discord user ID.

## Usage
- Use commands to send messages in-game.
- Works on both desktop and mobile Discord!
- Send messages in Discord to see them appear in FFXIV.

## Contributing
Feel free to submit issues or pull requests to improve AetherLink.

## License
This project is licensed under the MIT License.
