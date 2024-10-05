namespace Packages.ExcelParserPack.Runtime
{
    public interface ITable
    {
        void Load(string path);
        void Save(string path);
    }
}