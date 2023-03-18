using PyronicBot.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PyronicBot.Commands
{
    public class CommandEventStart : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "eventstart";

        public string Help => "Starts an event.";

        public string Syntax => "<flags>";

        public List<string> Aliases => new() { "estart" };

        public List<string> Permissions => new() { "pyronic.estart" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                EventManager.Instance.StartEvent();
                UnturnedChat.Say(caller, $"Started event {EventManager.Instance.Title}. There are {EventManager.Instance.Teams} teams.");
            }
            else if (command.Length > 1)
            {
                if (EventManager.Instance.ReadCommandFlags(command))
                {
                    EventManager.Instance.StartEvent();
                    UnturnedChat.Say(caller, $"Started event {EventManager.Instance.Title}. There are {EventManager.Instance.Teams} teams.");
                }
                else
                {
                    EventManager.Instance.Defaults();
                    UnturnedChat.Say(caller, $"Failed to properly start an event. Invalid command flags.", Color.red);
                }
            }
        }
    }
}
