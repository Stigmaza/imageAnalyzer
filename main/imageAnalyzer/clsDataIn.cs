using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    public class clsDataIn : clsDataPoint
    {
        public clsDataIn(clsProcessZItem parent, string name)
        {
            this.name = name;

            this.parent = parent;
            this.frame = new Mat();

            type = POINT_TYPE.IN;

            initGdi();
        }
    }
}
