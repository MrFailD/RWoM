using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TorannMagic
{
    public partial class CompAbilityUserMagic
    {
        private void DrawTechnoBit()
        {
            float num = Mathf.Lerp(1.2f, 1.55f, 1f);
            if (bitFloatingDown)
            {
                if (bitOffset < .38f)
                {
                    bitFloatingDown = false;
                }
                bitOffset -= .001f;
            }
            else
            {
                if (bitOffset > .57f)
                {
                    bitFloatingDown = true;
                }
                bitOffset += .001f;
            }

            bitPosition = Pawn.Drawer.DrawPos;
            bitPosition.x -= .5f + Rand.Range(-.01f, .01f);
            bitPosition.z += bitOffset;
            bitPosition.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            float angle = 0f;
            Vector3 s = new Vector3(.35f, 1f, .35f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(bitPosition, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.bitMat, 0);
        }

        private void DrawMageLight()
        {
            if (!mageLightSet)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, 1f);
                Vector3 lightPos = Vector3.zero;

                lightPos = Pawn.Drawer.DrawPos;
                lightPos.x -= .5f;
                lightPos.z += .6f;

                lightPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(.27f, .5f, .27f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(lightPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.mageLightMat, 0);
            }

        }

        public void DrawEnchantMark()
        {
            DrawMark(TM_RenderQueue.enchantMark, new Vector3(.5f, 1f, .5f), 0, -.2f);
        }

        public void DrawScornWings()
        {
            bool flag = !Pawn.Dead && !Pawn.Downed;
            if (flag)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, 1f);
                Vector3 vector = Pawn.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                if (Pawn.Rotation == Rot4.North)
                {
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.PawnState);
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(3f, 3f, 3f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                if (Pawn.Rotation == Rot4.South || Pawn.Rotation == Rot4.North)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.scornWingsNS, 0);
                }
                if (Pawn.Rotation == Rot4.East)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.scornWingsE, 0);
                }
                if (Pawn.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, TM_RenderQueue.scornWingsW, 0);
                }
            }
        }
        
        private int deathRetaliationDelayCount;
        public void DoDeathRetaliation()
        {
            if (!Pawn.Downed || Pawn.Map == null || Pawn.IsPrisoner || Pawn.Faction == null || !Pawn.Faction.HostileTo(Faction.OfPlayerSilentFail))
            {
                deathRetaliating = false;
                canDeathRetaliate = false;
                deathRetaliationDelayCount = 0;
            }
            if (canDeathRetaliate && deathRetaliating)
            {
                ticksTillRetaliation--;
                if (deathRing == null || deathRing.Count < 1)
                {
                    deathRing = TM_Calc.GetOuterRing(Pawn.Position, 1f, 2f);
                }
                if (Find.TickManager.TicksGame % 6 == 0)
                {
                    Vector3 moteVec = deathRing.RandomElement().ToVector3Shifted();
                    moteVec.x += Rand.Range(-.4f, .4f);
                    moteVec.z += Rand.Range(-.4f, .4f);
                    float angle = (Quaternion.AngleAxis(90, Vector3.up) * TM_Calc.GetVector(moteVec, Pawn.DrawPos)).ToAngleFlat();
                    ThingDef mote = TorannMagicDefOf.Mote_Psi_Grayscale;
                    mote.graphicData.color = Color.white;
                    TM_MoteMaker.ThrowGenericMote(TorannMagicDefOf.Mote_Psi_Grayscale, moteVec, Pawn.Map, Rand.Range(.25f, .6f), .1f, .05f, .05f, 0, Rand.Range(4f, 6f), angle, angle);
                }
                if (ticksTillRetaliation <= 0)
                {
                    canDeathRetaliate = false;
                    deathRetaliating = false;
                    TM_Action.CreateMagicDeathEffect(Pawn, Pawn.Position);
                }
            }
            else if (canDeathRetaliate)
            {
                if (deathRetaliationDelayCount >= 20 && Rand.Value < .04f)
                {
                    
                    deathRetaliating = true;
                    ticksTillRetaliation = Mathf.RoundToInt(Rand.Range(400, 1200) * ModOptions.Settings.Instance.deathRetaliationDelayFactor);
                    deathRing = TM_Calc.GetOuterRing(Pawn.Position, 1f, 2f);
                }
                else
                {
                    deathRetaliationDelayCount++;
                }
            }
        }
        
        private void AssignAbilities()
        {
            
            float hardModeMasterChance = .35f;
            float masterChance = .05f;
            Pawn abilityUser = Pawn;
            bool flag2;
            List<TMAbilityDef> usedAbilities = new List<TMAbilityDef>();
            usedAbilities.Clear();
            if (abilityUser != null && abilityUser.story != null && abilityUser.story.traits != null)
            {
                if (customClass != null)
                {
                    for (int z = 0; z < MagicData.AllMagicPowers.Count; z++)
                    {
                        TMAbilityDef ability = (TMAbilityDef)MagicData.AllMagicPowers[z].abilityDef;
                        if (usedAbilities.Contains(ability))
                        {
                            continue;
                        }
                        else
                        {
                            usedAbilities.Add(ability);
                        }
                        if (customClass.classMageAbilities.Contains(ability))
                        {
                            MagicData.AllMagicPowers[z].learned = true;
                        }                    
                        if (MagicData.AllMagicPowers[z].requiresScroll)
                        {
                            MagicData.AllMagicPowers[z].learned = false;
                        }
                        if (!abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false) && !Rand.Chance(ability.learnChance))
                        {
                            MagicData.AllMagicPowers[z].learned = false;
                        }                        
                        if (MagicData.AllMagicPowers[z].learned)
                        {
                            if (ability.shouldInitialize)
                            {
                                AddPawnAbility(ability);
                            }
                            if (ability.childAbilities != null && ability.childAbilities.Count > 0)
                            {
                                for (int c = 0; c < ability.childAbilities.Count; c++)
                                {
                                    if (ability.childAbilities[c].shouldInitialize)
                                    {
                                        AddPawnAbility(ability.childAbilities[c]);
                                    }
                                }
                            }
                        }                        
                    }
                    MagicPower branding = MagicData.AllMagicPowers.FirstOrDefault((MagicPower p) => p.abilityDef == TorannMagicDefOf.TM_Branding);
                    if(branding != null && branding.learned && abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Golemancer))
                    {
                        int count = 0;
                        while (count < 2)
                        {
                            TMAbilityDef tmpAbility = TM_Data.BrandList().RandomElement();
                            for (int i = 0; i < MagicData.AllMagicPowers.Count; i++)
                            {
                                TMAbilityDef ad = (TMAbilityDef)MagicData.AllMagicPowers[i].abilityDef;
                                if (!MagicData.AllMagicPowers[i].learned && ad == tmpAbility)
                                {
                                    count++;
                                    MagicData.AllMagicPowers[i].learned = true;
                                    RemovePawnAbility(ad);
                                    TryAddPawnAbility(ad);
                                }
                            }
                        }
                    }                    
                    if (customClass.classHediff != null)
                    {
                        HealthUtility.AdjustSeverity(abilityUser, customClass.classHediff, customClass.hediffSeverity);
                    }
                }
                else
                {
                    //for (int z = 0; z < this.MagicData.AllMagicPowers.Count; z++)
                    //{
                    //    this.MagicData.AllMagicPowers[z].learned = false;                        
                    //}
                    flag2 = TM_Calc.IsWanderer(abilityUser);
                    if (flag2)
                    {
                        //Log.Message("Initializing Wanderer Abilities");
                        MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_Cantrips).learned = true;
                        magicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_WandererCraft).learned = true;
                        for (int i = 0; i < 3; i++)
                        {
                            MagicPower mp = MagicData.MagicPowersStandalone.RandomElement();
                            if (mp.abilityDef == TorannMagicDefOf.TM_TransferMana)
                            {
                                mp.learned = true;
                                spell_TransferMana = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_SiphonMana)
                            {
                                mp.learned = true;
                                spell_SiphonMana = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_SpellMending)
                            {
                                mp.learned = true;
                                spell_SpellMending = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_DirtDevil)
                            {
                                mp.learned = true;
                                spell_DirtDevil = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Heater)
                            {
                                mp.learned = true;
                                spell_Heater = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Cooler)
                            {
                                mp.learned = true;
                                spell_Cooler = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_PowerNode)
                            {
                                mp.learned = true;
                                spell_PowerNode = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Sunlight)
                            {
                                mp.learned = true;
                                spell_Sunlight = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_SmokeCloud)
                            {
                                mp.learned = true;
                                spell_SmokeCloud = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Extinguish)
                            {
                                mp.learned = true;
                                spell_Extinguish = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_EMP)
                            {
                                mp.learned = true;
                                spell_EMP = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_ManaShield)
                            {
                                mp.learned = true;
                                spell_ManaShield = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Blur)
                            {
                                mp.learned = true;
                                spell_Blur = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_ArcaneBolt)
                            {
                                mp.learned = true;
                                spell_ArcaneBolt = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_LightningTrap)
                            {
                                mp.learned = true;
                                spell_LightningTrap = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Invisibility)
                            {
                                mp.learned = true;
                                spell_Invisibility = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_MageLight)
                            {
                                mp.learned = true;
                                spell_MageLight = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_Ignite)
                            {
                                mp.learned = true;
                                spell_Ignite = true;
                            }
                            else if (mp.abilityDef == TorannMagicDefOf.TM_SnapFreeze)
                            {
                                mp.learned = true;
                                spell_SnapFreeze = true;
                            }
                            else
                            {
                                int rnd = Rand.RangeInclusive(0, 4);
                                switch (rnd)
                                {
                                    case 0:
                                        MagicData.MagicPowersP.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Heal).learned = true;
                                        spell_Heal = true;
                                        break;
                                    case 1:
                                        MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Blink).learned = true;
                                        spell_Blink = true;
                                        break;
                                    case 2:
                                        MagicData.MagicPowersHoF.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rainmaker).learned = true;
                                        spell_Rain = true;
                                        break;
                                    case 3:
                                        MagicData.MagicPowersS.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonMinion).learned = true;
                                        spell_SummonMinion = true;
                                        break;
                                    case 4:
                                        MagicData.MagicPowersA.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Teleport).learned = true;
                                        spell_Teleport = true;
                                        break;
                                }
                            }
                        }
                        if (!abilityUser.IsColonist)
                        {
                            spell_ArcaneBolt = true;
                            AddPawnAbility(TorannMagicDefOf.TM_ArcaneBolt);
                        }
                        InitializeSpell();
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.InnerFire);
                    if (flag2)
                    {
                        //Log.Message("Initializing Inner Fire Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_RayofHope);
                                MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope).learned = true;
                            }
                            if (Rand.Chance(.6f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Firebolt);
                                MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Firebolt).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Fireclaw);
                                MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireclaw).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Fireball);
                                MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireball).learned = true;
                            }
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Firestorm).learned = false;
                        }
                        else
                        {
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RayofHope).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_RayofHope);
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Firebolt).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Firebolt);
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireclaw).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Fireclaw);
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Fireball).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Fireball);
                            MagicData.MagicPowersIF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Firestorm).learned = false;

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_Firestorm = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost);
                    if (flag2)
                    {
                        //Log.Message("Initializing Heart of Frost Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Soothe);
                                MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Icebolt);
                                MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Icebolt).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Snowball);
                                MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Snowball).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_FrostRay);
                                MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_FrostRay).learned = true;
                            }
                            if (Rand.Chance(.7f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Rainmaker);
                                MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rainmaker).learned = true;
                                spell_Rain = true;
                            }
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Blizzard).learned = false;
                        }
                        else
                        {
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Soothe).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Soothe);
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Icebolt).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Icebolt);
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Snowball).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Snowball);
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_FrostRay).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_FrostRay);
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rainmaker).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Rainmaker);
                            spell_Rain = true;
                            MagicData.MagicPowersHoF.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Blizzard).learned = false;

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_Blizzard = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.StormBorn);
                    if (flag2)
                    {
                        //Log.Message("Initializing Storm Born Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_AMP);
                                MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AMP).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_LightningBolt);
                                MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningBolt).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_LightningCloud);
                                MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningCloud).learned = true;
                            }
                            if (Rand.Chance(.2f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_LightningStorm);
                                MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningStorm).learned = true;
                            }
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EyeOfTheStorm).learned = false;
                        }
                        else
                        {
                            AddPawnAbility(TorannMagicDefOf.TM_AMP);
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AMP).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_LightningBolt);
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningBolt).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_LightningCloud);
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningCloud).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_LightningStorm);
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LightningStorm).learned = true;
                            MagicData.MagicPowersSB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EyeOfTheStorm).learned = false;

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_EyeOfTheStorm = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Arcanist);
                    if (flag2)
                    {
                        //Log.Message("Initializing Arcane Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Shadow);
                                MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shadow).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_MagicMissile);
                                MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_MagicMissile).learned = true;
                            }
                            if (Rand.Chance(.7f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Blink);
                                MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Blink).learned = true;
                                spell_Blink = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Summon);
                                MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Summon).learned = true;
                            }
                            if (Rand.Chance(.2f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Teleport);
                                MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Teleport).learned = true;
                                spell_Teleport = true;
                            }
                            MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_FoldReality).learned = false;
                        }
                        else
                        {
                            for(int i = 0; i < MagicData.MagicPowersA.Count; i++)
                            {
                                if (MagicData.magicPowerA[i].abilityDef != TorannMagicDefOf.TM_FoldReality)
                                {
                                    MagicData.MagicPowersA[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_Shadow);
                            AddPawnAbility(TorannMagicDefOf.TM_MagicMissile);
                            AddPawnAbility(TorannMagicDefOf.TM_Blink);
                            AddPawnAbility(TorannMagicDefOf.TM_Summon);
                            AddPawnAbility(TorannMagicDefOf.TM_Teleport);  //Pending Redesign (graphics?)
                            spell_Blink = true;
                            spell_Teleport = true;

                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Paladin);
                    if (flag2)
                    {
                        //Log.Message("Initializing Paladin Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if(Rand.Chance(TorannMagicDefOf.TM_P_RayofHope.learnChance))
                            {
                                MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_P_RayofHope).learned = true;
                                AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope);
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Heal);
                                MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Heal).learned = true;
                                spell_Heal = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Shield);
                                MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shield).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ValiantCharge);
                                MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ValiantCharge).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Overwhelm);
                                MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overwhelm).learned = true;
                            }
                            MagicData.MagicPowersP.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_HolyWrath).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersP.Count; i++)
                            {
                                if (MagicData.MagicPowersP[i].abilityDef != TorannMagicDefOf.TM_HolyWrath)
                                {
                                    MagicData.MagicPowersP[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_Heal);
                            AddPawnAbility(TorannMagicDefOf.TM_Shield);
                            AddPawnAbility(TorannMagicDefOf.TM_ValiantCharge);
                            AddPawnAbility(TorannMagicDefOf.TM_Overwhelm);
                            AddPawnAbility(TorannMagicDefOf.TM_P_RayofHope);
                            spell_Heal = true;

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_HolyWrath = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Summoner);
                    if (flag2)
                    {
                        //Log.Message("Initializing Summoner Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SummonMinion);
                                MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonMinion).learned = true;
                                spell_SummonMinion = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SummonPylon);
                                MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonPylon).learned = true;
                            }                            
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SummonExplosive);
                                MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonExplosive).learned = true;
                            }                            
                            if (Rand.Chance(.2f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SummonElemental);
                                MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonElemental).learned = true;
                            }
                            MagicData.MagicPowersS.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SummonPoppi).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersS.Count; i++)
                            {
                                if (MagicData.MagicPowersS[i].abilityDef != TorannMagicDefOf.TM_SummonPoppi)
                                {
                                    MagicData.MagicPowersS[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_SummonMinion);
                            AddPawnAbility(TorannMagicDefOf.TM_SummonPylon);
                            AddPawnAbility(TorannMagicDefOf.TM_SummonExplosive);
                            AddPawnAbility(TorannMagicDefOf.TM_SummonElemental);
                            spell_SummonMinion = true;

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_SummonPoppi = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Druid);
                    if (flag2)
                    {
                        // Log.Message("Initializing Druid Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.6f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Poison);
                                MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Poison).learned = true;
                            }                            
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal);
                                MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SootheAnimal).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Regenerate);
                                MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Regenerate).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_CureDisease);
                                MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_CureDisease).learned = true;
                            }
                            MagicData.MagicPowersD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RegrowLimb).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersD.Count; i++)
                            {
                                if (MagicData.MagicPowersD[i].abilityDef != TorannMagicDefOf.TM_RegrowLimb)
                                {
                                    MagicData.MagicPowersD[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_Poison);
                            AddPawnAbility(TorannMagicDefOf.TM_SootheAnimal);
                            AddPawnAbility(TorannMagicDefOf.TM_Regenerate);
                            AddPawnAbility(TorannMagicDefOf.TM_CureDisease);
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Necromancer) || abilityUser.story.traits.HasTrait(TorannMagicDefOf.Lich);
                    if (flag2)
                    {
                        //Log.Message("Initializing Necromancer Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_RaiseUndead);
                                MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_RaiseUndead).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_DeathMark);
                                MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_DeathMark).learned = true;
                            }                            
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_FogOfTorment);
                                MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_FogOfTorment).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse);
                                MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ConsumeCorpse).learned = true;
                            }                           
                            if (Rand.Chance(.2f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion);
                                MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_CorpseExplosion).learned = true;
                            }
                            MagicData.MagicPowersN.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_LichForm).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersN.Count; i++)
                            {
                                if (MagicData.MagicPowersN[i].abilityDef != TorannMagicDefOf.TM_LichForm)
                                {
                                    MagicData.MagicPowersN[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_RaiseUndead);
                            AddPawnAbility(TorannMagicDefOf.TM_DeathMark);
                            AddPawnAbility(TorannMagicDefOf.TM_FogOfTorment);
                            AddPawnAbility(TorannMagicDefOf.TM_ConsumeCorpse);
                            AddPawnAbility(TorannMagicDefOf.TM_CorpseExplosion);

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_DeathBolt);
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Priest);
                    if (flag2)
                    {
                        //Log.Message("Initializing Priest Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_AdvancedHeal);
                                MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AdvancedHeal).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Purify);
                                MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Purify).learned = true;
                            }                            
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_HealingCircle);
                                MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_HealingCircle).learned = true;
                            }                            
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_BestowMight);
                                MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BestowMight).learned = true;
                            }
                            MagicData.MagicPowersPR.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Resurrection).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersPR.Count; i++)
                            {
                                if (MagicData.MagicPowersPR[i].abilityDef != TorannMagicDefOf.TM_Resurrection)
                                {
                                    MagicData.MagicPowersPR[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_AdvancedHeal);
                            AddPawnAbility(TorannMagicDefOf.TM_Purify);
                            AddPawnAbility(TorannMagicDefOf.TM_HealingCircle);
                            AddPawnAbility(TorannMagicDefOf.TM_BestowMight);
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Bard);
                    if (flag2)
                    {
                        //Log.Message("Initializing Priest Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (true)
                            {
                                MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BardTraining).learned = true;
                                MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Inspire).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Entertain);
                                MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Entertain).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Lullaby);
                                MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Lullaby).learned = true;
                            }
                            MagicData.MagicPowersB.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BattleHymn).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersB.Count; i++)
                            {
                                if (MagicData.MagicPowersB[i].abilityDef != TorannMagicDefOf.TM_BattleHymn)
                                {
                                    MagicData.MagicPowersB[i].learned = true;
                                }
                            }
                            //this.AddPawnAbility(TorannMagicDefOf.TM_BardTraining);
                            AddPawnAbility(TorannMagicDefOf.TM_Entertain);
                            //this.AddPawnAbility(TorannMagicDefOf.TM_Inspire);
                            AddPawnAbility(TorannMagicDefOf.TM_Lullaby);

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_BattleHymn = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Succubus);
                    if (flag2)
                    {
                        //Log.Message("Initializing Succubus Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.7f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                                MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                                MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShadowBolt).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                                MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Attraction);
                                MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Attraction).learned = true;
                            }
                            MagicData.MagicPowersSD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Scorn).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersSD.Count; i++)
                            {
                                if (MagicData.MagicPowersSD[i].abilityDef != TorannMagicDefOf.TM_Scorn)
                                {
                                    MagicData.MagicPowersSD[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                            AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                            AddPawnAbility(TorannMagicDefOf.TM_Attraction);

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_Scorn = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Warlock);
                    if (flag2)
                    {
                        //Log.Message("Initializing Succubus Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.7f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                                MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_SoulBond).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                                MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ShadowBolt).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                                MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Dominate).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Repulsion);
                                MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Repulsion).learned = true;
                            }
                            MagicData.MagicPowersWD.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_PsychicShock).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersWD.Count; i++)
                            {
                                if (MagicData.MagicPowersWD[i].abilityDef != TorannMagicDefOf.TM_PsychicShock)
                                {
                                    MagicData.MagicPowersWD[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_SoulBond);
                            AddPawnAbility(TorannMagicDefOf.TM_ShadowBolt);
                            AddPawnAbility(TorannMagicDefOf.TM_Dominate);
                            AddPawnAbility(TorannMagicDefOf.TM_Repulsion);
                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_PsychicShock = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Geomancer);
                    if (flag2)
                    {
                        //Log.Message("Initializing Heart of Geomancer Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Stoneskin);
                                MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Stoneskin).learned = true;
                            }
                            if (Rand.Chance(.6f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Encase);
                                MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Encase).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_EarthSprites);
                                MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EarthSprites).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_EarthernHammer);
                                MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EarthernHammer).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Sentinel);
                                MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sentinel).learned = true;
                            }
                            MagicData.MagicPowersG.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Meteor).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersG.Count; i++)
                            {
                                if (!MagicData.MagicPowersG[i].abilityDef.defName.StartsWith("TM_Meteor"))
                                {
                                    MagicData.MagicPowersG[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_Stoneskin);
                            AddPawnAbility(TorannMagicDefOf.TM_Encase);
                            AddPawnAbility(TorannMagicDefOf.TM_EarthSprites);
                            AddPawnAbility(TorannMagicDefOf.TM_EarthernHammer);
                            AddPawnAbility(TorannMagicDefOf.TM_Sentinel);

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_Meteor);
                                    spell_Meteor = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Technomancer);
                    if (flag2)
                    {
                        //Log.Message("Initializing Technomancer Abilities");                        
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_TechnoShield);
                                MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoShield).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Sabotage);
                                MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Sabotage).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Overdrive);
                                MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Overdrive).learned = true;
                            }
                            MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike).learned = false;
                            if (Rand.Chance(.2f))
                            {
                                spell_OrbitalStrike = true;
                                MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike).learned = true;
                                InitializeSpell();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersT.Count; i++)
                            {
                                MagicData.MagicPowersT[i].learned = true;
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_TechnoShield);
                            AddPawnAbility(TorannMagicDefOf.TM_Sabotage);
                            AddPawnAbility(TorannMagicDefOf.TM_Overdrive);
                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    spell_OrbitalStrike = true;
                                }
                            }
                        }
                        MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoBit).learned = false;
                        MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoTurret).learned = false;
                        MagicData.MagicPowersT.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_TechnoWeapon).learned = false;
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.BloodMage);
                    if (flag2)
                    {
                        //Log.Message("Initializing Heart of Frost Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(1f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_BloodGift);
                                MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodGift).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_IgniteBlood);
                                MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_IgniteBlood).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_BloodForBlood);
                                MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodForBlood).learned = true;
                            }
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_BloodShield);
                                MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodShield).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Rend);
                                MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Rend).learned = true;
                            }
                            MagicData.MagicPowersBM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodMoon).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersBM.Count; i++)
                            {
                                if (!MagicData.MagicPowersBM[i].abilityDef.defName.StartsWith("TM_BloodMoon"))
                                {
                                    MagicData.MagicPowersBM[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_BloodGift);
                            AddPawnAbility(TorannMagicDefOf.TM_IgniteBlood);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodForBlood);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodShield);
                            AddPawnAbility(TorannMagicDefOf.TM_Rend);
                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_BloodMoon);
                                    spell_BloodMoon = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Enchanter);
                    if (flag2)
                    {
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.5f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_EnchantedBody);
                                AddPawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                                MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedAura).learned = true;
                                MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedBody).learned = true;
                                spell_EnchantedAura = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Transmutate);
                                MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Transmutate).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_EnchanterStone);
                                MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchanterStone).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_EnchantWeapon);
                                MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantWeapon).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Polymorph);
                                MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Polymorph).learned = true;
                            }
                            MagicData.MagicPowersE.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Shapeshift).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersE.Count; i++)
                            {
                                if (MagicData.MagicPowersE[i].abilityDef != TorannMagicDefOf.TM_Shapeshift)
                                {
                                    MagicData.MagicPowersE[i].learned = true;
                                }
                            }
                            MagicData.MagicPowersStandalone.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_EnchantedAura).learned = true;
                            AddPawnAbility(TorannMagicDefOf.TM_EnchantedBody);
                            AddPawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                            spell_EnchantedAura = true;
                            AddPawnAbility(TorannMagicDefOf.TM_Transmutate);
                            AddPawnAbility(TorannMagicDefOf.TM_EnchanterStone);
                            AddPawnAbility(TorannMagicDefOf.TM_EnchantWeapon);
                            AddPawnAbility(TorannMagicDefOf.TM_Polymorph);
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Chronomancer);
                    if (flag2)
                    {
                        //Log.Message("Initializing Chronomancer Abilities");
                        if (abilityUser.IsColonist && !abilityUser.health.hediffSet.HasHediff(TorannMagicDefOf.TM_Uncertainty, false))
                        {
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_Prediction);
                                MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Prediction).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_AlterFate);
                                MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AlterFate).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_AccelerateTime);
                                MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_AccelerateTime).learned = true;
                            }
                            if (Rand.Chance(.4f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ReverseTime);
                                MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ReverseTime).learned = true;
                            }
                            if (Rand.Chance(.3f))
                            {
                                AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField);
                                MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ChronostaticField).learned = true;
                            }
                            MagicData.MagicPowersC.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Recall).learned = false;
                        }
                        else
                        {
                            for (int i = 0; i < MagicData.MagicPowersC.Count; i++)
                            {
                                if (MagicData.MagicPowersC[i].abilityDef == TorannMagicDefOf.TM_Recall)
                                {
                                    MagicData.MagicPowersC[i].learned = true;
                                }
                            }
                            AddPawnAbility(TorannMagicDefOf.TM_Prediction);
                            AddPawnAbility(TorannMagicDefOf.TM_AlterFate);
                            AddPawnAbility(TorannMagicDefOf.TM_AccelerateTime);
                            AddPawnAbility(TorannMagicDefOf.TM_ReverseTime);
                            AddPawnAbility(TorannMagicDefOf.TM_ChronostaticField);

                            if (!abilityUser.IsColonist)
                            {
                                if ((ModOptions.Settings.Instance.AIHardMode && Rand.Chance(hardModeMasterChance)) || Rand.Chance(masterChance))
                                {
                                    AddPawnAbility(TorannMagicDefOf.TM_Recall);
                                    spell_Recall = true;
                                }
                            }
                        }
                    }
                    flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage);
                    if (flag2)
                    {
                        foreach (MagicPower current in MagicData.AllMagicPowers)
                        {
                            if (current.abilityDef != TorannMagicDefOf.TM_ChaosTradition)
                            {
                                current.learned = false;
                            }
                        }
                        MagicData.MagicPowersCM.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_ChaosTradition).learned = true;
                        AddPawnAbility(TorannMagicDefOf.TM_ChaosTradition);
                        TM_Calc.AssignChaosMagicPowers(this, !abilityUser.IsColonist);
                    }
                }
                AssignAdvancedClassAbilities(true);
            }
        }
        
        public void InitializeSpell()
        {
            Pawn abilityUser = Pawn;
            if (IsMagicUser)
            {
                if (customClass != null)
                {
                    for (int j = 0; j < MagicData.AllMagicPowers.Count; j++)
                    {                       
                        if (MagicData.AllMagicPowers[j].learned && !customClass.classMageAbilities.Contains(MagicData.AllMagicPowers[j].abilityDef))
                        {
                            RemovePawnAbility(MagicData.AllMagicPowers[j].abilityDef);
                            AddPawnAbility(MagicData.AllMagicPowers[j].abilityDef);                            
                        }
                    }
                    if(recallSpell)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Recall);
                        AddPawnAbility(TorannMagicDefOf.TM_Recall);
                    }
                }
                else
                {
                    if (spell_Rain && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost))
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Rainmaker);
                        AddPawnAbility(TorannMagicDefOf.TM_Rainmaker);

                    }
                    if (spell_Blink && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.Arcanist))
                    {
                        if (!abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Blink);
                            AddPawnAbility(TorannMagicDefOf.TM_Blink);
                        }
                        else
                        {
                            bool hasAbility = false;
                            for (int i = 0; i < chaosPowers.Count; i++)
                            {
                                if (chaosPowers[i].Ability == TorannMagicDefOf.TM_Blink || chaosPowers[i].Ability == TorannMagicDefOf.TM_Blink_I || chaosPowers[i].Ability == TorannMagicDefOf.TM_Blink_II || chaosPowers[i].Ability == TorannMagicDefOf.TM_Blink_III)
                                {
                                    hasAbility = true;
                                }
                            }
                            if (!hasAbility)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Blink);
                                AddPawnAbility(TorannMagicDefOf.TM_Blink);
                            }
                        }
                    }
                    if (spell_Teleport && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.Arcanist))
                    {
                        if (!(abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage) && MagicData.MagicPowersA.FirstOrDefault<MagicPower>((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Teleport).learned))
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Teleport);
                            AddPawnAbility(TorannMagicDefOf.TM_Teleport);
                        }
                    }
                    
                    if (spell_Heal && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.Paladin))
                    {
                        if (!abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Heal);
                            AddPawnAbility(TorannMagicDefOf.TM_Heal);
                        }
                        else
                        {
                            bool hasAbility = false;
                            for (int i = 0; i < chaosPowers.Count; i++)
                            {
                                if (chaosPowers[i].Ability == TorannMagicDefOf.TM_Heal)
                                {
                                    hasAbility = true;
                                }
                            }
                            if (!hasAbility)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_Heal);
                                AddPawnAbility(TorannMagicDefOf.TM_Heal);
                            }
                        }
                    }
                    if (spell_Heater)
                    {
                        //if (this.summonedHeaters == null || (this.summonedHeaters != null && this.summonedHeaters.Count <= 0))
                        //{
                        RemovePawnAbility(TorannMagicDefOf.TM_Heater);
                        AddPawnAbility(TorannMagicDefOf.TM_Heater);
                        //}
                    }
                    if (spell_Cooler)
                    {
                        //if(this.summonedCoolers == null || (this.summonedCoolers != null && this.summonedCoolers.Count <= 0))
                        //{
                        RemovePawnAbility(TorannMagicDefOf.TM_Cooler);
                        AddPawnAbility(TorannMagicDefOf.TM_Cooler);
                        //}
                    }
                    if (spell_PowerNode)
                    {
                        //if (this.summonedPowerNodes == null || (this.summonedPowerNodes != null && this.summonedPowerNodes.Count <= 0))
                        //{
                        RemovePawnAbility(TorannMagicDefOf.TM_PowerNode);
                        AddPawnAbility(TorannMagicDefOf.TM_PowerNode);
                        //}
                    }
                    if (spell_Sunlight)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Sunlight);
                        AddPawnAbility(TorannMagicDefOf.TM_Sunlight);

                    }
                    if (spell_DryGround)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_DryGround);
                        AddPawnAbility(TorannMagicDefOf.TM_DryGround);
                    }
                    if (spell_WetGround)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_WetGround);
                        AddPawnAbility(TorannMagicDefOf.TM_WetGround);
                    }
                    if (spell_ChargeBattery)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ChargeBattery);
                        AddPawnAbility(TorannMagicDefOf.TM_ChargeBattery);
                    }
                    if (spell_SmokeCloud)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SmokeCloud);
                        AddPawnAbility(TorannMagicDefOf.TM_SmokeCloud);
                    }
                    if (spell_Extinguish)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Extinguish);
                        AddPawnAbility(TorannMagicDefOf.TM_Extinguish);
                    }
                    if (spell_EMP)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_EMP);
                        AddPawnAbility(TorannMagicDefOf.TM_EMP);
                    }
                    if (spell_Blizzard)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Blizzard);
                        AddPawnAbility(TorannMagicDefOf.TM_Blizzard);
                    }
                    if (spell_Firestorm)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Firestorm);
                        AddPawnAbility(TorannMagicDefOf.TM_Firestorm);
                    }
                    if (spell_SummonMinion && !abilityUser.story.traits.HasTrait(TorannMagicDefOf.Summoner))
                    {
                        if (!abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_SummonMinion);
                            AddPawnAbility(TorannMagicDefOf.TM_SummonMinion);
                        }
                        else
                        {
                            bool hasAbility = false;
                            for (int i = 0; i < chaosPowers.Count; i++)
                            {
                                if (chaosPowers[i].Ability == TorannMagicDefOf.TM_SummonMinion)
                                {
                                    hasAbility = true;
                                }
                            }
                            if (!hasAbility)
                            {
                                RemovePawnAbility(TorannMagicDefOf.TM_SummonMinion);
                                AddPawnAbility(TorannMagicDefOf.TM_SummonMinion);
                            }
                        }
                    }
                    if (spell_TransferMana)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_TransferMana);
                        AddPawnAbility(TorannMagicDefOf.TM_TransferMana);
                    }
                    if (spell_SiphonMana)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SiphonMana);
                        AddPawnAbility(TorannMagicDefOf.TM_SiphonMana);
                    }
                    if (spell_RegrowLimb)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_RegrowLimb);
                        AddPawnAbility(TorannMagicDefOf.TM_RegrowLimb);
                    }
                    if (spell_EyeOfTheStorm)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_EyeOfTheStorm);
                        AddPawnAbility(TorannMagicDefOf.TM_EyeOfTheStorm);
                    }
                    if (spell_HeatShield)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_HeatShield);
                        AddPawnAbility(TorannMagicDefOf.TM_HeatShield);
                    }
                    if (spell_ManaShield)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ManaShield);
                        AddPawnAbility(TorannMagicDefOf.TM_ManaShield);
                    }
                    if (spell_Blur)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Blur);
                        AddPawnAbility(TorannMagicDefOf.TM_Blur);
                    }
                    if (spell_FoldReality)
                    {
                        MagicData.ReturnMatchingMagicPower(TorannMagicDefOf.TM_FoldReality).learned = true;
                        RemovePawnAbility(TorannMagicDefOf.TM_FoldReality);
                        AddPawnAbility(TorannMagicDefOf.TM_FoldReality);
                    }
                    if (spell_Resurrection)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Resurrection);
                        AddPawnAbility(TorannMagicDefOf.TM_Resurrection);
                    }
                    if (spell_HolyWrath)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_HolyWrath);
                        AddPawnAbility(TorannMagicDefOf.TM_HolyWrath);
                    }
                    if (spell_LichForm)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_LichForm);
                        AddPawnAbility(TorannMagicDefOf.TM_LichForm);
                    }
                    if (spell_Flight)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Flight);
                        AddPawnAbility(TorannMagicDefOf.TM_Flight);
                    }
                    if (spell_SummonPoppi)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SummonPoppi);
                        AddPawnAbility(TorannMagicDefOf.TM_SummonPoppi);
                    }
                    if (spell_BattleHymn)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_BattleHymn);
                        AddPawnAbility(TorannMagicDefOf.TM_BattleHymn);
                    }
                    if (spell_CauterizeWound)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_CauterizeWound);
                        AddPawnAbility(TorannMagicDefOf.TM_CauterizeWound);
                    }
                    if (spell_SpellMending)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SpellMending);
                        AddPawnAbility(TorannMagicDefOf.TM_SpellMending);
                    }
                    if (spell_FertileLands)
                    {
                        //if (this.fertileLands == null || (this.fertileLands != null && this.fertileLands.Count <= 0))
                        //{
                        RemovePawnAbility(TorannMagicDefOf.TM_FertileLands);
                        AddPawnAbility(TorannMagicDefOf.TM_FertileLands);
                        //}
                    }
                    if (spell_PsychicShock)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_PsychicShock);
                        AddPawnAbility(TorannMagicDefOf.TM_PsychicShock);
                    }
                    if (spell_Scorn)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Scorn);
                        AddPawnAbility(TorannMagicDefOf.TM_Scorn);
                    }
                    if (spell_BlankMind)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_BlankMind);
                        AddPawnAbility(TorannMagicDefOf.TM_BlankMind);
                    }
                    if (spell_ShadowStep)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ShadowStep);
                        AddPawnAbility(TorannMagicDefOf.TM_ShadowStep);
                    }
                    if (spell_ShadowCall)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ShadowCall);
                        AddPawnAbility(TorannMagicDefOf.TM_ShadowCall);
                    }
                    if (spell_Teach)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_TeachMagic);
                        AddPawnAbility(TorannMagicDefOf.TM_TeachMagic);
                    }
                    if (spell_Meteor)
                    {
                        MagicPower meteorPower = MagicData.MagicPowersG.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Meteor);
                        if (meteorPower == null)
                        {
                            meteorPower = MagicData.MagicPowersG.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Meteor_I);
                            if (meteorPower == null)
                            {
                                meteorPower = MagicData.MagicPowersG.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Meteor_II);
                                if (meteorPower == null)
                                {
                                    meteorPower = MagicData.MagicPowersG.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_Meteor_III);
                                }
                            }
                        }
                        if (meteorPower.level == 3)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Meteor_III);
                            AddPawnAbility(TorannMagicDefOf.TM_Meteor_III);
                        }
                        else if (meteorPower.level == 2)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Meteor_II);
                            AddPawnAbility(TorannMagicDefOf.TM_Meteor_II);
                        }
                        else if (meteorPower.level == 1)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Meteor_I);
                            AddPawnAbility(TorannMagicDefOf.TM_Meteor_I);
                        }
                        else
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Meteor);
                            AddPawnAbility(TorannMagicDefOf.TM_Meteor);
                        }
                    }
                    if (spell_OrbitalStrike)
                    {
                        MagicPower OrbitalStrikePower = MagicData.magicPowerT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike);
                        if (OrbitalStrikePower == null)
                        {
                            OrbitalStrikePower = MagicData.magicPowerT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_I);
                            if (OrbitalStrikePower == null)
                            {
                                OrbitalStrikePower = MagicData.magicPowerT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_II);
                                if (OrbitalStrikePower == null)
                                {
                                    OrbitalStrikePower = MagicData.magicPowerT.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_III);
                                }
                            }
                        }
                        if (OrbitalStrikePower.level == 3)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_III);
                            AddPawnAbility(TorannMagicDefOf.TM_OrbitalStrike_III);
                        }
                        else if (OrbitalStrikePower.level == 2)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_II);
                            AddPawnAbility(TorannMagicDefOf.TM_OrbitalStrike_II);
                        }
                        else if (OrbitalStrikePower.level == 1)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_I);
                            AddPawnAbility(TorannMagicDefOf.TM_OrbitalStrike_I);
                        }
                        else
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike);
                            AddPawnAbility(TorannMagicDefOf.TM_OrbitalStrike);
                        }
                    }
                    if (spell_BloodMoon)
                    {
                        MagicPower BloodMoonPower = MagicData.MagicPowersBM.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodMoon);
                        if (BloodMoonPower == null)
                        {
                            BloodMoonPower = MagicData.MagicPowersBM.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodMoon_I);
                            if (BloodMoonPower == null)
                            {
                                BloodMoonPower = MagicData.MagicPowersBM.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodMoon_II);
                                if (BloodMoonPower == null)
                                {
                                    BloodMoonPower = MagicData.MagicPowersBM.FirstOrDefault((MagicPower x) => x.abilityDef == TorannMagicDefOf.TM_BloodMoon_III);
                                }
                            }
                        }
                        if (BloodMoonPower.level == 3)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_III);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodMoon_III);
                        }
                        else if (BloodMoonPower.level == 2)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_II);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodMoon_II);
                        }
                        else if (BloodMoonPower.level == 1)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_I);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodMoon_I);
                        }
                        else
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon);
                            AddPawnAbility(TorannMagicDefOf.TM_BloodMoon);
                        }
                    }
                    if (spell_EnchantedAura)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                        AddPawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                    }
                    if (spell_Shapeshift)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Shapeshift);
                        AddPawnAbility(TorannMagicDefOf.TM_Shapeshift);
                    }
                    if (spell_ShapeshiftDW)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ShapeshiftDW);
                        AddPawnAbility(TorannMagicDefOf.TM_ShapeshiftDW);
                    }
                    if (spell_DirtDevil)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_DirtDevil);
                        AddPawnAbility(TorannMagicDefOf.TM_DirtDevil);
                    }
                    if (spell_MechaniteReprogramming)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_MechaniteReprogramming);
                        AddPawnAbility(TorannMagicDefOf.TM_MechaniteReprogramming);
                    }
                    if (spell_ArcaneBolt)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_ArcaneBolt);
                        AddPawnAbility(TorannMagicDefOf.TM_ArcaneBolt);
                    }
                    if (spell_LightningTrap)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_LightningTrap);
                        AddPawnAbility(TorannMagicDefOf.TM_LightningTrap);
                    }
                    if (spell_Invisibility)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Invisibility);
                        AddPawnAbility(TorannMagicDefOf.TM_Invisibility);
                    }
                    if (spell_BriarPatch)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_BriarPatch);
                        AddPawnAbility(TorannMagicDefOf.TM_BriarPatch);
                    }
                    if (spell_Recall)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_TimeMark);
                        AddPawnAbility(TorannMagicDefOf.TM_TimeMark);
                        if (recallSpell)
                        {
                            RemovePawnAbility(TorannMagicDefOf.TM_Recall);
                            AddPawnAbility(TorannMagicDefOf.TM_Recall);
                        }
                    }
                    if (spell_MageLight)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_MageLight);
                        AddPawnAbility(TorannMagicDefOf.TM_MageLight);
                    }
                    if (spell_SnapFreeze)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_SnapFreeze);
                        AddPawnAbility(TorannMagicDefOf.TM_SnapFreeze);
                    }
                    if (spell_Ignite)
                    {
                        RemovePawnAbility(TorannMagicDefOf.TM_Ignite);
                        AddPawnAbility(TorannMagicDefOf.TM_Ignite);
                    }
                    
                    if (IsMagicUser && MagicData.MagicPowersCustomAll != null && MagicData.MagicPowersCustomAll.Count > 0)
                    {
                        for (int j = 0; j < MagicData.MagicPowersCustomAll.Count; j++)
                        {
                            if (MagicData.MagicPowersCustomAll[j].learned)
                            {
                                RemovePawnAbility(MagicData.MagicPowersCustomAll[j].abilityDef);
                                AddPawnAbility(MagicData.MagicPowersCustomAll[j].abilityDef);
                            }
                        }
                    }
                }
                //this.UpdateAbilities();
            }
        }
        
        public void RemovePowers(bool clearStandalone = true)
        {
            Pawn abilityUser = Pawn;
            if (magicPowersInitialized && MagicData != null)
            {
                bool flag2 = true;
                if (customClass != null)
                {
                    for (int i = 0; i < MagicData.AllMagicPowers.Count; i++)
                    {
                        MagicPower mp = MagicData.AllMagicPowers[i];
                        for (int j = 0; j < mp.TMabilityDefs.Count; j++)
                        {
                            TMAbilityDef tmad = mp.TMabilityDefs[j] as TMAbilityDef;
                            if(tmad.childAbilities != null && tmad.childAbilities.Count > 0)
                            {
                                for(int k = 0; k < tmad.childAbilities.Count; k++)
                                {
                                    RemovePawnAbility(tmad.childAbilities[k]);
                                }
                            }                            
                            RemovePawnAbility(tmad);
                        }
                        mp.learned = false;
                    }
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.InnerFire);
                if (TM_Calc.IsWanderer(Pawn))
                {
                    spell_ArcaneBolt = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_ArcaneBolt);
                }
                if (abilityUser.story.traits.HasTrait(TorannMagicDefOf.ChaosMage))
                {
                    foreach (MagicPower current in MagicData.AllMagicPowersForChaosMage)
                    {
                        if (current.abilityDef != TorannMagicDefOf.TM_ChaosTradition)
                        {
                            current.learned = false;
                            foreach (TMAbilityDef tmad in current.TMabilityDefs)
                            {
                                if (tmad.childAbilities != null && tmad.childAbilities.Count > 0)
                                {
                                    for (int k = 0; k < tmad.childAbilities.Count; k++)
                                    {
                                        RemovePawnAbility(tmad.childAbilities[k]);
                                    }
                                }
                                RemovePawnAbility(tmad);
                            }
                        }
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                    RemovePawnAbility(TorannMagicDefOf.TM_NanoStimulant);
                    RemovePawnAbility(TorannMagicDefOf.TM_LightSkipMass);
                    RemovePawnAbility(TorannMagicDefOf.TM_LightSkipGlobal);
                    spell_EnchantedAura = false;
                    spell_ShadowCall = false;
                    spell_ShadowStep = false;
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowCall);
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowStep);

                }
                if (flag2)
                {
                    //Log.Message("Fixing Inner Fire Abilities");
                    foreach (MagicPower currentIF in MagicData.MagicPowersIF)
                    {
                        if (currentIF.abilityDef != TorannMagicDefOf.TM_Firestorm)
                        {
                            currentIF.learned = false;
                        }
                        RemovePawnAbility(currentIF.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_RayofHope_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_RayofHope_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_RayofHope_III);

                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.HeartOfFrost);
                if (flag2)
                {
                    //Log.Message("Fixing Heart of Frost Abilities");
                    foreach (MagicPower currentHoF in MagicData.MagicPowersHoF)
                    {
                        if (currentHoF.abilityDef != TorannMagicDefOf.TM_Blizzard)
                        {
                            currentHoF.learned = false;
                        }
                        RemovePawnAbility(currentHoF.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Soothe_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Soothe_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Soothe_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_FrostRay_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_FrostRay_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_FrostRay_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.StormBorn);
                if (flag2)
                {
                    //Log.Message("Fixing Storm Born Abilities");                   
                    foreach (MagicPower currentSB in MagicData.MagicPowersSB)
                    {
                        if (currentSB.abilityDef != TorannMagicDefOf.TM_EyeOfTheStorm)
                        {
                            currentSB.learned = false;
                        }
                        RemovePawnAbility(currentSB.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_AMP_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_AMP_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_AMP_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Arcanist);
                if (flag2)
                {
                    //Log.Message("Fixing Arcane Abilities");
                    foreach (MagicPower currentA in MagicData.MagicPowersA)
                    {
                        if (currentA.abilityDef != TorannMagicDefOf.TM_FoldReality)
                        {
                            currentA.learned = false;
                        }
                        RemovePawnAbility(currentA.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Shadow_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Shadow_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Shadow_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_MagicMissile_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_MagicMissile_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_MagicMissile_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_Blink_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Blink_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Blink_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_Summon_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Summon_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Summon_III);

                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Paladin);
                if (flag2)
                {
                    //Log.Message("Fixing Paladin Abilities");
                    foreach (MagicPower currentP in MagicData.MagicPowersP)
                    {
                        if (currentP.abilityDef != TorannMagicDefOf.TM_HolyWrath)
                        {
                            currentP.learned = false;
                        }
                        RemovePawnAbility(currentP.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Shield_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Shield_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Shield_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Summoner);
                if (flag2)
                {
                    foreach (MagicPower currentS in MagicData.MagicPowersS)
                    {
                        if (currentS.abilityDef != TorannMagicDefOf.TM_SummonPoppi)
                        {
                            currentS.learned = false;
                        }
                        RemovePawnAbility(currentS.abilityDef);
                    }
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Druid);
                if (flag2)
                {
                    foreach (MagicPower currentD in MagicData.MagicPowersD)
                    {
                        if (currentD.abilityDef != TorannMagicDefOf.TM_RegrowLimb)
                        {
                            currentD.learned = false;
                        }
                        RemovePawnAbility(currentD.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_SootheAnimal_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_SootheAnimal_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_SootheAnimal_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Necromancer) || abilityUser.story.traits.HasTrait(TorannMagicDefOf.Lich);
                if (flag2)
                {
                    foreach (MagicPower currentN in MagicData.MagicPowersN)
                    {
                        if (currentN.abilityDef != TorannMagicDefOf.TM_LichForm)
                        {
                            currentN.learned = false;
                        }
                        RemovePawnAbility(currentN.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathMark_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathMark_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathMark_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_ConsumeCorpse_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_CorpseExplosion_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_CorpseExplosion_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_CorpseExplosion_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathBolt_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathBolt_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_DeathBolt_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Priest);
                if (flag2)
                {
                    foreach (MagicPower currentPR in MagicData.MagicPowersPR)
                    {
                        if (currentPR.abilityDef != TorannMagicDefOf.TM_Resurrection)
                        {
                            currentPR.learned = false;
                        }
                        RemovePawnAbility(currentPR.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_HealingCircle_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_HealingCircle_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_HealingCircle_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_BestowMight_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_BestowMight_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_BestowMight_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.TM_Bard);
                if (flag2)
                {
                    foreach (MagicPower currentB in MagicData.MagicPowersB)
                    {
                        if (currentB.abilityDef != TorannMagicDefOf.TM_BattleHymn)
                        {
                            currentB.learned = false;
                        }
                        RemovePawnAbility(currentB.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Lullaby_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Lullaby_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Lullaby_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Succubus);
                if (flag2)
                {
                    foreach (MagicPower currentSD in MagicData.MagicPowersSD)
                    {
                        if (currentSD.abilityDef != TorannMagicDefOf.TM_Scorn)
                        {
                            currentSD.learned = false;
                        }
                        RemovePawnAbility(currentSD.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowBolt_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowBolt_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_ShadowBolt_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_Attraction_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Attraction_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Attraction_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Warlock);
                if (flag2)
                {
                    foreach (MagicPower currentWD in MagicData.MagicPowersWD)
                    {
                        if (currentWD.abilityDef != TorannMagicDefOf.TM_PsychicShock)
                        {
                            currentWD.learned = false;
                        }
                        RemovePawnAbility(currentWD.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Repulsion_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Repulsion_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Repulsion_III);
                }
                // flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Geomancer);
                if (flag2)
                {
                    foreach (MagicPower currentG in MagicData.MagicPowersG)
                    {
                        if (currentG.abilityDef == TorannMagicDefOf.TM_Meteor || currentG.abilityDef == TorannMagicDefOf.TM_Meteor_I || currentG.abilityDef == TorannMagicDefOf.TM_Meteor_II || currentG.abilityDef == TorannMagicDefOf.TM_Meteor_III)
                        {
                            currentG.learned = true;
                        }
                        else
                        {
                            currentG.learned = false;
                        }
                        RemovePawnAbility(currentG.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Encase_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Encase_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Encase_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_Meteor_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Meteor_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Meteor_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Technomancer);
                if (flag2)
                {
                    foreach (MagicPower currentT in MagicData.MagicPowersT)
                    {
                        if (currentT.abilityDef == TorannMagicDefOf.TM_OrbitalStrike || currentT.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_I || currentT.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_II || currentT.abilityDef == TorannMagicDefOf.TM_OrbitalStrike_III)
                        {
                            currentT.learned = true;
                        }
                        else
                        {
                            currentT.learned = false;
                        }
                        RemovePawnAbility(currentT.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_OrbitalStrike_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.BloodMage);
                if (flag2)
                {
                    foreach (MagicPower currentBM in MagicData.MagicPowersBM)
                    {
                        if (currentBM.abilityDef == TorannMagicDefOf.TM_BloodMoon || currentBM.abilityDef == TorannMagicDefOf.TM_BloodMoon_I || currentBM.abilityDef == TorannMagicDefOf.TM_BloodMoon_II || currentBM.abilityDef == TorannMagicDefOf.TM_BloodMoon_III)
                        {
                            currentBM.learned = true;
                        }
                        else
                        {
                            currentBM.learned = false;
                        }
                        RemovePawnAbility(currentBM.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Rend_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Rend_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Rend_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_BloodMoon_III);
                }
                //flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Enchanter);
                if (flag2)
                {

                    foreach (MagicPower currentE in MagicData.MagicPowersE)
                    {
                        if (currentE.abilityDef != TorannMagicDefOf.TM_Shapeshift)
                        {
                            currentE.learned = false;
                        }
                        RemovePawnAbility(currentE.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_Polymorph_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_Polymorph_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_Polymorph_III);
                    RemovePawnAbility(TorannMagicDefOf.TM_EnchantedAura);
                }
                // flag2 = abilityUser.story.traits.HasTrait(TorannMagicDefOf.Chronomancer);
                if (flag2)
                {
                    foreach (MagicPower currentC in MagicData.MagicPowersC)
                    {
                        if (currentC.abilityDef != TorannMagicDefOf.TM_Recall)
                        {
                            currentC.learned = false;
                        }
                        RemovePawnAbility(currentC.abilityDef);
                    }
                    RemovePawnAbility(TorannMagicDefOf.TM_ChronostaticField_I);
                    RemovePawnAbility(TorannMagicDefOf.TM_ChronostaticField_II);
                    RemovePawnAbility(TorannMagicDefOf.TM_ChronostaticField_III);
                }
                if (flag2)
                {
                    foreach (MagicPower currentShadow in MagicData.MagicPowersShadow)
                    {
                        RemovePawnAbility(currentShadow.abilityDef);
                    }
                }
                if (clearStandalone)
                {
                    foreach (MagicPower currentStandalone in MagicData.MagicPowersStandalone)
                    {
                        RemovePawnAbility(currentStandalone.abilityDef);
                    }
                }
            }
        }
        
        private void LoadPowers()
        {
            foreach (MagicPower currentA in MagicData.MagicPowersA)
            {
                //Log.Message("Removing ability: " + currentA.abilityDef.defName.ToString());
                currentA.level = 0;
                RemovePawnAbility(currentA.abilityDef);
            }
            foreach (MagicPower currentHoF in MagicData.MagicPowersHoF)
            {
                //Log.Message("Removing ability: " + currentHoF.abilityDef.defName.ToString());
                currentHoF.level = 0;
                RemovePawnAbility(currentHoF.abilityDef);
            }
            foreach (MagicPower currentSB in MagicData.MagicPowersSB)
            {
                //Log.Message("Removing ability: " + currentSB.abilityDef.defName.ToString());
                currentSB.level = 0;
                RemovePawnAbility(currentSB.abilityDef);
            }
            foreach (MagicPower currentIF in MagicData.MagicPowersIF)
            {
                //Log.Message("Removing ability: " + currentIF.abilityDef.defName.ToString());
                currentIF.level = 0;
                RemovePawnAbility(currentIF.abilityDef);
            }
            foreach (MagicPower currentP in MagicData.MagicPowersP)
            {
                //Log.Message("Removing ability: " + currentP.abilityDef.defName.ToString());
                currentP.level = 0;
                RemovePawnAbility(currentP.abilityDef);
            }
            foreach (MagicPower currentS in MagicData.MagicPowersS)
            {
                //Log.Message("Removing ability: " + currentP.abilityDef.defName.ToString());
                currentS.level = 0;
                RemovePawnAbility(currentS.abilityDef);
            }
            foreach (MagicPower currentD in MagicData.MagicPowersD)
            {
                //Log.Message("Removing ability: " + currentP.abilityDef.defName.ToString());
                currentD.level = 0;
                RemovePawnAbility(currentD.abilityDef);
            }
            foreach (MagicPower currentN in MagicData.MagicPowersN)
            {
                //Log.Message("Removing ability: " + currentP.abilityDef.defName.ToString());
                currentN.level = 0;
                RemovePawnAbility(currentN.abilityDef);
            }
        }
        
        public float ActualManaCost(TMAbilityDef magicDef)
        {
            float adjustedManaCost = magicDef.manaCost;
            if (magicDef.efficiencyReductionPercent != 0)
            {
                if (magicDef == TorannMagicDefOf.TM_EnchantedAura)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_EnchantedBody).level);
                }
                else if (magicDef == TorannMagicDefOf.TM_ShapeshiftDW)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Shapeshift).level);
                }
                else if (magicDef == TorannMagicDefOf.TM_ShadowStep || magicDef == TorannMagicDefOf.TM_ShadowCall)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_SoulBond).level);
                }
                else if( magicDef == TorannMagicDefOf.TM_LightSkipGlobal || magicDef == TorannMagicDefOf.TM_LightSkipMass)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_LightSkip).level);
                }      
                else if(magicDef == TorannMagicDefOf.TM_SummonTotemEarth || magicDef == TorannMagicDefOf.TM_SummonTotemHealing || magicDef == TorannMagicDefOf.TM_SummonTotemLightning)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Totems).level);
                }
                else if (magicDef == TorannMagicDefOf.TM_Hex_CriticalFail || magicDef == TorannMagicDefOf.TM_Hex_Pain || magicDef == TorannMagicDefOf.TM_Hex_MentalAssault)
                {
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * MagicData.GetSkill_Efficiency(TorannMagicDefOf.TM_Hex).level);
                }
                else if(Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
                {
                    CompAbilityUserMight compMight = Pawn.GetCompAbilityUserMight();
                    adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * compMight.MightData.GetSkill_Efficiency(TorannMagicDefOf.TM_Mimic).level);
                }
                else
                {
                    MagicPowerSkill mps = MagicData.GetSkill_Efficiency(magicDef);
                    if (mps != null)
                    {
                        adjustedManaCost *= 1f - (magicDef.efficiencyReductionPercent * mps.level);
                    }
                }
            }
            if (Pawn.health.hediffSet.HasHediff(HediffDef.Named("TM_SyrriumSenseHD"), false))
            {
                adjustedManaCost = adjustedManaCost * .9f;
            }
            if (mpCost != 1f)
            {
                if (magicDef == TorannMagicDefOf.TM_Explosion)
                {
                    adjustedManaCost = adjustedManaCost * (1f - ((1f - mpCost)/10f));
                }
                else
                {
                    adjustedManaCost = adjustedManaCost * mpCost;
                }
            }
            if (magicDef.abilityHediff != null && Pawn.health.hediffSet.HasHediff(magicDef.abilityHediff))
            {
                return 0f;
            }
            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.Faceless))
            {
                adjustedManaCost = 0;
            }
            if (Pawn.story.traits.HasTrait(TorannMagicDefOf.ChaosMage) || (customClass != null && customClass.classMageAbilities.Contains(TorannMagicDefOf.TM_ChaosTradition)))
            {
                adjustedManaCost = adjustedManaCost * 1.2f;
            }
            if (Pawn.Map != null && Pawn.Map.gameConditionManager.ConditionIsActive(TorannMagicDefOf.TM_ManaStorm))
            {
                return Mathf.Max(adjustedManaCost *.5f, 0f);
            }
            return Mathf.Max(adjustedManaCost, (.5f * magicDef.manaCost));
        }
        
        public void DoArcaneForging()
        {
            if (Pawn.CurJob.targetA.Thing.def.defName == "TableArcaneForge")
            {
                ArcaneForging = true;
                Job job = Pawn.CurJob;
                Thing forge = Pawn.CurJob.targetA.Thing;
                if (Pawn.Position == forge.InteractionCell && Pawn.jobs.curDriver.CurToilIndex >= 10)
                {
                    if (Find.TickManager.TicksGame % 20 == 0)
                    {
                        if (Mana.CurLevel >= .1f)
                        {
                            Mana.CurLevel -= .025f;
                            MagicUserXP += 4;
                            FleckMaker.ThrowSmoke(forge.DrawPos, forge.Map, Rand.Range(.8f, 1.2f));
                        }
                        else
                        {
                            Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                        }
                    }

                    ThingDef mote = null;
                    int rnd = Rand.RangeInclusive(0, 3);
                    switch (rnd)
                    {
                        case 0:
                            mote = TorannMagicDefOf.Mote_ArcaneFabricationA;
                            break;
                        case 1:
                            mote = TorannMagicDefOf.Mote_ArcaneFabricationB;
                            break;
                        case 2:
                            mote = TorannMagicDefOf.Mote_ArcaneFabricationC;
                            break;
                        case 3:
                            mote = TorannMagicDefOf.Mote_ArcaneFabricationD;
                            break;
                    }
                    Vector3 drawPos = forge.DrawPos;
                    drawPos.x += Rand.Range(-.05f, .05f);
                    drawPos.z += Rand.Range(-.05f, .05f);
                    TM_MoteMaker.ThrowGenericMote(mote, drawPos, forge.Map, Rand.Range(.25f, .4f), .02f, 0f, .01f, Rand.Range(-200, 200), 0, 0, forge.Rotation.AsAngle);
                }
            }
            else
            {
                ArcaneForging = false;
            }
        }
    }
}