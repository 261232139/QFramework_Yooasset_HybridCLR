using Game.Level.Data;

namespace Game.Level.Editor
{
    public interface ILevelGenerator
    {
        string GeneratorName { get; }
        LevelConfig Generate(LevelGenerationRequest request);
        bool Validate(LevelGenerationRequest request, out string error);
    }
}
