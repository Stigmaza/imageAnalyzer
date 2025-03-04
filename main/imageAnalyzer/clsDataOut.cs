using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    public class clsDataOut : clsDataPoint
    {
        public clsDataOut(clsProcessZItem parent, string name)
        {
            this.name = name;
            this.parent = parent;
            this.frame = new Mat();

            dataTo = new List<clsDataPoint>();
            dataToGuid = new List<string>();
            dataToInName = new List<string>();

            type = POINT_TYPE.OUT;

            initGdi();
        }

        public void addDataTo(clsDataIn _dataTo)
        {
            for (int i = 0; i < dataTo.Count; i++)
            {
                if (dataTo[i] is clsDataIn)
                {
                    if (dataTo[i].parent.guid == _dataTo.parent.guid)
                    {
                        return;
                    }
                }
            }

            dataTo.Add(_dataTo);
        }

        public void addDataTo(string _dataTo, string _in)
        {
            dataToGuid.Add(_dataTo);
            dataToInName.Add(_in);
        }

        public void transData()
        {
            foreach (var item in dataTo)
            {
                if(item.frame != null)
                frame.CopyTo(item.frame);
            }
        }


    }
}
