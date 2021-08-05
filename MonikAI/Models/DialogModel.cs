using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MonikaOnDesktop.Models
{
    public class DialogModel
    {
        public XmlNode Node { get; set; }
        public DialogModel(XmlNode node)
        {
            this.Node = node;
        }
    }
}
