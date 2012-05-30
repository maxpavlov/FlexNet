namespace SenseNet.ContentRepository.Storage.Security
{
    public interface ISecurityMember
    {
        int Id { get; }
        string Path { get; }
    }
    public interface ISecurityContainer : ISecurityMember
    {
        //bool IsMember(ISecurityMember member);
    }
}
