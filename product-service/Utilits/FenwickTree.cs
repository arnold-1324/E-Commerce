namespace product_service.Utilits
{
    public class FenwickTree
    {

        private int[] tree;
        private int size;

        public FenwickTree(int size)
        {
            this.size = size;
            tree = new int[size + 1];
        }

        private void Update(int index, int delta)
        {
            index++; 
            while (index < tree.Length) 
            {
                tree[index] += delta;
                index += index & -index;
            }
        }


        private int Query(int index)
        {
            index++;
            int sum = 0;
            while(index>0)
            {
                sum+=tree[index];
                index-=index & -index;
            }
            return sum;
        }

        public void Add(int index,int value)
        {
            Update(index,value);
        }

        public int RangeQuery(int left,int right)
        {
            if(left>right) return 0;
            return Query(right)-Query(left-1);
        }
    }
}
