using PyronicBot.Components;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UBotDiscordCore.API;
using UDiscordBotCore.API;
using Rocket.Core.Extensions;

namespace PyronicBot.Managers
{
    public class EventManager
    {
        public static EventManager Instance { get; private set; }
        public Dictionary<byte, GroupInfo> Groups { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsEventOngoing { get; set; }
        public bool IsUsingLives { get; set; }
        public byte Teams { get; set; }
        public byte AmountLives { get; set; }

        public static Timer DiscordTimer { get; set; }
        public ulong Message { get; set; }

        public EventManager()
        {
            Instance = this;
            Defaults();
        }

        public void Defaults()
        {
            Groups = new();
            Title = "Event";
            Description = "Event";
            IsEventOngoing = false;
            IsUsingLives = false;
            AmountLives = 0;
            Teams = 0;
        }

        public void CleanUp()
        {
            EndEvent();
            Instance = null;
        }

        public void StartEvent()
        {
            for (byte i = 1; i > Teams; i++)
            {
                CSteamID GroupId = GroupManager.generateUniqueGroupID();
                Logger.Log($"Team {i} Group ID is: {GroupId}");
                GroupInfo info = GroupManager.addGroup(GroupId, $"Team {i}");
                Groups.Add(i, info);
            }

            PlayerLife.onPlayerDied += PlayerLife_onPlayerDied;
            PlayerLife.OnSelectingRespawnPoint += PlayerLife_OnSelectingRespawnPoint;
            SetupDiscordTimer();
        }

        private void PlayerLife_OnSelectingRespawnPoint(PlayerLife sender, bool wantsToSpawnAtHome, ref UnityEngine.Vector3 position, ref float yaw)
        {
            PyronicPlayer pyronicPlayer = sender.player.GetPyronicPlayer();
            if (pyronicPlayer.PartOfEvent && pyronicPlayer.EventGroup != null)
            {
                var mates = GetTeamMembers(pyronicPlayer.EventGroup);
                int index = mates.GetRandomIndex();
                if (IsUsingLives && pyronicPlayer.EventLives > 0)
                {
                    position = mates[index].player.transform.position;
                    UnturnedChat.Say(sender.player.GetUnturnedPlayer(), $"Respawning you to a team member! You have {pyronicPlayer.EventLives}/{AmountLives} lives left.");
                }
                else if (!IsUsingLives)
                {
                    position = mates[index].player.transform.position;
                    UnturnedChat.Say(sender.player.GetUnturnedPlayer(), $"Respawning you to a team member!");
                }
            }
        }

        private void PlayerLife_onPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, Steamworks.CSteamID instigator)
        {
            PyronicPlayer pyronicPlayer = sender.player.GetPyronicPlayer();
            if (pyronicPlayer.PartOfEvent && IsUsingLives && instigator != CSteamID.Nil)
            {
                pyronicPlayer.EventLives--;
            }
        }

        public void SetupDiscordTimer()
        {
            DiscordTimer = new(30000);
            DiscordTimer.AutoReset = true;
            DiscordTimer.Elapsed += DiscordTimer_Elapsed;
            DiscordTimer.Enabled = true;

            CreateMessageParams messageParams = new("Event Message");
            string description = "";
            description += Description;
            foreach (var pair in Groups)
            {
                description += $"\n\n__Team {pair.Key}__";
                foreach (SteamPlayer player in GetTeamMembers(pair.Value))
                {
                    PyronicPlayer pyronicPlayer = player.player.GetPyronicPlayer();
                    description += $"\n{player.playerID.characterName} - {pyronicPlayer.EventLives}/{AmountLives}♥";
                }
            }
            messageParams.Embeds = new(new Embed[]{
                new Embed()
                {
                    Title = Title,
                    Description = description
                }
            });
            Message message = PyronicBot.Bot.SendComplexMessage(PyronicBot.Instance.Configuration.Instance.EventChannelId, messageParams);
            Message = message.Id;
        }

        private void DiscordTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CreateMessageParams messageParams = new("Event Message");
            string description = "";
            description += Description;
            foreach (var pair in Groups)
            {
                description += $"\n\n__Team {pair.Key}__";
                foreach (SteamPlayer player in GetTeamMembers(pair.Value))
                {
                    PyronicPlayer pyronicPlayer = player.player.GetPyronicPlayer();
                    description += $"\n{player.playerID.characterName} - {pyronicPlayer.EventLives}/{AmountLives}♥";
                }
            }
            messageParams.Embeds = new(new Embed[]{
                new Embed()
                {
                    Title = Title,
                    Description = description
                }
            });
            PyronicBot.Bot.EditComplexMessage(PyronicBot.Instance.Configuration.Instance.EventChannelId, Message, messageParams);
        }

        public void EndEvent()
        {
            DiscordTimer.Stop();
            DiscordTimer = null;
            foreach (SteamPlayer player in Provider.clients)
            {
                RemovePlayer(player.player.GetUnturnedPlayer());
            }

            foreach (GroupInfo info in Groups.Values)
            {
                GroupManager.deleteGroup(info.groupID);
            }
            Defaults();

            PlayerLife.onPlayerDied -= PlayerLife_onPlayerDied;
            PlayerLife.OnSelectingRespawnPoint -= PlayerLife_OnSelectingRespawnPoint;
        }

        public bool AddPlayer(UnturnedPlayer player, byte team = 0)
        {
            if (!IsEventOngoing)
                return false;
            PyronicPlayer pyronicPlayer = player.Player.GetPyronicPlayer();
            if (Teams != 0)
            {
                GroupInfo info = Groups[team];
                player.Player.quests.ServerAssignToGroup(info.groupID, EPlayerGroupRank.MEMBER, true);
                pyronicPlayer.EventGroup = info;
                pyronicPlayer.EventLives = AmountLives;
                pyronicPlayer.PartOfEvent = true;
                return true;
            }
            pyronicPlayer.EventGroup = null;
            pyronicPlayer.EventLives = AmountLives;
            pyronicPlayer.PartOfEvent = true;
            return true;
        }

        public void RemovePlayer(UnturnedPlayer player)
        {
            player.Player.quests.leaveGroup(true);
            PyronicPlayer pyronicPlayer = player.Player.GetPyronicPlayer();
            pyronicPlayer.EventGroup = null;
            pyronicPlayer.EventLives = 0;
            pyronicPlayer.PartOfEvent = false;
        }

        public List<SteamPlayer> GetTeamMembers(GroupInfo info)
        {
            List<SteamPlayer> players = new();
            foreach (SteamPlayer player in Provider.clients)
            {
                if (player.player.quests.groupID == info.groupID)
                    players.Add(player);
            }
            return players;
        }

        public bool ReadCommandFlags(string[] command)
        {
            for (int i = 0; i > command.Length; i++)
            {
                try
                {
                    switch (command[i])
                    {
                        case "-lives":
                            IsUsingLives = true;
                            AmountLives = byte.Parse(command[i + 1]);
                            break;
                        case "-description":
                            Description = command[i];
                            break;
                        case "-title":
                            Title = command[i];
                            break;
                        case "-teams":
                            Teams = byte.Parse(command[i + 1]);
                            break;
                    }
                }
                catch
                {
                    Logger.Log("Caught a small error in command flags for events!");
                    return false;
                }
            }
            return true;
        }
    }
}
