namespace DotAgent.Core.Generator;

public enum GeneratorMode
{
    FunctionCalling = 0, // When model supports function calling.
    OnlyText = 1 // When model only supports text generation.
}