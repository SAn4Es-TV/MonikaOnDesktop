using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MonikaOnDesktop.Models
{
    public class DialogModel : IDisposable
    {
        public XmlNode Node { get; set; }
        public DialogModel(XmlNode node)
        {
            this.Node = node;
        }

        void IDisposable.Dispose()
        {
            Node = null;
            GC.Collect();
            GC.SuppressFinalize(this);
            GC.ReRegisterForFinalize(this);
        }
    }
}
