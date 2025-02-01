using System.Collections.Generic;
using System.Linq;

public static class TagHierarchyBuilder
{
    public static TagTreeNode BuildTagHierarchy(List<GameplayTagInfo> tagInfos)
    {
        var root = new TagTreeNode("Root", "Root"); // Create a root node for all tags
        foreach (var tagInfo in tagInfos)
        {
            AddTagToHierarchy(root, tagInfo.Tag, root.FullTag);
        }
        return root;
    }

    private static void AddTagToHierarchy(TagTreeNode currentNode, string tag, string parentFullTag)
    {
        var parts = tag.Split('.'); // Split tag into components (e.g., "A.B.C" -> ["A", "B", "C"])
        var currentPart = parts[0];

        // Calculate the full tag for the current part
        var currentFullTag = currentNode.Tag == "Root" ? currentPart : $"{parentFullTag}.{currentPart}";

        // Check if a child node with this part already exists
        var childNode = currentNode.Children.FirstOrDefault(node => node.Tag == currentPart);
        if (childNode == null)
        {
            childNode = new TagTreeNode(currentPart, currentFullTag);
            currentNode.Children.Add(childNode);
        }

        // Recurse for the rest of the tag (if any parts remain)
        if (parts.Length > 1)
        {
            var remainingTag = string.Join('.', parts.Skip(1));
            AddTagToHierarchy(childNode, remainingTag, currentFullTag);
        }
    }
}


public class TagTreeNode
{
    public string Tag { get; set; }      // Node's tag (e.g., "C")
    public string FullTag { get; set; } // Node's full tag (e.g., "A.B.C")
    public List<TagTreeNode> Children { get; private set; }

    public TagTreeNode(string tag, string fullTag)
    {
        Tag = tag;
        FullTag = fullTag;
        Children = new List<TagTreeNode>();
    }
}
