using System;

public class TextureNotFoundException : Exception
{
    public TextureNotFoundException(string message) : base(message)
    {
    }
}
