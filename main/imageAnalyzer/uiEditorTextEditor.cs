using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace imageAnalyzer
{
    internal class uiEditorTextEditor : UITypeEditor
    {
        // 편집 스타일을 드롭다운으로 설정
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        // 값 편집 로직 구현
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService != null)
            {
                // 사용자 정의 편집기 UI
                TextBox textBox = new TextBox
                {
                    Text = value as string
                };

                textBox.Multiline = true;

                textBox.Width = 300;
                textBox.Height = 200;

                // 드롭다운 컨트롤로 표시
                editorService.DropDownControl(textBox);

                value = textBox.Text;
            }

            return value;
        }
    }
}
