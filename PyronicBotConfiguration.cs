using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyronicBot
{
    public class PyronicBotConfiguration : IRocketPluginConfiguration
    {
        public ulong EventChannelId { get; set; }

        public void LoadDefaults()
        {
            EventChannelId = 1086527358921613402;
        }
    }
}
