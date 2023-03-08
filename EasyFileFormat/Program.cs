internal static class Program
{
    private struct TestFormat : FileSerializer
    {
        public int magic;
        public byte[] code;
    }

    private static void Main()
    {
        void Write()
        {
            FileStream fileStream = File.OpenWrite("out.testformat");
            BinaryWriter binaryWriter = new(fileStream);

            FileSerializer tf = new TestFormat() { magic = 50, code = new byte[] { 50, 40, 30, 20, 10, 5, 0 } };
            tf.WriteToStream(binaryWriter);

            binaryWriter.Flush();


            fileStream.Flush();
            binaryWriter.Close();
            fileStream.Close();
        }

        Write();

        void Read()
        {
            FileStream fileStream = File.OpenRead("out.testformat");
            BinaryReader binaryReader = new(fileStream);

            FileSerializer tf = new TestFormat();
            tf.ReadFromStream(binaryReader);

            fileStream.Close();

            Console.WriteLine(tf);
        }

        Read();
    }
}