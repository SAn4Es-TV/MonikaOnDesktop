using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop.Models
{
    class NamedDialogModel
    {
        public string[] Names { get; set; }
        public List<DialogModel> DM { get; set; }

        public NamedDialogModel(string[] names, List<DialogModel> dm)
        {
            this.DM = dm;
            this.Names = names;
        }

    }
}
