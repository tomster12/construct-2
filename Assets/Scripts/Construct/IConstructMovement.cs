using UnityEngine;

public interface IConstructMovement : IPartController
{
    public void Move(Vector3 dir);

    public void Aim(Vector3 pos);
}
