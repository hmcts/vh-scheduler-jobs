using System;

namespace Testing.Common
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class NullScope : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        public static NullScope Instance { get; } = new();

        private NullScope() {
        }

#pragma warning disable CA1816
        public void Dispose() {
#pragma warning restore CA1816
            throw new NotImplementedException();
        }
    }
}
