﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EIData
{
    internal class DamageCastFinder : InstantCastFinder
    {
        public delegate bool DamageCastChecker(AbstractHealthDamageEvent evt, ParsedEvtcLog log);
        private DamageCastChecker _triggerCondition { get; set; }

        private readonly long _damageSkillID;
        public DamageCastFinder(long skillID, long damageSkillID ) : base(skillID)
        {
            UsingNotAccurate(true);
            _damageSkillID = damageSkillID;
        }

        internal DamageCastFinder UsingChecker(DamageCastChecker checker)
        {
            _triggerCondition = checker;
            return this;
        }

        public override List<InstantCastEvent> ComputeInstantCast(ParsedEvtcLog log)
        {
            var res = new List<InstantCastEvent>();
            var damages = log.CombatData.GetDamageData(_damageSkillID).GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToList());
            foreach (KeyValuePair<AgentItem, List<AbstractHealthDamageEvent>> pair in damages)
            {
                long lastTime = int.MinValue;
                foreach (AbstractHealthDamageEvent de in pair.Value)
                {
                    if (de.Time - lastTime < ICD)
                    {
                        lastTime = de.Time;
                        continue;
                    }
                    if (_triggerCondition != null)
                    {
                        if (_triggerCondition(de, log))
                        {
                            lastTime = de.Time;
                            res.Add(new InstantCastEvent(de.Time, log.SkillData.Get(SkillID), de.From));
                        }
                    }
                    else
                    {
                        lastTime = de.Time;
                        res.Add(new InstantCastEvent(de.Time, log.SkillData.Get(SkillID), de.From));
                    }
                }
            }
            return res;
        }
    }
}
