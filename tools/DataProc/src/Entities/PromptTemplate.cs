namespace DataProc.Entities;

public class PromptTemplate {
    public string Key { get; set; }
    public string Name { get; set; }
    public string Prompt { get; set; }

    public override string ToString() {
        return Name;
    }
}