﻿using LuckParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckParser.Models.ParseModels
{
    public class BuffApplyEvent : AbstractBuffEvent
    {
        private readonly bool _initial;
        public int AppliedDuration { get; }

        public BuffApplyEvent(CombatItem evtcItem, AgentData agentData, SkillData skillData, long offset) : base(evtcItem, skillData, offset)
        {
            _initial = evtcItem.IsStateChange == ParseEnum.StateChange.BuffInitial;
            AppliedDuration = evtcItem.Value;
            By = agentData.GetAgentByInstID(evtcItem.SrcMasterInstid > 0 ? evtcItem.SrcMasterInstid : evtcItem.SrcInstid, evtcItem.LogTime);
            ByMinion = evtcItem.SrcMasterInstid > 0 ? agentData.GetAgentByInstID(evtcItem.SrcInstid, evtcItem.LogTime) : null;
            To = agentData.GetAgentByInstID(evtcItem.DstInstid, evtcItem.LogTime);
        }

        public BuffApplyEvent(AgentItem by, AgentItem to, long time, int duration, SkillItem buffSkill) : base(buffSkill, time)
        {
            AppliedDuration = duration;
            By = by;
            To = to;
        }

        public override bool IsBoonSimulatorCompliant(long fightEnd)
        {
            return BuffSkill.ID != ProfHelper.NoBuff;
        }

        public override void TryFindSrc(ParsedLog log)
        {
        }

        public override void UpdateSimulator(BoonSimulator simulator)
        {
            simulator.Add(AppliedDuration, By, Time);
        }
    }
}
