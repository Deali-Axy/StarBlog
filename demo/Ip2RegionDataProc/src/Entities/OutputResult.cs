using System.Collections.Generic;

namespace Ip2RegionDataProc.Entities;

public class OutputResult {
    public string Result { get; set; }
    public IEnumerable<string> Messages { get; set; }
}