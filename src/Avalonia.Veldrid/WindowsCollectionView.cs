using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Veldrid
{
    public class WindowsCollectionView : IReadOnlyList<VeldridTopLevelImpl>
    {
        internal List<VeldridTopLevelImpl> _list = new List<VeldridTopLevelImpl>(1);

        public int Count => _list.Count;

        public VeldridTopLevelImpl this[int index] => _list[index];

        public void Fetch(AvaloniaVeldridContext context)
        {
            if (context == null)
            {
                _list.Clear();
                return;
            }

            context.UpdateView(_list);
        }

        public IEnumerator<VeldridTopLevelImpl> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}