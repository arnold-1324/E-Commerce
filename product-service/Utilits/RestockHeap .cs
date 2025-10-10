namespace product_service.Utilits
{
    public class RestockItem : IComparable<RestockItem>
    {
        public string SKU { get; set; }
        public int Stock { get; set; }

        public int CompareTo(RestockItem other)
        {
            return Stock.CompareTo(other.Stock); 
        }
    }
    public class RestockHeap
    {
        private SortedSet<RestockItem> heap = new();

        public void AddOrUpdate(string sku, int stock)
        {
            var existing = heap.FirstOrDefault(item => item.SKU == sku);

            if (existing != null)
            {
                heap.Remove(existing);
            }
            heap.Add(new RestockItem { SKU=sku, Stock=stock });
        }
    public RestockItem PopLowestStock()
    {
        var item = heap.Min;
        if (item != null)
        {
            heap.Remove(item);
        }
        return item;
    }
    }



}
