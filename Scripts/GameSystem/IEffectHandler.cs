using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.GameSystems
{
    public interface IEffectHandler
    {
        void ApplyEffect(AppliedEffect[] appliedEffects);
    }
}