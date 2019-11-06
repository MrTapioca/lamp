namespace Lamp.Interfaces
{
    public interface IKeyGenerator
    {
        string GenerateKey();
        string GenerateKey(int length);
    }
}