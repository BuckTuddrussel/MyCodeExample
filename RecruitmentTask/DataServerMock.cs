using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Random = System.Random;

public class DataServerMock : IDataServer
{
	private readonly Random _random = new Random();

	private readonly List<DataItem> _items = new List<DataItem>(128);

	private readonly int _delayMin;
	private readonly int _delayMax;

	private static readonly string[] _names =
	{
		"lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing",
		"elit", "sed", "do", "eiusmod", "tempor", "incididunt", "ut", "labore",
		"et", "dolore", "magna", "aliqua"
	};

	public DataServerMock() : this(200,1200,15,30)
	{
	}

	public DataServerMock(int delayMin, int delayMax, int itemsCountMin, int itemsCountMax)
	{
		_delayMin = delayMin;
		_delayMax = delayMax;

		int count = _random.Next(itemsCountMin, itemsCountMax);
		for (int i = 0; i < count; i++)
		{
			AddDataItem();
		}	
	}

	public async Task<int> DataAvailable(CancellationToken ct)
	{
		await Task.Delay(_random.Next(_delayMin, _delayMax), ct);

		return _items.Count;
	}

	public async Task<IList<DataItem>> RequestData(int index, int count, CancellationToken ct)
	{
		await Task.Delay(_random.Next(_delayMin, _delayMax), ct);

		return _items.GetRange(index, count);
	}

	private void AddDataItem()
	{
		bool special = _random.Next(100) > 75;
		DataItem.CategoryType category = (DataItem.CategoryType)_random.Next(3);

		StringBuilder sb = new StringBuilder(128);
		for (int i = 0; i < _random.Next(2, 20); i++)
		{
			sb.Append(_names[_random.Next(_names.Length)]);
			sb.Append(" ");
		}
		
		_items.Add(new DataItem(category, sb.ToString(), special));
	}
}