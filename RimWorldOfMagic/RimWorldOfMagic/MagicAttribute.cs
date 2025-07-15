using AbilityUser;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TorannMagic
{
    public class MagicAttribute : IExposable
    {
        public  int level;

        public List<AbilityDef> TMattributeDefs;

        public AbilityDef attributeDef
        {
            get
            {
                AbilityDef result = null;
                bool flag = TMattributeDefs != null && TMattributeDefs.Count > 0;
                if (flag)
                {
                    result = TMattributeDefs[0];
                    int num = level;
                    bool flag2 = num > -1 && num < TMattributeDefs.Count;
                    if (flag2)
                    {
                        result = TMattributeDefs[num];
                    }
                    else
                    {
                        bool flag3 = num >= TMattributeDefs.Count;
                        if (flag3)
                        {
                            result = TMattributeDefs[TMattributeDefs.Count - 1];
                        }
                    }
                }
                return result;
            }
        }

        public AbilityDef nextLevelAttributeDef
        {
            get
            {
                AbilityDef result = null;
                bool flag = attributeDef != null && TMattributeDefs.Count > 0;
                if (flag)
                {
                    result = TMattributeDefs[0];
                    int num = level;
                    bool flag2 = num > -1 && num <= TMattributeDefs.Count;
                    if (flag2)
                    {
                        result = TMattributeDefs[num];
                    }
                    else
                    {
                        bool flag3 = num >= TMattributeDefs.Count;
                        if (flag3)
                        {
                            result = TMattributeDefs[TMattributeDefs.Count - 1];
                        }
                    }
                }
                return result;
            }
        }

        public AbilityDef GetAttributeDef(int index)
        {
            AbilityDef result = null;
            bool flag = TMattributeDefs != null && TMattributeDefs.Count > 0;
            if (flag)
            {
                result = TMattributeDefs[0];
                bool flag2 = index > -1 && index < TMattributeDefs.Count;
                if (flag2)
                {
                    result = TMattributeDefs[index];
                }
                else
                {
                    bool flag3 = index >= TMattributeDefs.Count;
                    if (flag3)
                    {
                        result = TMattributeDefs[TMattributeDefs.Count - 1];
                    }
                }
            }
            return result;
        }

        public MagicAttribute()
        {
        }

        public MagicAttribute(List<AbilityDef> newAttributeDefs)
        {
            TMattributeDefs = newAttributeDefs;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref level, "level", 0, false);
            Scribe_Collections.Look<AbilityDef>(ref TMattributeDefs, "TMattributeDefs", LookMode.Def, null);
        }
    }
}
