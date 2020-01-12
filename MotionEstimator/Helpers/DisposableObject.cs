using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionEstimator.Helpers
{
    public abstract class DisposableObject
    {
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    OnDispose();
                }
                disposed = true;
            }
        }

        ~DisposableObject()
        {
            Dispose(false);
        }

        public abstract void OnDispose();
    }
}
