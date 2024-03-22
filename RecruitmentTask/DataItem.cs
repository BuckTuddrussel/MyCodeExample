public class DataItem
{
	public enum CategoryType
	{
		RED,
		GREEN,
		BLUE
	}

	public readonly CategoryType Category;
	public readonly string Description;
	public readonly bool Special;

	public DataItem(CategoryType category, string description, bool special)
	{
		Category = category;
		Description = description;
		Special = special;
	}
}
