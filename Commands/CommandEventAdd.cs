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
    public class CommandEventAdd : IRocketCommand
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
                if (otherPlayer != null)
                {
                    if (command.Length > 1 && byte.TryParse(command[2], out byte team))
                    {
                        if (team != 0 && EventManager.Instance.Teams >= team)
                        {
                            EventManager.Instance.AddPlayer(otherPlayer, team);
                            UnturnedChat.Say($"Added {otherPlayer.DisplayName} to the event on team {team}.");
                        }
                        else
                        {
                            EventManager.Instance.AddPlayer(otherPlayer);
                            UnturnedChat.Say($"Added {otherPlayer.DisplayName} to the event.");
                        }
                    }
                    else
                    {
                        UnturnedChat.Say($"Added {otherPlayer.DisplayName} to the event");
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Sorry, but invalid parameters.", Color.red);
                }
            }
        }
        
    }
}
