using Unity.Netcode;
using UnityEngine;

public struct AttackInfo : INetworkSerializable
{
    public Teams team;
    public int primaryPower;
    public int secondaryPower;
    public string colour;
    public float lifetime;
    public float speed;
    public float aoeSize;
    public Vector2 direction;
    public Vector2 originPos;
    public Vector2 extraVelocity;

    public AttackInfo(Teams newTeam, int newPower, Vector2 newOriginPos, float newLifetime, string newColour = "#cccccc", float newSpeed = 0, float newAoeSize = 1, Vector2 newDirection = default, Vector2 newExtraVelocity = default, int newSecondaryPower = 0)
    {
        team = newTeam;
        primaryPower = newPower;
        secondaryPower = newSecondaryPower;
        colour = newColour;
        lifetime = newLifetime;
        speed = newSpeed;
        aoeSize = newAoeSize;
        direction = newDirection;
        originPos = newOriginPos;
        extraVelocity = newExtraVelocity;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref primaryPower);
        serializer.SerializeValue(ref secondaryPower);
        serializer.SerializeValue(ref colour, true);
        serializer.SerializeValue(ref lifetime);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref aoeSize);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref originPos);
        serializer.SerializeValue(ref extraVelocity);
    }
}
