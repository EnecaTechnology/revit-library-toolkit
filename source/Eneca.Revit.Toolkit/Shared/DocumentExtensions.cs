namespace Eneca.Revit.Toolkit.Shared;

public static class DocumentExtensions
{
    public static Element GetElement(this Document doc, long elementId)
    {
#if R24 || R25
		return doc.GetElement(new ElementId(elementId));
#else
        var id = Convert.ToInt32(elementId);
        return doc.GetElement(new ElementId(id));
#endif
    }
    public static bool Commit(this Document doc, string transactionName, Action action)
    {
        using Transaction t = new Transaction(doc, transactionName);
        t.Start();
        bool success;
        try
        {
            action?.Invoke();
            t.Commit();
            success = true;
        }
        finally
        {
            if (!t.HasEnded())
            {
                t.RollBack();
            }
        }
        return success;
    }
    public static string GetDocumentPath(this Document doc)
    {
        string path = "null";

        if (doc == null) return path;
        if (doc.IsWorkshared)
        {
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath());
        }

        return doc.PathName;
    }
}