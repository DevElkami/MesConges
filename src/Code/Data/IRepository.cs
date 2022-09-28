
namespace WebApplicationConges.Data
{
    public interface IRepository
    {
        void Create();
        int Order { get; }
    }
}