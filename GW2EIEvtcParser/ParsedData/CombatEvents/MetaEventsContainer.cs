﻿using System.Collections.Generic;

namespace GW2EIEvtcParser.ParsedData
{
    internal class MetaEventsContainer
    {
        public BuildEvent BuildEvent { get; set; }
        public InstanceStartEvent InstanceStartEvent { get; set; }
        public LanguageEvent LanguageEvent { get; set; }
        public LogEndEvent LogEndEvent { get; set; }
        public LogStartEvent LogStartEvent { get; set; }
        public List<MapIDEvent> MapIDEvents { get; } = new List<MapIDEvent>();
        public List<ShardEvent> ShardEvents { get; } = new List<ShardEvent>();
        public PointOfViewEvent PointOfViewEvent { get; set; }
        public Dictionary<AgentItem, List<GuildEvent>> GuildEvents { get; } = new Dictionary<AgentItem, List<GuildEvent>>();
        public Dictionary<long, BuffInfoEvent> BuffInfoEvents { get; } = new Dictionary<long, BuffInfoEvent>();
        public Dictionary<ArcDPSEnums.BuffCategory, List<BuffInfoEvent>> BuffInfoEventsByCategory { get; } = new Dictionary<ArcDPSEnums.BuffCategory, List<BuffInfoEvent>>();
        public Dictionary<long, SkillInfoEvent> SkillInfoEvents { get; } = new Dictionary<long, SkillInfoEvent>();
        public List<ErrorEvent> ErrorEvents { get; } = new List<ErrorEvent>();
        public Dictionary<long, List<EffectIDToGUIDEvent>> EffectIDToGUIDEvents { get; } = new Dictionary<long, List<EffectIDToGUIDEvent>>();
        public Dictionary<string, List<EffectIDToGUIDEvent>> GUIDToEffectIDEvents { get; } = new Dictionary<string, List<EffectIDToGUIDEvent>>();
    }
}
