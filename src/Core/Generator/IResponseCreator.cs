using DotAgent.Core.Models;

namespace DotAgent.Core.Generator;

public interface IResponseCreator
{
    GenerationResponse CreateResponse(string response);
}