﻿using System.Collections.Generic;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EIData
{
    internal class WeaponSwapCastFinder : InstantCastFinder
    {
        public delegate bool WeaponSwapCastChecker(WeaponSwapEvent evt, CombatData combatData, SkillData skillData);
        private WeaponSwapCastChecker _triggerCondition { get; set; }

        private readonly long _swappedTo;
        public WeaponSwapCastFinder(long skillID, long swappedTo) : base(skillID)
        {
            _swappedTo = swappedTo;
        }
        internal WeaponSwapCastFinder UsingChecker(WeaponSwapCastChecker checker)
        {
            _triggerCondition = checker;
            return this;
        }

        public override List<InstantCastEvent> ComputeInstantCast(CombatData combatData, SkillData skillData, AgentData agentData)
        {
            var res = new List<InstantCastEvent>();
            foreach (AgentItem playerAgent in agentData.GetAgentByType(AgentItem.AgentType.Player))
            {
                IReadOnlyList<WeaponSwapEvent> swaps = combatData.GetWeaponSwapData(playerAgent);
                long lastTime = int.MinValue;
                foreach (WeaponSwapEvent swap in swaps)
                {
                    if (swap.SwappedTo != _swappedTo)
                    {
                        continue;
                    }
                    if (swap.Time - lastTime < ICD)
                    {
                        lastTime = swap.Time;
                        continue;
                    }
                    if (_triggerCondition != null)
                    {
                        if (_triggerCondition(swap, combatData, skillData))
                        {
                            lastTime = swap.Time;
                            res.Add(new InstantCastEvent(swap.Time, skillData.Get(SkillID), swap.Caster));
                        }
                    }
                    else
                    {
                        lastTime = swap.Time;
                        res.Add(new InstantCastEvent(swap.Time, skillData.Get(SkillID), swap.Caster));
                    }
                }
            }
            return res;
        }
    }
}
