public static class ExtensionUtil
{
    public static void SetIndex(this IdOwn owner)
    {
        owner.IndexId = IdManager.Instance.Next();
    }
}