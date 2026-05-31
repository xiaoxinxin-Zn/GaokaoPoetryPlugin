namespace GaokaoPoetryPlugin.Models;

public class PoetryItem
{
    public string title { get; set; } = "";
    public string author { get; set; } = "";
    public string[] content { get; set; } = System.Array.Empty<string>();
    public string book { get; set; } = "其他";
}
