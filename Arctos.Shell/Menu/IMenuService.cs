using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Menu
{
    public interface IMenuService
    {
        public void AddNewMenu(string menuName);
        public T AddNode<T>(string menuName, string parentId, string id, int priority) where T : IMenuNodeVM;
    }
}
