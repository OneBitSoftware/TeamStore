namespace TeamStore.Keeper.Enums
{
    public enum Role
    {
        Owner = 1, // create project, archive project, edit project, create asset, edit asset, archive asset, update asset, read asset
        Editor = 2, // create asset, edit asset, archive asset, update asset, read asset
        Contributor = 3, //update asset, read asset
        Reader = 4 // read asset
    }
}
