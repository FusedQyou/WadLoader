namespace Wadloader.Models
{
    public class Lump
    {
        public Lump(string name, int address, int size)
        {
            Name = name;
            Adress = address;
            Size = size;
        }

        public string Name { get; set; }
        public int Adress { get; set; }
        public int Size { get; set; }
    }
}