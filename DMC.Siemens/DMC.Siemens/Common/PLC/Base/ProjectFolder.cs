using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public class ProjectFolder
    {
        public string Name { get; set; }
        public List<ProjectFolder> SubFolders { get; set; }
        public ProjectFolder ParentFolder { get; set; }

        public ProjectFolder()
        {
            SubFolders = new List<ProjectFolder>();
        }

        public string Path
        {
            get
            {
                if (ParentFolder != null)
                {
                    return ParentFolder.Path + "\\" + Name;
                }
                return Name;
            }
        }

    }
}
