using System.Collections;

interface IAttacherMovement : IPartController
{
    IEnumerator Attach(AttacheeComponent atachee);
    IEnumerator Detach();
    bool CanAttach(AttacheeComponent attachee);
    bool CanDetach();
}