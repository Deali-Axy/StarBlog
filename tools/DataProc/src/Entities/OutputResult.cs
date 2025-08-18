using System.Collections.Generic;

namespace DataProc.Entities;

public class OutputResult {
    public string Result { get; set; }
    public IEnumerable<string> Messages { get; set; }
}