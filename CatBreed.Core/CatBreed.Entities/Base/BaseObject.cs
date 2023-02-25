using System;
namespace CatBreed.Entities.Base
{
    [System.Serializable]
    public abstract class BaseObject : IDisposable
    {
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        protected object Copy()
        {
            return this.MemberwiseClone();
        }

        public virtual object DeepCopy()
        {
            return Copy();
        }
    }
}

