using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMUtils
{
    public class BlockDeviceWriter
    {
        public static void WriteForBlockDevice(string path, List<int> binaryStream)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            System.IO.FileStream block = new System.IO.FileStream(path + "/0.block", System.IO.FileMode.Create);
            System.IO.BinaryWriter blockWriter = new System.IO.BinaryWriter(block);

            int blockSize = 4096;
            int blockCount = binaryStream.Count / blockSize + 1; // Plus one due to rounding down
            blockWriter.Write(blockCount);

            for (int val = 1; val < blockSize; val++)
            {
                blockWriter.Write(0);
            }
            blockWriter.Close();

            for (int codeBlock = 0; codeBlock < blockCount; codeBlock++)
            {
                int physicalBlock = codeBlock + 1;
                block = new System.IO.FileStream(path + "/" + physicalBlock.ToString() + ".block", System.IO.FileMode.Create);
                blockWriter = new System.IO.BinaryWriter(block);

                for (int indexWithinBlock = 0; indexWithinBlock < blockSize; indexWithinBlock++)
                {
                    int codeIndex = codeBlock * blockSize + indexWithinBlock;

                    if (codeIndex < binaryStream.Count)
                    {
                        blockWriter.Write(binaryStream[codeIndex]);
                    }
                    else
                    {
                        blockWriter.Write(0);
                    }
                }
                blockWriter.Close();
            }
        }
    }
}
