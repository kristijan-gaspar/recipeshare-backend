namespace RecipeShare.Application.DTOs.Common;

public class CursorPagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
