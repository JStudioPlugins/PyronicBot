using PyronicBot.Components;
using PyronicBot.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PyronicBot.Commands
{
    public class CommandEventRemove : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "eventadd";

        public string Help => "Adds a player to an event, and perhaps a team.";

        public string Syntax => "<player> [team]";

        public List<string> Aliases => new();

        public List<string> Permissions => new() { "pyronic.eadd" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, "Sorry, but invalid parameters.", Color.red);
            }
            else if (command.Length >= 1)
            {
                UnturnedPlayer otherPlayer = PlayerTool.getPlayer(command[0]).GetUnturnedPlayer();
                PyronicPlayer pyronicPlayer = otherPlayer.Player.GetPyronicPlayer();
                if (otherPlayer != null)
                {
                    if (pyronicPlayer.PartOfEvent)
                    {
                        EventManager.Instance.RemovePlayer(otherPlayer);
                        UnturnedChat.Say(caller, $"Removed {otherPlayer.DisplayName} from the event");
                    }
                    else
                        UnturnedChat.Say(caller, $"{otherPlayer.DisplayName} is not part of the event!", Color.red);
                }
                else
                {
                    UnturnedChat.Say(caller, "Sorry, but invalid parameters.", Color.red);
                }
            }
        }

    }
}
