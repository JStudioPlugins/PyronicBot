using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDiscordBotCore;
using UDiscordBotCore.API;
using System.Net;
using System.IO;
using Rocket.Core.Logging;
using Steamworks;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using PyronicBot.Components;
using PyronicBot.Managers;

namespace PyronicBot
{
    public class PyronicBot : RocketPlugin<PyronicBotConfiguration>
    {
        public static PyronicBot Instance { get; private set; }
        public string Token { get; private set; }
        public static UDiscordBot Bot { get; private set; }

        protected override void Load()
        {
            Token = "SOME_TOKEN";
            Instance = this;
            Bot = new UDiscordBot(Token, new GatewayIntents[] { GatewayIntents.GUILD_MESSAGES });
            Bot.UBotAddEventHandler(DispatchEvents.MESSAGE_CREATE, (IEvent eve) => {
                MessageCreateEvent msgEve = (MessageCreateEvent)eve;
                if (msgEve.Content.Value.Contains(".link"))
                {
                    Bot.SendStandardMessage(msgEve.ChannelId, "Sorry, this feature isn't currently implemented.");
                }
                else if (msgEve.Content.Value.Contains(".raidalerts"))
                {
                    Bot.SendStandardMessage(msgEve.ChannelId, "Sorry, this feature isn't currently implemented.");
                }
                else if (msgEve.Content.Value.Contains(".data"))
                {
                    Bot.SendStandardMessage(msgEve.ChannelId, $"Sorry, this feature isn't currently implemented.");
                }

            });

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            Level.onPostLevelLoaded += Events_OnPostLevelLoaded;
        }

        private void Events_OnPostLevelLoaded(int level)
        {
            new EventManager();
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            PyronicPlayer pyronicPlayer = player.Player.GetPyronicPlayer();
            if (pyronicPlayer.PartOfEvent && EventManager.Instance.IsEventOngoing)
                EventManager.Instance.RemovePlayer(player);
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            player.Player.gameObject.AddComponent<PyronicPlayer>();
        }

        protected override void Unload()
        {
            Bot.UBotRemoveEventHandlers(DispatchEvents.MESSAGE_CREATE);
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            EventManager.Instance.CleanUp();
        }
    }
}
