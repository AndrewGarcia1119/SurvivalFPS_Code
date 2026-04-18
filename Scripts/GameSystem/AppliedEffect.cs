using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.GameSystems
{
    [System.Serializable]
    public struct AppliedEffect : INetworkSerializable
    {
        public Effect type;
        public bool hasDuration;
        public float duration;
        [Tooltip("duration is infinite if value is at 0 or less")]
        public float maxDuration;
        [Tooltip("use this if you intend to apply an effect that gets stronger the more times you apply it. Only applies to effects with durations")]
        public bool stackable;
        [Tooltip("no stack limit if value is 0 or less")]
        public int maxStacks;
        [Tooltip("set this to 1 for default, set this higher if you want effect to add multiple stacks")]
        public int currentStacks;
        [HideInInspector]
        public ulong sourceId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref hasDuration);
            serializer.SerializeValue(ref duration);
            serializer.SerializeValue(ref maxDuration);
            serializer.SerializeValue(ref stackable);
            serializer.SerializeValue(ref maxStacks);
            serializer.SerializeValue(ref currentStacks);
            serializer.SerializeValue(ref sourceId);
        }
    }

}