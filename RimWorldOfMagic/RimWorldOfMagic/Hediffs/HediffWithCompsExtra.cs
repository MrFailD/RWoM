﻿using Verse;
using UnityEngine;

namespace TorannMagic
{
    [StaticConstructorOnStartup]
    public class HediffWithCompsExtra : HediffWithComps
    {
        public virtual string GizmoLabel => Label;
        public virtual float MaxSeverity => def.maxSeverity;

        public override string LabelInBrackets => this.Severity.ToString("F1");

        public override float Severity
        {
            get => base.Severity;
            set
            {
                severityInt = Mathf.Clamp(value, 0f, MaxSeverity);
            }
        }
    }
}
