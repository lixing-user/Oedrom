using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IPieceSelectionExtensionRegistry
{
    void EnableExtension(string extensionName);
    void DisableExtension(string extensionName);
    bool IsExtensionEnabled(string extensionName);
    IEnumerable<string> GetAvailableExtensions();
}

public class PieceSelectionExtensionRegistry : IPieceSelectionExtensionRegistry
{
    private readonly List<IPieceSelectionExtension> _extensions;

    public PieceSelectionExtensionRegistry(List<IPieceSelectionExtension> extensions)
    {
        _extensions = extensions;
    }

    public void EnableExtension(string extensionName)
    {
        var extension = FindExtension(extensionName);
        if (extension != null)
        {
            extension.IsEnabled = true;
            Debug.Log($"[ExtensionRegistry] 啟用擴展: {extensionName}");
        }
    }

    public void DisableExtension(string extensionName)
    {
        var extension = FindExtension(extensionName);
        if (extension != null)
        {
            extension.IsEnabled = false;
            Debug.Log($"[ExtensionRegistry] 停用擴展: {extensionName}");
        }
    }

    public bool IsExtensionEnabled(string extensionName)
    {
        var extension = FindExtension(extensionName);
        return extension?.IsEnabled ?? false;
    }

    public IEnumerable<string> GetAvailableExtensions()
    {
        return _extensions.Select(e => e.ExtensionName);
    }

    private IPieceSelectionExtension FindExtension(string name)
    {
        return _extensions.FirstOrDefault(e => e.ExtensionName == name);
    }
}
