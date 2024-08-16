using System;
using System.Collections;

public interface IAttacherMovement
{
    IEnumerator EnumStartAttach(ConstructPart attacherPart, Action<bool> callback);
}
