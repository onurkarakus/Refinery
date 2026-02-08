using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refinery.Core.Entities;

public class MailData
{
    public string Subject { get; set; }

    public string Body { get; set; }

    public string Sender { get; set; }
    
    public string Recipient { get; set; }
}
