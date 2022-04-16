﻿using System.Collections.Generic;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class Siax : Nightmare
    {
        public Siax(int triggerID) : base(triggerID)
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new HitOnPlayerMechanic(VileSpit, "Vile Spit", new MechanicPlotlySetting("circle","rgb(70,150,0)"), "Spit","Vile Spit (green goo)", "Poison Spit",0),
            new HitOnPlayerMechanic(TailLashSiax, "Tail Lash", new MechanicPlotlySetting("triangle-left","rgb(255,200,0)"), "Tail","Tail Lash (half circle Knockback)", "Tail Lash",0),
            new SpawnMechanic((int)ArcDPSEnums.TrashID.NightmareHallucinationSiax, "Nightmare Hallucination", new MechanicPlotlySetting("star-open","rgb(0,0,0)"), "Hallu","Nightmare Hallucination Spawn", "Hallucination",0),
            new HitOnPlayerMechanic(CascadeOfTorment1, "Cascade of Torment", new MechanicPlotlySetting("circle-open","rgb(255,140,0)"), "Rings","Cascade of Torment (Alternating Rings)", "Rings", 0),
            new HitOnPlayerMechanic(CascadeOfTorment2, "Cascade of Torment", new MechanicPlotlySetting("circle-open","rgb(255,140,0)"), "Rings","Cascade of Torment (Alternating Rings)", "Rings", 0),
            new EnemyCastStartMechanic(CausticExplosion1Siax, "Caustic Explosion", new MechanicPlotlySetting("diamond-tall","rgb(255,200,0)"), "Phase","Phase Start", "Phase", 0),
            new EnemyCastEndMechanic(CausticExplosion1Siax, "Caustic Explosion", new MechanicPlotlySetting("diamond-tall","rgb(255,0,0)"), "Phase Fail","Phase Fail (Failed to kill Echos in time)", "Phase Fail", 0, (ce,log) => ce.ActualDuration >= 20649), //
            new EnemyCastStartMechanic(CausticExplosion2Siax, "Caustic Explosion", new MechanicPlotlySetting("diamond-wide","rgb(0,160,150)"), "CC","Breakbar Start", "Breakbar", 0),
            new EnemyCastEndMechanic(CausticExplosion2Siax, "Caustic Explosion", new MechanicPlotlySetting("diamond-wide","rgb(255,0,0)"), "CC Fail","Failed to CC in time", "CC Fail", 0, (ce,log) => ce.ActualDuration >= 15232),
            new PlayerBuffApplyMechanic(FixatedNightmare, "Fixated", new MechanicPlotlySetting("star-open","rgb(200,0,200)"), "Fixate", "Fixated by Volatile Hallucination", "Fixated",0),
            });
            Extension = "siax";
            Icon = "https://i.imgur.com/ar5ELOI.png";
            EncounterCategoryInformation.InSubCategoryOrder = 1;
        }

        protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
        {
            return new CombatReplayMap("https://i.imgur.com/UzaQHW9.png",
                            (476, 548),
                            (663, -4127, 3515, -997)/*,
                            (-6144, -6144, 9216, 9216),
                            (11804, 4414, 12444, 5054)*/);
        }

        protected override List<ArcDPSEnums.TrashID> GetTrashMobsIDs()
        {
            return new List<ArcDPSEnums.TrashID>
            {
                ArcDPSEnums.TrashID.SiaxHallucination
            };
        }

        internal override FightData.CMStatus IsCM(CombatData combatData, AgentData agentData, FightData fightData)
        {
            return FightData.CMStatus.CMnoName;
        }

        internal override long GetFightOffset(FightData fightData, AgentData agentData, List<CombatItem> combatData)
        {
            return GetFightOffsetByFirstInvulFilter(fightData, agentData, combatData, (int)ArcDPSEnums.TargetID.Siax, Determined762, 1500);
        }

    }
}
