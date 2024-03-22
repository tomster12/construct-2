using System.Collections;

internal interface IAttacherMovement : IConstructMovement
{
    IEnumerator Attach(AttacheeComponent atachee);

    IEnumerator Detach();

    bool CanAttach(AttacheeComponent attachee);

    bool CanDetach();
}
