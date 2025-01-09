using System.Collections.ObjectModel;


public interface ITaggable
{ 
    ReadOnlyCollection<string> tags { get; }
}
