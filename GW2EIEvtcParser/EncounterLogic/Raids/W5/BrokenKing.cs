﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class BrokenKing : HallOfChains
    {
        // TODO - add CR icons and some mechanics
        public BrokenKing(int triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new HitOnPlayerMechanic(48066, "King's Wrath", new MechanicPlotlySetting(Symbols.TriangleLeft,Colors.LightBlue), "Cone Hit","King's Wrath (Auto Attack Cone Part)", "Cone Auto Attack",0),
            new HitOnPlayerMechanic(47531, "Numbing Breach", new MechanicPlotlySetting(Symbols.AsteriskOpen,Colors.LightBlue), "Cracks","Numbing Breach (Ice Cracks in the Ground)", "Cracks",0),
            new PlayerBuffApplyMechanic(FrozenWind, "Frozen Wind", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.Green), "Green","Frozen Wind (Stood in Green)", "Green Stack",0),
            }
            );
            Extension = "brokenking";
            Icon = "https://wiki.guildwars2.com/images/3/37/Mini_Broken_King.png";
            EncounterCategoryInformation.InSubCategoryOrder = 2;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            return new CombatReplayMap("https://i.imgur.com/JRPskkX.png",
                            (999, 890),
                            (2497, 5388, 7302, 9668)/*,
                            (-21504, -12288, 24576, 12288),
                            (19072, 15484, 20992, 16508)*/);
        }

        internal override void ComputePlayerCombatReplayActors(AbstractPlayer p, ParsedEvtcLog log, CombatReplay replay)
        {
            var green = log.CombatData.GetBuffData(SkillIDs.FrozenWind).Where(x => x.To == p.AgentItem && x is BuffApplyEvent).ToList();
            foreach (AbstractBuffEvent c in green)
            {
                int duration = 45000;
                AbstractBuffEvent removedBuff = log.CombatData.GetBuffRemoveAllData(SkillIDs.FrozenWind).FirstOrDefault(x => x.To == p.AgentItem && x.Time > c.Time && x.Time < c.Time + duration);
                int start = (int)c.Time;
                int end = start + duration;
                if (removedBuff != null)
                {
                    end = (int)removedBuff.Time;
                }
                replay.Decorations.Add(new CircleDecoration(true, 0, 100, (start, end), "rgba(100, 200, 255, 0.25)", new AgentConnector(p)));
            }
        }

        internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
        {
            IReadOnlyList<AbstractCastEvent> cls = target.GetCastEvents(log, 0, log.FightData.FightEnd);
            switch (target.ID)
            {
                case (int)ArcDPSEnums.TargetID.BrokenKing:
                    var Cone = cls.Where(x => x.SkillId == 48066).ToList();
                    foreach (AbstractCastEvent c in Cone)
                    {
                        int start = (int)c.Time;
                        int end = (int)c.EndTime;
                        int range = 450;
                        int angle = 100;
                        Point3D facing = replay.Rotations.LastOrDefault(x => x.Time <= start + 1000);
                        if (facing == null)
                        {
                            continue;
                        }
                        replay.Decorations.Add(new PieDecoration(true, 0, range, facing, angle, (start, end), "rgba(0,100,255,0.2)", new AgentConnector(target)));
                        replay.Decorations.Add(new PieDecoration(true, 0, range, facing, angle, (start + 1900, end), "rgba(0,100,255,0.3)", new AgentConnector(target)));
                    }
                    break;
                default:
                    break;
            }

        }

        internal override List<InstantCastFinder> GetInstantCastFinders()
        {
            return new List<InstantCastFinder>()
            {
                new DamageCastFinder(48218, 48218, InstantCastFinder.DefaultICD), // Biting Aura
            };
        }
        internal override void CheckSuccess(CombatData combatData, AgentData agentData, FightData fightData, IReadOnlyCollection<AgentItem> playerAgents)
        {
            SetSuccessByDeath(combatData, fightData, playerAgents, true, (int)ArcDPSEnums.TargetID.BrokenKing);
        }

        internal override string GetLogicName(CombatData combatData, AgentData agentData)
        {
            return "Statue of Ice";
        }
    }
}
