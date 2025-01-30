using System.Threading.Tasks;

public interface IAttacherMovement
{
    Task<bool> AttachTo(ConstructPart attacherPart);
}
