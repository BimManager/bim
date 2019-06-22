using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins
{
    class RenameException: Exception
    {
        public RenameException() { }
        public RenameException(string msg)
            :base(msg) { }
    }

    class UpdateRepositoryException: Exception
    {
        public UpdateRepositoryException() { }
        public UpdateRepositoryException(string msg)
            : base(msg) { }
    }
}
