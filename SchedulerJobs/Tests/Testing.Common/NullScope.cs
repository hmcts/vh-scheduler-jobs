using System;

namespace Testing.Common
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class NullScope : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope() {
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
