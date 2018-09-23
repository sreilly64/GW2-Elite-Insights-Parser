﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LuckParser.Models.HtmlModels
{
    [DataContract]
    public class PlayerDto
    {
        [DataMember] public int group;
        [DataMember] public string name;
        [DataMember] public string acc;
        [DataMember] public string profession;
        [DataMember] public int condi;
        [DataMember] public int conc;
        [DataMember] public int heal;
        [DataMember] public int tough;
        [DataMember] public readonly List<MinionDto> minions = new List<MinionDto>();
        [DataMember] public string[] weapons;
        [DataMember] public string colBoss;
        [DataMember] public string colCleave;
        [DataMember] public string colTotal;

        public PlayerDto() { }

        public PlayerDto(int group, string name, string acc, string profession)
        {
            this.group = group;
            this.name = name;
            this.acc = acc;
            this.profession = profession;
        }
    }
}
