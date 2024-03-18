using System.Collections;

internal interface IAttacherMovement : IPartController
{
    IEnumerator Attach(AttacheeComponent atachee);

    IEnumerator Detach();

    bool CanAttach(AttacheeComponent attachee);

    bool CanDetach();
}
