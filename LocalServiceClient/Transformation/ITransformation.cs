using System.Collections.Generic;

public interface ITransformation
{
    IEnumerable<Dictionary<string, string>> Transform(string[] args);
}