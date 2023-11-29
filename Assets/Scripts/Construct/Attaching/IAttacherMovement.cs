using System.Collections;

interface IAttacherMovement : IPartController
{
    IEnumerator Attach(AttacheePartComponent atachee);
    IEnumerator Detach();
    bool CanAttach(AttacheePartComponent attachee);
    bool CanDetach();
}