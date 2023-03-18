using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PyronicBot.Components
{
    public class PyronicPlayer : MonoBehaviour
    {
        public byte EventLives { get; set; }
        public GroupInfo EventGroup { get; set; }
        public bool PartOfEvent { get; set; }

        public Player NativePlayer { get; set; }

        protected virtual void Awake()
        {
            NativePlayer = gameObject.GetComponent<Player>();
            EventLives = 0;
            EventGroup = null;
            PartOfEvent = false;
        }
    }

    public static class PyronicPlayerExtensions
    {
        public static PyronicPlayer GetPyronicPlayer(this Player player)
        {
            return player.gameObject.GetComponent<PyronicPlayer>();
        }

        public static UnturnedPlayer GetUnturnedPlayer(this Player player)
        {
            return UnturnedPlayer.FromPlayer(player);
        }
    }
}
