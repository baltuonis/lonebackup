using System;
using System.IO;

namespace LoneBackup.App
{
    public class ObservableMemoryStream : MemoryStream
    {
        private Action<long, long> _callback;

        public ObservableMemoryStream(Action<long, long> callback) : base()
        {
            _callback = callback;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            _callback?.Invoke(Position, Length);
            return base.Read(array, offset, count);
        }
    }
}