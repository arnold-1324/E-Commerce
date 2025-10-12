namespace product_service.Utilits
{
    public class RestockItem : IComparable<RestockItem>
    {
        public string SKU { get; set; } = string.Empty;
        public int Stock { get; set; }

        public int CompareTo(RestockItem? other)
        {
            if (other == null) return 1;
            return Stock.CompareTo(other.Stock); 
        }
    }
    public class RestockHeap
    {
        public SortedSet<RestockItem> Heap { get; private set; } = new();

        public void AddOrUpdate(string sku, int stock)
        {
            var existing = Heap.FirstOrDefault(item => item.SKU == sku);
            if (existing != null)
            {
                Heap.Remove(existing);
            }
            Heap.Add(new RestockItem { SKU = sku, Stock = stock });
        }

        public RestockItem? PopLowestStock()
        {
            var item = Heap.Min;
            if (item != null)
            {
                Heap.Remove(item);
            }
            return item;
        }

        public RestockItem? PeekLowestStock()
        {
            return Heap.Min;
        }
    }



}
