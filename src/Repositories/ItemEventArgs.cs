namespace UCode.Repositories
{
    public class ItemEventArgs<T>
    {
        public ItemEventArgs(T item, int position)
        {
            this.Item = item;
            this.Position = position;
        }

        public T Item
        {
            get;
        }
        public int Position
        {
            get;
        }
    }
}
