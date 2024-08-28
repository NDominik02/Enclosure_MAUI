using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BekeritesM.Model;

namespace BekeritesM.Persistence
{
    public class BekeritesFileDataAccess : IBekeritesDataAccess
    {
		// Könyvtár
		private String? _directory = String.Empty;
        public BekeritesFileDataAccess(String? saveDirectory = null)
        {
            _directory = saveDirectory;
        }

        public async Task<BekeritesTable> LoadAsync(String path)
        {
            if (!String.IsNullOrEmpty(_directory))
                path = Path.Combine(_directory, path);

            try
            {
				using (StreamReader reader = new StreamReader(path))
				{
					String line = await reader.ReadLineAsync() ?? String.Empty;
					String[] numbers = line.Split(' '); // beolvasunk egy sort, és szóköz mentén széttöredezzük
					Int32 tableSize = Int32.Parse(numbers[0]); //beolvassuk a tábla méretét
					Player currentPlayer = (Player)Int32.Parse(numbers[1]);
					BekeritesTable table = new BekeritesTable(tableSize);
					table.CurrentPlayer = currentPlayer;

					for (Int32 i = 0; i < tableSize; i++)
					{
						line = await reader.ReadLineAsync() ?? String.Empty;
						numbers = line.Split(' ');

						for (Int32 j = 0; j < tableSize; j++)
						{
							table.SetValue(i, j, (Player)Int32.Parse(numbers[j]));
						}
					}
					return table;
				}
			}
			catch
			{

				throw new BekeritesDataException();
			}
        }

		public async Task SaveAsync(String path, BekeritesTable table)
		{
            if (!String.IsNullOrEmpty(_directory))
                path = Path.Combine(_directory, path);

            try
            {
				using (StreamWriter writer = new StreamWriter(path)) //fájl megnyitása
                { 
					writer.Write(table.Size); //kiírjuk a tábla méretét
					await writer.WriteLineAsync(" " + (int)table.CurrentPlayer);
					for (Int32 i = 0; i < table.Size; i++)
					{
						for (Int32 j = 0; j < table.Size; j++)
						{
							await writer.WriteAsync((int)table[i, j] + " "); //kiírjuk az értékeket
						}
						await writer.WriteLineAsync();
					}
				}
			}
			catch
			{

				throw new BekeritesDataException();
			}
		}
    }
}
