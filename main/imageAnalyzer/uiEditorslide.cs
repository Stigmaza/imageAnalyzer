using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Web.UI;

namespace imageAnalyzer
{
    public class uiEditorslide : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                Panel panel = new Panel();
                TrackBar trackBar = new TrackBar();
                Label label = new Label();

                trackBar.Minimum = 0;
                trackBar.Maximum = ((clsProcessZItem)context.Instance).rangeMax(context.PropertyDescriptor.DisplayName);
                trackBar.Value = Convert.ToInt32(value);
                trackBar.TickFrequency = 10;
                trackBar.Dock = DockStyle.Top;

                label.Text = trackBar.Value.ToString();
                label.Dock = DockStyle.Bottom;
                                
                trackBar.ValueChanged += (s, e) =>
                {
                    label.Text = trackBar.Value.ToString();
                    context.PropertyDescriptor.SetValue(context.Instance, trackBar.Value);
                };

                trackBar.MouseUp += (s, e) =>
                {
                    editorService.CloseDropDown();
                };

                panel.Controls.Add(trackBar);
                panel.Controls.Add(label);

                editorService.DropDownControl(panel);

                return trackBar.Value;
            }

            return value;
        }
    }
}
